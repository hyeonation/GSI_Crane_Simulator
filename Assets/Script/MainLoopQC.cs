using System;
using UnityEngine;


// Organizing data
public class MainLoopQC : MonoBehaviour
{
    CommPLC[] plc;

    KeyCmd keyGantryCmd, keyTrolleyCmd, keySpreaderCmd,
           keyMM0Cmd, keyMM1Cmd, keyMM2Cmd, keyMM3Cmd;

    [SerializeField] private GameObject cranePrefab;

    void Start()
    {

        // Initialize key commands with settings from GM
        keyGantryCmd = new KeyCmd(GM.settingParams.keyGantrySpeed, KeyCode.Q, KeyCode.A);
        keyTrolleyCmd = new KeyCmd(GM.settingParams.keyTrolleySpeed, KeyCode.W, KeyCode.S);
        keySpreaderCmd = new KeyCmd(GM.settingParams.keySpreaderSpeed, KeyCode.E, KeyCode.D);
        keyMM0Cmd = new KeyCmd(GM.settingParams.keyMMSpeed, KeyCode.R, KeyCode.F);
        keyMM1Cmd = new KeyCmd(GM.settingParams.keyMMSpeed, KeyCode.T, KeyCode.G);
        keyMM2Cmd = new KeyCmd(GM.settingParams.keyMMSpeed, KeyCode.Y, KeyCode.H);
        keyMM3Cmd = new KeyCmd(GM.settingParams.keyMMSpeed, KeyCode.U, KeyCode.J);

        // Using PLC data
        if (GM.cmdWithPLC)
        {

            //// IP 개수만큼 크레인 생성
            // i = 1부터 시작. 기존 크레인은 유지.
            GameObject crane;
            string craneType;

            craneType = "QC";
            crane = GameObject.Find("Crane");
            for (int i = 1; i < GM.settingParams.listIP.Count; i++)
            {
                GameObject craneObject = Instantiate(cranePrefab, GM.cranePOS[i], Quaternion.identity);
                craneObject.name = $"{craneType}{i + 1}";
                craneObject.transform.SetParent(crane.transform);
            }

            // init var
            GM.InitVar();

            //// PLC Connect
            // Check if listIP is not null
            if (GM.settingParams.listIP != null)
            {
                // connect
                plc = new CommPLC[GM.settingParams.listIP.Count];
                for (int i = 0; i < GM.settingParams.listIP.Count; i++)
                {
                    plc[i] = new CommPLC(ip: GM.settingParams.listIP[i],
                        readDBNum: 9101,
                        readLength: 542,
                        writeDBNum: 9101,
                        writeStartIdx: 542,
                        writeLength: 566 - 542  // 24 byte
                    );

                    plc[i].Connect();
                }
            }

            else
            {
                Debug.Log("GM.listIP is null. Please check the GameManager settings.");
            }
        }
    }

    void Update()
    {
        // Using PLC data
        if (GM.cmdWithPLC)
        {
            for (int iCrane = 0; iCrane < GM.settingParams.listIP.Count; iCrane++)
            {
                // Read PLC DB
                plc[iCrane].ReadPLCdataQC(iCrane);

                // Write PLC DB
                plc[iCrane].WriteUnitydataToPLCQC();
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

            // 20ft, 40ft
            GM.cmd20ft[iCrane] = Input.GetKeyDown(KeyCode.Z) ? true : GM.cmd20ft[iCrane];
            GM.cmd20ft[iCrane] = Input.GetKeyDown(KeyCode.X) ? false : GM.cmd20ft[iCrane];
            GM.cmd40ft[iCrane] = Input.GetKeyDown(KeyCode.Z) ? false : GM.cmd40ft[iCrane];
            GM.cmd40ft[iCrane] = Input.GetKeyDown(KeyCode.X) ? true : GM.cmd40ft[iCrane];

            // Twist Lock
            GM.cmdTwlLock[iCrane] = Input.GetKeyDown(KeyCode.C) ? true : GM.cmdTwlLock[iCrane];
            GM.cmdTwlLock[iCrane] = Input.GetKeyDown(KeyCode.V) ? false : GM.cmdTwlLock[iCrane];
            GM.cmdTwlUnlock[iCrane] = Input.GetKeyDown(KeyCode.C) ? false : GM.cmdTwlUnlock[iCrane];
            GM.cmdTwlUnlock[iCrane] = Input.GetKeyDown(KeyCode.V) ? true : GM.cmdTwlUnlock[iCrane];
        }
    }
}
