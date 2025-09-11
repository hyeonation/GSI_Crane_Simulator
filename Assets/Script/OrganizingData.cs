using System;
using UnityEngine;


// Organizing data
public class OrganizingData : MonoBehaviour
{
    CommPLC[] plc;
    byte[] writeDB;


    GameObject[] cranes;
    KeyCmd keyGantryCmd, keyTrolleyCmd, keySpreaderCmd,
           keyMM0Cmd, keyMM1Cmd, keyMM2Cmd, keyMM3Cmd;

    [SerializeField] private GameObject cranePrefab;

    void Start()
    {

        // Debug.Log($"crane pos : {GameObject.Find("ARTG1").transform.position}");

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
            // // reset
            // GM.DestroyVar();

            // making crane
            // cranes = new GameObject[GM.settingParams.listIP.Count];

            // i = 1부터 시작. 기존 크레인은 유지.
            GameObject crane = GameObject.Find("Crane");
            // cranes[0] = 
            for (int i = 1; i < GM.settingParams.listIP.Count; i++)
            {
                GameObject craneObject = Instantiate(cranePrefab, GM.cranePOS[i], Quaternion.identity);
                craneObject.name = $"ARTG{i + 1}";
                craneObject.transform.SetParent(crane.transform);
            }

            // init var
            GM.InitVar();

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

    void WriteUnitydataToPLC(int iCrane)
    {

        float testFloat = 123.0f;
        Array.Copy(FloatToByteArr(testFloat), 0, writeDB, 0, 4);

        byte boolByte = 0;
        boolByte |= 1 << 0;
        boolByte |= 1 << 3;
        boolByte |= 1 << 4;

        // if (valve) boolByte |= 1 << 0; // DBX8.0
        // if (pump)  boolByte |= 1 << 1; // DBX8.1
        // if (alarm) boolByte |= 1 << 2; // DBX8.2

        writeDB[204] = boolByte;

        plc[iCrane].WriteToPLC(writeDB);
    }

    byte[] FloatToByteArr(float floatData)
    {
        // float -> byteArr
        byte[] bytes = BitConverter.GetBytes(floatData);

        // sync
        return reverseByteArr(bytes);
    }


    void ReadPLCdata(int iCrane)
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
        const int lengthFloatData = 4;

        // get byte array
        byte[] bytes = reverseByteArr(rawData[startIndex..(startIndex + lengthFloatData)]);

        // Convert byte array to float
        return BitConverter.ToSingle(bytes, 0); // Convert to float
    }

    byte[] reverseByteArr(byte[] byteArr)
    {

        byte[] output = new byte[byteArr.Length];

        // Reverse the byte order for little-endian to big-endian conversion
        // Assuming rawData is in little-endian format, we need to reverse it
        for (int i = 0; i < byteArr.Length; i++)
        {
            int revIdx = byteArr.Length - 1 - i; // Reverse index
            output[i] = byteArr[revIdx];
        }

        return output;
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
        if (Input.GetKeyDown(keyFWD)) directionIdx++;
        else if (Input.GetKeyDown(keyBWD)) directionIdx--;

        // Ensure directionIdx is within bounds
        directionIdx = Mathf.Clamp(directionIdx, 0, 2);

        return speedABS * direction[directionIdx];
    }
}




/// <summary>
/// write 코드 작성 시 참고
/// </summary>
/// 
/// <param name="iCrane"></param>
//     public void Write_PLC()
//     {
//         for (int i = 0; i < property.Crane.Length; i++)
//         {

//             //Crane Postion
//             if (property.rmgTransform[i].transform.rotation.y < 0)
//             {
//                 rmgAngle[i] = property.rmgTransform[i].transform.rotation.eulerAngles.y - 360;
//             }
//             else
//             {
//                 rmgAngle[i] = property.rmgTransform[i].transform.rotation.eulerAngles.y;
//             }
//             float[] DB_data = new float[]
//             {
//                 //Gantry
//                 rmgAngle[i],
//                 property.rmg_BTransform[i].transform.position.x,
//                 property.rmg_BTransform[i].transform.position.z,
//                 property.rmg_FTransform[i].transform.position.x,
//                 property.rmg_FTransform[i].transform.position.z,

//                 //Trolley
//                 property.trolleyTransform[i].transform.localPosition.x,

//                 //Spreader
//                 property.HoistTransform[i].transform.localPosition.x,
//                 OP.Hoist_pos[i],
//                 property.HoistTransform[i].transform.localPosition.z,

//                 NormalizeAngle(property.HoistTransform[i].transform.localEulerAngles.x),
//                 NormalizeAngle(property.HoistTransform[i].transform.localEulerAngles.y),
//                 NormalizeAngle(property.HoistTransform[i].transform.localEulerAngles.z),

//                 //MicroMotion
//                 property.MMtranform[i,0].localPosition.x,
//                 property.MMtranform[i,1].localPosition.z,
//                 property.MMtranform[i,2].localPosition.x,
//                 property.MMtranform[i,3].localPosition.z,

//             };



//             for (short j =  0; j < property.laserTransform.GetLength(1); j++)
//             {
//                 ALS_Data[j] = property.laserTransform[i, 0].GetComponent<Laser>().laserDistances[i, j];
//             }

//             for (short j = 0; j < 9; j++)
//             {
//                 SPSS_Data[j] = property.spssTransform[i, 0].GetComponent<SPSS>().Stack_F[j];
//                 SPSS_Data[j + 9] = property.spssTransform[i, 2].GetComponent<SPSS>().Stack_R[j];
//             }

