using System;
using UnityEngine;

// Organizing data
public class MainLoop : MonoBehaviour
{
    public CommPLC[] plc;

    public KeyCmd keyGantryCmd, keyTrolleyCmd, keySpreaderCmd,
           keyMM0Cmd, keyMM1Cmd, keyMM2Cmd, keyMM3Cmd;
    KeyCode keyCode20ft, keyCode40ft, keyCode45ft, keyCodeTwlLock, keyCodeTwlUnlock;

    [SerializeField] private GameObject cranePrefab;

    void Awake()
    {
        // Display Activated
        Debug.Log("Displays connected: " + Display.displays.Length);
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
        initKeyCmd();

        // InitCraneSpecVar
        InitCraneSpecVar();

        // Using PLC data
        if (GM.cmdWithPLC)
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

            //// PLC Connect
            // Check if listIP is not null
            if (GM.settingParams.listIP != null)
            {
                // connect
                plc = new CommPLC[GM.settingParams.listIP.Count];
                for (int i = 0; i < GM.settingParams.listIP.Count; i++)
                {
                    Debug.Log(GM.settingParams.listIP[i]);
                    plc[i] = new CommPLC(ip: GM.settingParams.listIP[i],
                        readDBNum: GM.readDBNum,
                        readLength: GM.readLength,
                        writeDBNum: GM.writeDBNum,
                        writeStartIdx: GM.writeStartIdx,
                        writeLength: GM.writeLength
                    );
                    plc[i].Connect();
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
        if (GM.cmdWithPLC)
        {
            for (int iCrane = 0; iCrane < GM.settingParams.listIP.Count; iCrane++)
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
        var rawData = plc[iCrane].ReadFromPLC();

        // Read float data
        GM.cmdGantryVelFWD[iCrane] = CommPLC.ReadFloatData(rawData, floatStartIdxGantryVelFWD);
        GM.cmdGantryVelBWD[iCrane] = CommPLC.ReadFloatData(rawData, floatStartIdxGantryVelBWD);
        GM.cmdTrolleyVel[iCrane] = CommPLC.ReadFloatData(rawData, floatStartIdxTrolleyVel);
        GM.cmdSpreaderVel[iCrane] = CommPLC.ReadFloatData(rawData, floatStartIdxSpreaderVel);
        GM.cmdMM0Vel[iCrane] = CommPLC.ReadFloatData(rawData, floatStartIdxMM0Vel);
        GM.cmdMM1Vel[iCrane] = CommPLC.ReadFloatData(rawData, floatStartIdxMM1Vel);
        GM.cmdMM2Vel[iCrane] = CommPLC.ReadFloatData(rawData, floatStartIdxMM2Vel);
        GM.cmdMM3Vel[iCrane] = CommPLC.ReadFloatData(rawData, floatStartIdxMM3Vel);

        // Read boolean data
        GM.cmdTwlLock[iCrane] = CommPLC.ReadBoolData(rawData, boolStartIdxTwistLock, boolStartPointTwlLock);
        GM.cmdTwlUnlock[iCrane] = CommPLC.ReadBoolData(rawData, boolStartIdxTwistLock, boolStartPointTwlUnlock);
    }

    public virtual void WriteUnitydataToPLC(int iCrane)
    {

        float testFloat = 123.0f;
        int startIdx = 0;
        plc[iCrane].WriteFloat(testFloat, startIdx);



        // byte boolByte = 0;  // init
        // CommPLC.WriteBool(true, 0, boolByte);
        // CommPLC.WriteBool(false, 1, boolByte);
        // CommPLC.WriteBool(false, 2, boolByte);

        // plc[iCrane].WriteByte(boolByte, 204);

        // write to PLC
        plc[iCrane].WriteToPLC();
    }

    void CmdKeyboard()
    {
        int iCrane = 0;

        // keyboard input
        if (Input.anyKeyDown)
        {
            // Gantry
            GM.cmdGantryVelFWD[iCrane] = keyGantryCmd.GetSpeed();
            GM.cmdGantryVelBWD[iCrane] = GM.cmdGantryVelFWD[iCrane];

            // Trolley, spreader
            GM.cmdTrolleyVel[iCrane] = keyTrolleyCmd.GetSpeed();
            GM.cmdSpreaderVel[iCrane] = keySpreaderCmd.GetSpeed();

            // Micro Motion
            GM.cmdMM0Vel[iCrane] = keyMM0Cmd.GetSpeed();
            GM.cmdMM1Vel[iCrane] = keyMM1Cmd.GetSpeed();
            GM.cmdMM2Vel[iCrane] = keyMM2Cmd.GetSpeed();
            GM.cmdMM3Vel[iCrane] = keyMM3Cmd.GetSpeed();

            // 20ft, 40ft, 45ft
            GM.cmd20ft[iCrane] = Input.GetKeyDown(keyCode20ft) ? true : GM.cmd20ft[iCrane];
            GM.cmd20ft[iCrane] = Input.GetKeyDown(keyCode40ft) ? false : GM.cmd20ft[iCrane];
            GM.cmd20ft[iCrane] = Input.GetKeyDown(keyCode45ft) ? false : GM.cmd20ft[iCrane];

            GM.cmd40ft[iCrane] = Input.GetKeyDown(keyCode20ft) ? false : GM.cmd40ft[iCrane];
            GM.cmd40ft[iCrane] = Input.GetKeyDown(keyCode40ft) ? true : GM.cmd40ft[iCrane];
            GM.cmd40ft[iCrane] = Input.GetKeyDown(keyCode45ft) ? false : GM.cmd40ft[iCrane];

            GM.cmd45ft[iCrane] = Input.GetKeyDown(keyCode20ft) ? false : GM.cmd45ft[iCrane];
            GM.cmd45ft[iCrane] = Input.GetKeyDown(keyCode40ft) ? false : GM.cmd45ft[iCrane];
            GM.cmd45ft[iCrane] = Input.GetKeyDown(keyCode45ft) ? true : GM.cmd45ft[iCrane];

            // Twist Lock
            GM.cmdTwlLock[iCrane] = Input.GetKeyDown(keyCodeTwlLock) ? true : GM.cmdTwlLock[iCrane];
            GM.cmdTwlLock[iCrane] = Input.GetKeyDown(keyCodeTwlUnlock) ? false : GM.cmdTwlLock[iCrane];
            GM.cmdTwlUnlock[iCrane] = Input.GetKeyDown(keyCodeTwlLock) ? false : GM.cmdTwlUnlock[iCrane];
            GM.cmdTwlUnlock[iCrane] = Input.GetKeyDown(keyCodeTwlUnlock) ? true : GM.cmdTwlUnlock[iCrane];
        }
    }

    void initKeyCmd()
    {
        // Initialize key commands with settings from GM
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
    }
}


public class KeyCmd
{
    float speedABS = 0f;
    float[] direction = new float[3] { -1f, 0f, 1f };
    int directionIdx = 1; // 0: BWD, 1: Stop, 2: FWD

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
        directionIdx = Mathf.Clamp(directionIdx, 0, 2);

        return speedABS * direction[directionIdx];
    }
}