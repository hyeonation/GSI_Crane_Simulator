using System;
using System.Linq;
using UnityEngine;

// Organizing data
public class MainLoop : MonoBehaviour
{
    

    public KeyCmd keyGantryCmd, keyTrolleyCmd, keySpreaderCmd,
           keyMM0Cmd, keyMM1Cmd, keyMM2Cmd, keyMM3Cmd,
           keyTruckCmd;
    KeyCode keyCode20ft, keyCode40ft, keyCode45ft, keyCodeTwlLock, keyCodeTwlUnlock;

    [SerializeField] private GameObject cranePrefab;

    protected CraneDataBase craneData;
    void Awake()
    {
        // Display Activated
        // Debug.Log("Displays connected: " + Display.displays.Length);
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
            Debug.Log("Display 1 Activated");
        }
        if (Display.displays.Length > 2)
        {
            Display.displays[2].Activate();
            Debug.Log("Display 2 Activated");
        }

        // init KeyCmd
        GM.OnSelectTruck -= initKeyCmdTruck;
        GM.OnSelectTruck += initKeyCmdTruck;
        initKeyCmd();

        // InitCraneSpecVar
        InitCraneSpecVar();

        // Using PLC data
        if (GM.settingParams.cmdWithPLC)
        {
            //// IP 개수만큼 크레인 생성
            // i = 1부터 시작. 기존 크레인은 유지.
            GameObject crane = GameObject.Find("Crane");
            for (int i = 1; i < GM.settingParams.listIP.Count; i++)
            {
                GameObject craneObject = Instantiate(cranePrefab, GM.cranePOS[i], Quaternion.identity);
                craneObject.name = $"{GM.craneTypeStr}{i + 1}";
                craneObject.transform.SetParent(crane.transform);
            }

            //// PLC Connect,
            // Check if listIP is not null
            if (GM.settingParams.listIP != null)
            {
                // connect
                GM.plc = new CommPLC[GM.settingParams.listIP.Count];
                for (int i = 0; i < GM.settingParams.listIP.Count; i++)
                {
                    Debug.Log(GM.settingParams.listIP[i]);
                    GM.plc[i] = new CommPLC(ip: GM.settingParams.listIP[i],
                        readDBNum: GM.readDBNum,
                        readLength: GM.readLength,
                        writeDBNum: GM.writeDBNum,
                        writeStartIdx: GM.writeStartIdx,
                        writeLength: GM.writeLength
                    );
                    GM.plc[i].Connect();
                }
            }

            else
            {
                Debug.Log("GM.listIP is null. Please check the GameManager settings.");
            }
        }