//             float[] gap = new float[]
//             {
//                 property.spssTransform[i, 0].GetComponent<SPSS>().RightGap,
//                 property.spssTransform[i, 2].GetComponent<SPSS>().LeftGap,
//                 property.spssTransform[i, 1].GetComponent<SPSS>().R_FrontGap,
//                 property.spssTransform[i, 1].GetComponent<SPSS>().R_RearGap,
//                 property.spssTransform[i, 3].GetComponent<SPSS>().L_FrontGap,
//                 property.spssTransform[i, 3].GetComponent<SPSS>().L_RearGap,
//             };


//             float[] con_transform_Lock = new float[]
//             {
//                 //ConPos, ConAng
//                 property.Container_inf[i].GetComponent<GetContainerInf>().Con_PosX,
//                 property.Container_inf[i].GetComponent<GetContainerInf>().Con_PosY,
//                 NormalizeAngle(property.Container_inf[i].GetComponent<GetContainerInf>().Con_Ang),
//             };


//             float[] con_transform_Unlock = new float[]
//             {
//                 //ConPos, ConAng
//                 property.trolleyTransform[i].GetComponent<GetContainerInf>().Con_PosX,
//                 property.trolleyTransform[i].GetComponent<GetContainerInf>().Con_PosY,
//                 NormalizeAngle(property.trolleyTransform[i].GetComponent<GetContainerInf>().Con_Ang),
//             };

//             List<float> combine_data1 = new List<float>();
//             combine_data1.AddRange(DB_data);
//             combine_data1.AddRange(ALS_Data);

//             plc[i].Write($"DB{DB}.DBD46", combine_data1.ToArray());

//             List<float> combine_data2 = new List<float>();
//             combine_data2.AddRange(SPSS_Data);
//             combine_data2.AddRange(gap);

//             if (OP.Lock[i])
//             {
//                 combine_data2.AddRange(con_transform_Lock);
//             }
//             else
//             {
//                 combine_data2.AddRange(con_transform_Unlock);
//             }

//             if (ReadfloatValue[i, 1] != 0 || ReadfloatValue[i, 2] != 0 || OP.Unlock[i]) //ȣ�̽�Ʈ �����ϰ� ũ���� ��Ʈ�� �ӵ��� ���� ���� SPSS �� ���
//             {
//                 plc[i].Write($"DB{DB}.DBD134", combine_data2.ToArray());
//             }


//             //MicroMotion
//             if (OP.Feet20_ack[i])
//             {
//                 plc[i].Write($"DB{DB}.DBX32.0", 0);
//                 plc[i].Write($"DB{DB}.DBX254.4", 1);
//                 plc[i].Write($"DB{DB}.DBX254.5", 0);

//                 OP.Feet20_ack[i] = false;
//             }
//             else if (OP.Feet40_ack[i])
//             {
//                 plc[i].Write($"DB{DB}.DBX32.1", 0);
//                 plc[i].Write($"DB{DB}.DBX254.4", 0);
//                 plc[i].Write($"DB{DB}.DBX254.5", 1);

//                 OP.Feet40_ack[i] = false;
//             }

//             //Twist_Lock


//             if (OP.Lock[i])
//             {
//                 plc[i].Write($"DB{DB}.DBX254.2", 1);
//                 plc[i].Write($"DB{DB}.DBX254.3", 0);
//                 OP.Unlock[i] = false;
//             }
//             else if (OP.Unlock[i])
//             {
//                 plc[i].Write($"DB{DB}.DBX254.2", 0);
//                 plc[i].Write($"DB{DB}.DBX254.3", 1);
//                 OP.Lock[i] = false;
//             }




//             //land
//             if (property.CableComponent[i, 0].GetComponent<Cable>().loosenessScale != 0)
//             {
//                 plc[i].Write($"DB{DB}.DBX254.0", 1);

//                 if (OP.Land[i])
//                 {
//                     plc[i].Write($"DB{DB}.DBX254.1", 1);
//                 }
//                 else
//                 {
//                     plc[i].Write($"DB{DB}.DBX254.1", 0);
//                 }
//             }
//             else
//             {
//                 plc[i].Write($"DB{DB}.DBX254.0", 0);
//                 plc[i].Write($"DB{DB}.DBX254.1", 0);
//             }
//         }

//         for (int i = 0; i < property.Truck.Length; i++)
//         {
//             //Truck Angle
//             if (property.Truck[i].transform.rotation.y < 0)
//             {
//                 truckAngle[i] = property.Truck[i].transform.rotation.eulerAngles.y - 360;
//             }
//             else
//             {
//                 truckAngle[i] = property.Truck[i].transform.rotation.eulerAngles.y;
//             }

//             float[] truck_pos = new float[]
// {
//                 property.Truck[i].transform.position.x,
//                 property.Truck[i].transform.position.z,
//                 truckAngle[i]
//             };

//             plc[i].Write($"DB{DB}.DBD256", truck_pos);

//             //Truck
//             if (OP.fifthup[i])
//             {
//                 plc[i].Write($"DB{DB}.DBX268.0", 1);
//                 plc[i].Write($"DB{DB}.DBX268.1", 0);

//             }
//             else if (OP.fifthdown[i])
//             {
//                 plc[i].Write($"DB{DB}.DBX268.0", 0);
//                 plc[i].Write($"DB{DB}.DBX268.1", 1);
//             }
//         }
//     }