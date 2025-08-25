using System;
using UnityEngine;


// Organizing data
public class OrganizingData : MonoBehaviour
{
    CommPLC[] plc;

    KeyCmd keyGantryCmd, keyTrolleyCmd, keySpreaderCmd,
           keyMM0Cmd, keyMM1Cmd, keyMM2Cmd, keyMM3Cmd;

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
            // Check if listIP is not null
            if (GM.settingParams.listIP != null)
            {
                // connect
                plc = new CommPLC[GM.settingParams.listIP.Count];
                for (int i = 0; i < GM.settingParams.listIP.Count; i++)
                {
                    plc[i] = new CommPLC(ip: GM.settingParams.listIP[i]);
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
                ReadPLCdata(iCrane);

                // Write PLC DB
                // plc[i].WriteToPLC();
            }
        }

        // Using Keyboard
        else
        {
            CmdKeyboard();
        }
        // 시간 측정
        // Time.deltaTime: 프레임 간 시간 간격
        Debug.Log($"loop time = {Time.deltaTime} sec");

    }

    void ReadPLCdata(int iCrane)
    {
        const int floatStartIdxGantryVelBWD = 0;
        const int floatStartIdxGantryVelFWD = 4;
        const int floatStartIdxTrolleyVel = 8;
        const int floatStartIdxSpreaderVel = 12;
        const int floatStartIdxMM0Vel = 16;
        const int floatStartIdxMM1Vel = 20;
        const int floatStartIdxMM2Vel = 24;
        const int floatStartIdxMM3Vel = 28;

        const int boolStartIdxTwistLock = 34;
        const int boolBitTwlLock = 0;
        const int boolBitTwlUnlock = 1;

        // Read raw data from PLC
        var rawData = plc[iCrane].ReadToPLC();

        // Read float data
        GM.cmdGantryVelFWD[iCrane] = ReadFloatData(rawData, floatStartIdxGantryVelFWD);
        GM.cmdGantryVelBWD[iCrane] = ReadFloatData(rawData, floatStartIdxGantryVelBWD);
        GM.cmdTrolleyVel[iCrane] = ReadFloatData(rawData, floatStartIdxTrolleyVel);
        GM.cmdSpreaderVel[iCrane] = ReadFloatData(rawData, floatStartIdxSpreaderVel);
        GM.cmdMM0Vel[iCrane] = ReadFloatData(rawData, floatStartIdxMM0Vel);
        GM.cmdMM1Vel[iCrane] = ReadFloatData(rawData, floatStartIdxMM1Vel);
        GM.cmdMM2Vel[iCrane] = ReadFloatData(rawData, floatStartIdxMM2Vel);
        GM.cmdMM3Vel[iCrane] = ReadFloatData(rawData, floatStartIdxMM3Vel);

        // Read boolean data
        GM.cmdTwlLock[iCrane] = ReadBoolData(rawData, boolStartIdxTwistLock, boolBitTwlLock);
        GM.cmdTwlUnlock[iCrane] = ReadBoolData(rawData, boolStartIdxTwistLock, boolBitTwlUnlock);
    }

    float ReadFloatData(byte[] rawData, int startIndex)
    {

        // float 4 bytes
        byte[] bytes = new byte[4];

        // Reverse the byte order for little-endian to big-endian conversion
        // Assuming rawData is in little-endian format, we need to reverse it
        for (int i = 0; i < 4; i++)
        {
            int revIdx = 3 - i; // Reverse index
            bytes[i] = rawData[startIndex + revIdx];
        }

        // Convert byte array to float
        return BitConverter.ToSingle(bytes, 0); // Convert to float
    }

    bool ReadBoolData(byte[] rawData, int startIndex, int bitIndex)
    {
        // Check if the bit at bitIndex is set
        return (rawData[startIndex] & (1 << bitIndex)) != 0;
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

public class KeyCmd
{
    float speedABS = 0f;
    float[] direction = new float[3] { -1f, 0f, 1f };
    int directionIdx = 1; // 0: BWD, 1: Stop, 2: FWD
    
    KeyCode keyFWD;
    KeyCode keyBWD;

    public KeyCmd(float speedABS, KeyCode keyFWD, KeyCode keyBWD)
    {
        this.speedABS = speedABS;
        this.keyFWD = keyFWD;
        this.keyBWD = keyBWD;
    }

    public float GetSpeed()
    {
        if      (Input.GetKeyDown(keyFWD)) directionIdx++;
        else if (Input.GetKeyDown(keyBWD)) directionIdx--;

        // Ensure directionIdx is within bounds
        directionIdx = Mathf.Clamp(directionIdx, 0, 2);

        return speedABS * direction[directionIdx];
    }
}