        //// 생성된 Crane 대수만큼 배열크기 설정
        // init var
        GM.InitVar();
    }

    void Update()
    {
        // Using PLC data
        if (GM.settingParams.cmdWithPLC)
        {
            for (int iCrane = 0; iCrane < GM.plc.Length; iCrane++)
            {
                // Read PLC DB
                ReadPLCdata(iCrane);

                // Write PLC DB
                WriteUnitydataToPLC(iCrane);
            }
        }

        // Using Keyboard
        else
        {
            CmdKeyboard();
        }

        // GameMode 상관없이 항상 키입력 받는것
        AlwaysCmdKeyboard();

        // 시간 측정
        // Time.deltaTime: 프레임 간 시간 간격
        // Debug.Log($"loop time = {Time.deltaTime} sec");
    }

    public virtual void InitCraneSpecVar() { }

    public virtual void ReadPLCdata(int iCrane)
    {
        // DB start index
        const int floatStartIdxGantryVelBWD = 0;
        const int floatStartIdxGantryVelFWD = 4;
        const int floatStartIdxTrolleyVel = 8;
        const int floatStartIdxSpreaderVel = 12;
        const int floatStartIdxMM0Vel = 16;
        const int floatStartIdxMM1Vel = 20;
        const int floatStartIdxMM2Vel = 24;
        const int floatStartIdxMM3Vel = 28;

        const int boolStartIdxTwistLock = 34;
        const int boolStartPointTwlLock = 0;
        const int boolStartPointTwlUnlock = 1;

        // Read raw data from PLC
        var rawData = GM.plc[iCrane].ReadFromPLC();

        // Read float data
        GM.arrayCraneDataBase[iCrane].ReadData.gantryVelFWD = CommPLC.ReadFloatData(rawData, floatStartIdxGantryVelFWD);
        GM.arrayCraneDataBase[iCrane].ReadData.gantryVelBWD = CommPLC.ReadFloatData(rawData, floatStartIdxGantryVelBWD);
        GM.arrayCraneDataBase[iCrane].ReadData.trolleyVel = CommPLC.ReadFloatData(rawData, floatStartIdxTrolleyVel);
        GM.arrayCraneDataBase[iCrane].ReadData.spreaderVel = CommPLC.ReadFloatData(rawData, floatStartIdxSpreaderVel);
        GM.arrayCraneDataBase[iCrane].ReadData.MM0Vel = CommPLC.ReadFloatData(rawData, floatStartIdxMM0Vel);
        GM.arrayCraneDataBase[iCrane].ReadData.MM1Vel = CommPLC.ReadFloatData(rawData, floatStartIdxMM1Vel);
        GM.arrayCraneDataBase[iCrane].ReadData.MM2Vel = CommPLC.ReadFloatData(rawData, floatStartIdxMM2Vel);
        GM.arrayCraneDataBase[iCrane].ReadData.MM3Vel = CommPLC.ReadFloatData(rawData, floatStartIdxMM3Vel);

        // Read boolean data
        GM.arrayCraneDataBase[iCrane].ReadData.twlStatus.locked = CommPLC.ReadBoolData(rawData, boolStartIdxTwistLock, boolStartPointTwlLock);
        GM.arrayCraneDataBase[iCrane].ReadData.twlStatus.unlocked = CommPLC.ReadBoolData(rawData, boolStartIdxTwistLock, boolStartPointTwlUnlock);
    }

    public virtual void WriteUnitydataToPLC(int iCrane)
    {

        float testFloat = 123.0f;
        int startIdx = 0;
        GM.plc[iCrane].WriteFloat(testFloat, startIdx);


       

        // byte boolByte = 0;  // init
        // CommPLC.WriteBool(true, 0, boolByte);
        // CommPLC.WriteBool(false, 1, boolByte);
        // CommPLC.WriteBool(false, 2, boolByte);

        // plc[iCrane].WriteByte(boolByte, 204);

        // write to PLC
        GM.plc[iCrane].WriteToPLC();
    }

    void CmdKeyboard()
    {
        int iCrane = 0;

        // keyboard input
        if (Input.anyKeyDown)
        {
            // Gantry
            GM.arrayCraneDataBase[iCrane].ReadData.gantryVelFWD = keyGantryCmd.GetSpeed();
            GM.arrayCraneDataBase[iCrane].ReadData.gantryVelBWD = GM.arrayCraneDataBase[iCrane].ReadData.gantryVelFWD;

            // Trolley, spreader
            GM.arrayCraneDataBase[iCrane].ReadData.trolleyVel = keyTrolleyCmd.GetSpeed();
            GM.arrayCraneDataBase[iCrane].ReadData.spreaderVel = keySpreaderCmd.GetSpeed();

            // Micro Motion
            GM.arrayCraneDataBase[iCrane].ReadData.MM0Vel = keyMM0Cmd.GetSpeed();
            GM.arrayCraneDataBase[iCrane].ReadData.MM1Vel = keyMM1Cmd.GetSpeed();
            GM.arrayCraneDataBase[iCrane].ReadData.MM2Vel = keyMM2Cmd.GetSpeed();
            GM.arrayCraneDataBase[iCrane].ReadData.MM3Vel = keyMM3Cmd.GetSpeed();

            // 20ft, 40ft, 45ft
            GM.arrayCraneDataBase[iCrane].ReadData.sprdStatus.on20ft = Input.GetKeyDown(keyCode20ft) ? true : GM.arrayCraneDataBase[iCrane].ReadData.sprdStatus.on20ft;
            GM.arrayCraneDataBase[iCrane].ReadData.sprdStatus.on40ft = Input.GetKeyDown(keyCode40ft) ? false : GM.arrayCraneDataBase[iCrane].ReadData.sprdStatus.on20ft;
            GM.arrayCraneDataBase[iCrane].ReadData.sprdStatus.on45ft = Input.GetKeyDown(keyCode45ft) ? false : GM.arrayCraneDataBase[iCrane].ReadData.sprdStatus.on20ft;

            GM.arrayCraneDataBase[iCrane].ReadData.sprdStatus.on40ft = Input.GetKeyDown(keyCode20ft) ? false : GM.arrayCraneDataBase[iCrane].ReadData.sprdStatus.on40ft;
            GM.arrayCraneDataBase[iCrane].ReadData.sprdStatus.on40ft = Input.GetKeyDown(keyCode40ft) ? true : GM.arrayCraneDataBase[iCrane].ReadData.sprdStatus.on40ft;
            GM.arrayCraneDataBase[iCrane].ReadData.sprdStatus.on40ft = Input.GetKeyDown(keyCode45ft) ? false : GM.arrayCraneDataBase[iCrane].ReadData.sprdStatus.on40ft;
            GM.arrayCraneDataBase[iCrane].ReadData.sprdStatus.on45ft = Input.GetKeyDown(keyCode20ft) ? false : GM.arrayCraneDataBase[iCrane].ReadData.sprdStatus.on45ft;
            GM.arrayCraneDataBase[iCrane].ReadData.sprdStatus.on45ft = Input.GetKeyDown(keyCode40ft) ? false : GM.arrayCraneDataBase[iCrane].ReadData.sprdStatus.on45ft;
            GM.arrayCraneDataBase[iCrane].ReadData.sprdStatus.on45ft = Input.GetKeyDown(keyCode45ft) ? true : GM.arrayCraneDataBase[iCrane].ReadData.sprdStatus.on45ft;

            // Twist Lock
            GM.arrayCraneDataBase[iCrane].ReadData.twlStatus.locked = Input.GetKeyDown(keyCodeTwlLock) ? true : GM.arrayCraneDataBase[iCrane].ReadData.twlStatus.locked;
            GM.arrayCraneDataBase[iCrane].ReadData.twlStatus.locked = Input.GetKeyDown(keyCodeTwlUnlock) ? false : GM.arrayCraneDataBase[iCrane].ReadData.twlStatus.locked;
            GM.arrayCraneDataBase[iCrane].ReadData.twlStatus.unlocked = Input.GetKeyDown(keyCodeTwlLock) ? false : GM.arrayCraneDataBase[iCrane].ReadData.twlStatus.unlocked;
            GM.arrayCraneDataBase[iCrane].ReadData.twlStatus.unlocked = Input.GetKeyDown(keyCodeTwlUnlock) ? true : GM.arrayCraneDataBase[iCrane].ReadData.twlStatus.unlocked;
        }
    }
    
    // GameMode 상관없이 항상 키입력 받는것
    void AlwaysCmdKeyboard()
    {
        // Truck
        GM.cmdTruckVel = keyTruckCmd.GetSpeed();
    }

    void initKeyCmd()
    {
        // Initialize key commands with settings from GM
        // crane
        keyGantryCmd = new KeyCmd(GM.settingParams.keyGantrySpeed, KeyCode.D, KeyCode.A);
        keyTrolleyCmd = new KeyCmd(GM.settingParams.keyTrolleySpeed, KeyCode.S, KeyCode.W);
        keySpreaderCmd = new KeyCmd(GM.settingParams.keySpreaderSpeed, KeyCode.R, KeyCode.F);
        keyMM0Cmd = new KeyCmd(GM.settingParams.keyMMSpeed, KeyCode.T, KeyCode.G);
        keyMM1Cmd = new KeyCmd(GM.settingParams.keyMMSpeed, KeyCode.Y, KeyCode.H);
        keyMM2Cmd = new KeyCmd(GM.settingParams.keyMMSpeed, KeyCode.U, KeyCode.J);
        keyMM3Cmd = new KeyCmd(GM.settingParams.keyMMSpeed, KeyCode.I, KeyCode.K);

        keyCode20ft = KeyCode.Alpha1;
        keyCode40ft = KeyCode.Alpha2;
        keyCode45ft = KeyCode.Alpha3;
        keyCodeTwlLock = KeyCode.Q;
        keyCodeTwlUnlock = KeyCode.E;

        // truck
        initKeyCmdTruck();
    }

    void initKeyCmdTruck()
    {
        keyTruckCmd = new KeyCmd(GM.settingParams.keyTruckSpeed, KeyCode.UpArrow, KeyCode.DownArrow);
    }

    void Destroy()
    {
        GM.OnSelectTruck -= initKeyCmd;
    }




}


public class KeyCmd
{
    float speedABS = 0f;
    float[] direction = new float[5] { -1f, -0.1f, 0f, 0.1f, 1f };
    int directionIdx = 2; // 0: BWD, 1: Slow BWD, 2: Stop, 3: Slow FWD, 4: FWD

    KeyCode keyFWD, keyBWD;

    public KeyCmd(float speedABS, KeyCode keyFWD, KeyCode keyBWD)
    {
        this.speedABS = speedABS;
        this.keyFWD = keyFWD;
        this.keyBWD = keyBWD;
    }

    public float GetSpeed()
    {
        if (Input.GetKeyDown(keyFWD)) directionIdx++;
        else if (Input.GetKeyDown(keyBWD)) directionIdx--;

        // Ensure directionIdx is within bounds
        directionIdx = Mathf.Clamp(directionIdx, 0, direction.Length - 1);

        return speedABS * direction[directionIdx];
    }
}