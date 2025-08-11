using System;
using UnityEngine;

// Organizing data
public class OrganizingData : MonoBehaviour
{
    GM gm;
    CommPLC[] plc;

    float keyGantrySpeed = 0.5f;
    float keyTrolleySpeed = 0.5f;
    float keySpreaderSpeed = 0.5f;
    float keyMMSpeed = 0.05f;

    bool toggleQ = false;
    bool toggleA = false;
    bool toggleW = false;
    bool toggleS = false;
    bool toggleE = false;
    bool toggleD = false;
    bool toggleR = false;
    bool toggleF = false;
    bool toggleT = false;
    bool toggleG = false;
    bool toggleY = false;
    bool toggleH = false;
    bool toggleU = false;
    bool toggleJ = false;

    void Start()
    {
        // init variables
        gm = GameObject.Find("GameManager").GetComponent<GM>();

        // Using PLC data
        if (gm.cmdWithPLC)
        {
            // Check if listIP is not null
            if (gm.listIP != null)
            {
                // init plc array
                plc = new CommPLC[gm.listIP.Count];

                // connect
                for (int i = 0; i < gm.listIP.Count; i++)
                {
                    plc[i] = new CommPLC(ip: gm.listIP[i]);
                    plc[i].Connect();
                }
            }

            else
            {
                Debug.Log("입력된 IP가 없습니다.");
            }
        }
    }

    void Update()
    {
        // Debug.Log($"loop time = {Time.deltaTime} sec");

        // Using PLC data
        if (gm.cmdWithPLC)
        {
            for (int i = 0; i < gm.listIP.Count; i++)
            {
                // Read PLC DB
                ReadPLCdata();

                // Write PLC DB
                // 읽고 쓰는 것 동시에.
                // 읽고 반영하고 쓰는 것과 크게 차이 없을 거라 봐서.
                // plc[i].WriteToPLC();
            }
        }

        // Using Keyboard
        else
        {
            CmdKeyboard();
        }
            
    }

