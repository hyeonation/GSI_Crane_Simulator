using UnityEngine;


///////// 나중에 이 부분 따로 추가하기
/// </summary>
// // Read Bool
// for (int j = 0; j < 3; j++)
// {
//     ftcom[i, j] = ((data[32] & (1 << j)) != 0); // 각 비트를 확인하고 출력
//     if (j < 2)
//     {
//         TwistLockcom[i, j] = ((data[34] & (1 << j)) != 0);
//         trailercom[i, j] = ((data[44] & (1 << j)) != 0);
//         conelock[i,j] = ((data[44] & (1 << j+2)) != 0);
//     }
// }
// }


// Organizing data
public class OrganizingData : MonoBehaviour
{
    CommPLC[] plc;
    float[,] ReadfloatValue;

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
        
        // Using PLC data
        if (GM.cmdWithPLC)
        {
            if (GM.listIP != null)
            {
                // connect
                for (int i = 0; i < GM.listIP.Count; i++)
                {
                    plc[i] = new CommPLC(GM.listIP[i]);
                    // plc[i].Connect();
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

        Debug.Log($"loop time = {Time.deltaTime} sec");

        // Using PLC data
        if (GM.cmdWithPLC)
        {
            for (int i = 0; i < GM.listIP.Count; i++)
            {
                // Read PLC DB
                // data = plc[i].ReadToPLC();

                // Write PLC DB
                // 읽고 쓰는 것 동시에.
                // 읽고 반영하고 쓰는 것과 크게 차이 없을 거라 봐서.
                // plc[i].WriteToPLC();
            }
        }

        // Using Keyboard
        else
        {
            int iCrane = 0;

            if (Input.anyKeyDown)
            {
                // toggle boolean
                toggleQ = (Input.GetKeyDown(KeyCode.Q)) ? !toggleQ : toggleQ;
                toggleA = (Input.GetKeyDown(KeyCode.A)) ? !toggleA : toggleA;
                toggleW = (Input.GetKeyDown(KeyCode.W)) ? !toggleW : toggleW;
                toggleS = (Input.GetKeyDown(KeyCode.S)) ? !toggleS : toggleS;
                toggleE = (Input.GetKeyDown(KeyCode.E)) ? !toggleE : toggleE;
                toggleD = (Input.GetKeyDown(KeyCode.D)) ? !toggleD : toggleD;
                toggleR = (Input.GetKeyDown(KeyCode.R)) ? !toggleR : toggleR;
                toggleF = (Input.GetKeyDown(KeyCode.F)) ? !toggleF : toggleF;
                toggleT = (Input.GetKeyDown(KeyCode.T)) ? !toggleT : toggleT;
                toggleG = (Input.GetKeyDown(KeyCode.G)) ? !toggleG : toggleG;
                toggleY = (Input.GetKeyDown(KeyCode.Y)) ? !toggleY : toggleY;
                toggleH = (Input.GetKeyDown(KeyCode.H)) ? !toggleH : toggleH;
                toggleU = (Input.GetKeyDown(KeyCode.U)) ? !toggleU : toggleU;
                toggleJ = (Input.GetKeyDown(KeyCode.J)) ? !toggleJ : toggleJ;

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
}