    void ReadPLCdata()
    {
        // PLC에서 읽어온 float data idx
        const int floatStartIdxGantryVelBWD = 0;
        const int floatStartIdxGantryVelFWD = 4;
        const int floatStartIdxTrolleyVel = 8;
        const int floatStartIdxSpreaderVel = 12;
        const int floatStartIdxMM0Vel = 16;
        const int floatStartIdxMM1Vel = 20;
        const int floatStartIdxMM2Vel = 24;
        const int floatStartIdxMM3Vel = 28;

        // PLC에서 읽어온 boolean data idx
        const int boolStartIdxTwistLock = 34;
        const int boolBitTwlLock = 0;
        const int boolBitTwlUnlock = 1;

        for (int iCrane = 0; iCrane < plc.Length; iCrane++)
        {
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
    }

    float ReadFloatData(byte[] rawData, int startIndex) {

        // 4바이트씩 읽어서 float로 변환
        byte[] bytes = new byte[4];

        // 값 뒤집어야 정상 출력
        for (int i = 0; i < 4; i++)
        {
            int revIdx = 3 - i; // 역순으로 읽기
            bytes[i] = rawData[startIndex + revIdx];
        }

        // Convert byte array to float
        return BitConverter.ToSingle(bytes, 0); // 0은 byte 시작 값;
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
            // toggle boolean
            toggleQ = Input.GetKeyDown(KeyCode.Q) ? !toggleQ : toggleQ;
            toggleA = Input.GetKeyDown(KeyCode.A) ? !toggleA : toggleA;
            toggleW = Input.GetKeyDown(KeyCode.W) ? !toggleW : toggleW;
            toggleS = Input.GetKeyDown(KeyCode.S) ? !toggleS : toggleS;
            toggleE = Input.GetKeyDown(KeyCode.E) ? !toggleE : toggleE;
            toggleD = Input.GetKeyDown(KeyCode.D) ? !toggleD : toggleD;
            toggleR = Input.GetKeyDown(KeyCode.R) ? !toggleR : toggleR;
            toggleF = Input.GetKeyDown(KeyCode.F) ? !toggleF : toggleF;
            toggleT = Input.GetKeyDown(KeyCode.T) ? !toggleT : toggleT;
            toggleG = Input.GetKeyDown(KeyCode.G) ? !toggleG : toggleG;
            toggleY = Input.GetKeyDown(KeyCode.Y) ? !toggleY : toggleY;
            toggleH = Input.GetKeyDown(KeyCode.H) ? !toggleH : toggleH;
            toggleU = Input.GetKeyDown(KeyCode.U) ? !toggleU : toggleU;
            toggleJ = Input.GetKeyDown(KeyCode.J) ? !toggleJ : toggleJ;

            // 버튼 하나만 동작하도록
            toggleQ = toggleQ && toggleA && Input.GetKeyDown(KeyCode.A) ? false : toggleQ;
            toggleA = toggleQ && toggleA && Input.GetKeyDown(KeyCode.Q) ? false : toggleA;

            toggleW = toggleW && toggleS && Input.GetKeyDown(KeyCode.S) ? false : toggleW;
            toggleS = toggleW && toggleS && Input.GetKeyDown(KeyCode.W) ? false : toggleS;

            toggleE = toggleE && toggleD && Input.GetKeyDown(KeyCode.D) ? false : toggleE;
            toggleD = toggleE && toggleD && Input.GetKeyDown(KeyCode.E) ? false : toggleD;

            toggleR = toggleR && toggleF && Input.GetKeyDown(KeyCode.F) ? false : toggleR;
            toggleF = toggleR && toggleF && Input.GetKeyDown(KeyCode.R) ? false : toggleF;

            toggleT = toggleT && toggleG && Input.GetKeyDown(KeyCode.G) ? false : toggleT;
            toggleG = toggleT && toggleG && Input.GetKeyDown(KeyCode.T) ? false : toggleG;

            toggleY = toggleY && toggleH && Input.GetKeyDown(KeyCode.H) ? false : toggleY;
            toggleH = toggleY && toggleH && Input.GetKeyDown(KeyCode.Y) ? false : toggleH;

            toggleU = toggleU && toggleJ && Input.GetKeyDown(KeyCode.J) ? false : toggleU;
            toggleJ = toggleU && toggleJ && Input.GetKeyDown(KeyCode.U) ? false : toggleJ;

            // input velocity
            GM.cmdGantryVelFWD[iCrane] = toggleQ ? keyGantrySpeed : 0;
            GM.cmdGantryVelFWD[iCrane] = toggleA ? -keyGantrySpeed : GM.cmdGantryVelFWD[iCrane];

            GM.cmdGantryVelBWD[iCrane] = toggleQ ? keyGantrySpeed : 0;
            GM.cmdGantryVelBWD[iCrane] = toggleA ? -keyGantrySpeed : GM.cmdGantryVelBWD[iCrane];

            GM.cmdTrolleyVel[iCrane] = toggleW ? keyTrolleySpeed : 0;
            GM.cmdTrolleyVel[iCrane] = toggleS ? -keyTrolleySpeed : GM.cmdTrolleyVel[iCrane];

            GM.cmdSpreaderVel[iCrane] = toggleE ? keySpreaderSpeed : 0;
            GM.cmdSpreaderVel[iCrane] = toggleD ? -keySpreaderSpeed : GM.cmdSpreaderVel[iCrane];

            GM.cmdMM0Vel[iCrane] = toggleR ? keyMMSpeed : 0;
            GM.cmdMM0Vel[iCrane] = toggleF ? -keyMMSpeed : GM.cmdMM0Vel[iCrane];
            GM.cmdMM1Vel[iCrane] = toggleT ? keyMMSpeed : 0;
            GM.cmdMM1Vel[iCrane] = toggleG ? -keyMMSpeed : GM.cmdMM1Vel[iCrane];
            GM.cmdMM2Vel[iCrane] = toggleY ? keyMMSpeed : 0;
            GM.cmdMM2Vel[iCrane] = toggleH ? -keyMMSpeed : GM.cmdMM2Vel[iCrane];
            GM.cmdMM3Vel[iCrane] = toggleU ? keyMMSpeed : 0;
            GM.cmdMM3Vel[iCrane] = toggleJ ? -keyMMSpeed : GM.cmdMM3Vel[iCrane];

            GM.cmd20ft[iCrane] = Input.GetKeyDown(KeyCode.Z) ? true : GM.cmd20ft[iCrane];
            GM.cmd20ft[iCrane] = Input.GetKeyDown(KeyCode.X) ? false : GM.cmd20ft[iCrane];

            GM.cmd40ft[iCrane] = Input.GetKeyDown(KeyCode.Z) ? false : GM.cmd40ft[iCrane];
            GM.cmd40ft[iCrane] = Input.GetKeyDown(KeyCode.X) ? true : GM.cmd40ft[iCrane];

            GM.cmdTwlLock[iCrane] = Input.GetKeyDown(KeyCode.C) ? true : GM.cmdTwlLock[iCrane];
            GM.cmdTwlLock[iCrane] = Input.GetKeyDown(KeyCode.V) ? false : GM.cmdTwlLock[iCrane];
            GM.cmdTwlUnlock[iCrane] = Input.GetKeyDown(KeyCode.C) ? false : GM.cmdTwlUnlock[iCrane];
            GM.cmdTwlUnlock[iCrane] = Input.GetKeyDown(KeyCode.V) ? true : GM.cmdTwlUnlock[iCrane];
        }
    }
    
}