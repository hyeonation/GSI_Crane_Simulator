using Unity.VisualScripting;
using UnityEngine;
using System;
using Filo;

public class CraneDrawing : MonoBehaviour
{

    [Header("Laser Preset")]
    [Tooltip("Laser Preset")]
    public float laser_gap = 40f;
    public float laser_y_gap = 640f;

    [Header("Camera Preset")]
    [Tooltip("Camera Preset")]
    public float camera_x_gap = 510f;
    public float camera_y_gap = 640f;
    public float camera_z_gap = 145f;

    [Header("SPSS Preset")]
    [Tooltip("SPSS Preset")]
    public float SPSS_x_gap = 1500f;
    public float SPSS_y_gap = 22500f;
    public float SPSS_z_gap = 2500f;

    string nameSelf;
    int iSelf;
    Transform rtg, trolley, spreader, rtg_B, rtg_F, spreaderCam;
    
    Transform[] disc, SPSS, microMotion, twlLand, twlLock, laser, feet, cam;
    GameObject[] cables;
    GameObject container;
    Rigidbody containerRigidbody;

    bool Feet20_ack, Feet40_ack, fifthup, fifthdown, landedContainer, landedFloor, locked, unlocked;
    float hoistPos, gantryLength;

    const float target20feet = 3; // 3m shift
    const float target40feet = 0; // default
    const float spreaderFeetVel = 3.3f / 23;    // 어떤 계산식이지?
    const float landedHeight = 0.36f;   // spreader 바닥 landed 높이. flipper로 공중에 뜨기 때문.

    bool[] cmdTwlLockOld, cmdTwlUnlockOld;

    bool landedContainerOld, landedFloorOld;
    private FixedJoint containerFixedJoint;

    void Start()
    {

        // init array
        cables = new GameObject[8];
        disc = new Transform[13];
        SPSS = new Transform[4];
        twlLand = new Transform[4];
        twlLock = new Transform[4];
        laser = new Transform[6];
        feet = new Transform[2];
        microMotion = new Transform[4];
        cam = new Transform[4];

        FindObject();

        InitLaserPos(laser_gap, laser_y_gap);
        InitCameraPos(camera_x_gap, camera_y_gap, camera_z_gap);
        InitSPSSPos(SPSS_x_gap, SPSS_y_gap, SPSS_z_gap);

        // 변수 계산
        gantryLength = Vector3.Magnitude(rtg_F.position - rtg_B.position);

        // 중복 계산 방지를 위한 배열 복사
        cmdTwlLockOld = (bool[])GM.cmdTwlLock.Clone();
        cmdTwlUnlockOld = (bool[])GM.cmdTwlUnlock.Clone();
    }

    void Update()
    {
        Gantry_OP();
        Trolley_OP();
        Hoist_OP();
        MicroMotion_OP();
        Feet_OP();
        TwistLock_OP();
        Landed();
    }

    void FindObject()
    {

        // self crane info
        nameSelf = gameObject.name;
        iSelf = Array.IndexOf(GM.nameCranes, nameSelf);

        rtg = gameObject.transform.Find("RTG");
        var gantry = rtg.transform.Find("Gantry");
        rtg_B = gantry.transform.Find("B_Position");
        rtg_F = gantry.transform.Find("F_Position");

        var cable = rtg.transform.Find($"Cable");
        for (short j = 0; j < cables.Length; j++)
        {
            var cableTransform = cable.transform.Find($"Disc{j}_Cable");
            cables[j] = cableTransform.gameObject;
        }

        // Get Objects From Trolley
        trolley = rtg.transform.Find("Trolley");
        var Disc = trolley.transform.Find("Disc");
        for (short j = 0; j < disc.Length; j++)
        {
            disc[j] = Disc.transform.Find($"Disc{j}");
        }

        var spss = trolley.transform.Find("SPSS");
        for (short j = 0; j < SPSS.Length; j++)
        {
            SPSS[j] = spss.transform.Find($"Lidar{j}");
        }
        spreaderCam = trolley.transform.Find("Get_View_Camera");

        // Get Objects From Spreader
        spreader = gameObject.transform.Find("Spreader");

        feet[0] = spreader.transform.Find("Spreader_0");
        var twistlock_0 = feet[0].transform.Find("TwistLock");
        
        twlLand[0] = twistlock_0.transform.Find("Land_0");
        twlLand[1] = twistlock_0.transform.Find("Land_1");

        twlLock[0] = twistlock_0.transform.Find("Lock_0");
        twlLock[1] = twistlock_0.transform.Find("Lock_1");
        
        var laser_0 = feet[0].transform.Find("laser");

        laser[0] = laser_0.transform.Find("Laser0");
        laser[1] = laser_0.transform.Find("Laser1");
        laser[5] = laser_0.transform.Find("Laser5");

        var cam_0 = feet[0].transform.Find("Camera");
        cam[0] = cam_0.transform.Find("Camera1");
        cam[3] = cam_0.transform.Find("Camera4");

        // Spreader Sensor
        feet[1] = spreader.transform.Find("Spreader_1");
        var twistlock_1 = feet[1].transform.Find("TwistLock");

        twlLand[2] = twistlock_1.transform.Find("Land_2");
        twlLand[3] = twistlock_1.transform.Find("Land_3");
        
        twlLock[2] = twistlock_1.transform.Find("Lock_2");
        twlLock[3] = twistlock_1.transform.Find("Lock_3");

        var mm = spreader.transform.Find("MicroMotion");

        for (short j = 0; j < microMotion.Length; j++)
        {
            microMotion[j] = mm.transform.Find($"MM{j}");
        }
        var laser_1 = feet[1].transform.Find("laser");

        laser[2] = laser_1.transform.Find("Laser2");
        laser[3] = laser_1.transform.Find("Laser3");
        laser[4] = laser_1.transform.Find("Laser4");

        var cam_1 = feet[1].transform.Find("Camera");
        cam[1] = cam_1.transform.Find("Camera2");
        cam[2] = cam_1.transform.Find("Camera3");
    }

    void Gantry_OP()
    {

        float dPhi, vecDx, vecDz, theta, vecdLength, dirVecd;

        theta = (-rtg.eulerAngles[1] + 90) * Mathf.Deg2Rad;  // 회전 방향 반대여서 부호 음수 처리

        // 두 속도 같을 때
        if (GM.cmdGantryVelBWD[iSelf] == GM.cmdGantryVelFWD[iSelf])
        {
            dPhi = 0;
            vecDx = GM.cmdGantryVelBWD[iSelf] * Mathf.Cos(theta) * Time.deltaTime;
            vecDz = GM.cmdGantryVelBWD[iSelf] * Mathf.Sin(theta) * Time.deltaTime;
        }

        else
        {
            dPhi = (GM.cmdGantryVelFWD[iSelf] - GM.cmdGantryVelBWD[iSelf]) * Time.deltaTime / gantryLength;

            vecdLength = gantryLength * Math.Abs((GM.cmdGantryVelFWD[iSelf] + GM.cmdGantryVelBWD[iSelf]) / (2 * (GM.cmdGantryVelFWD[iSelf] - GM.cmdGantryVelBWD[iSelf])));
            vecDx = vecdLength * (Mathf.Sin(theta) * (1 - Mathf.Cos(dPhi)) - Mathf.Cos(theta) * Mathf.Sin(dPhi));
            vecDz = vecdLength * (-Mathf.Sin(theta) * Mathf.Sin(dPhi) + Mathf.Cos(theta) * (Mathf.Cos(dPhi) - 1));

            dirVecd = Math.Sign(Math.Abs(GM.cmdGantryVelBWD[iSelf]) - Math.Abs(GM.cmdGantryVelFWD[iSelf]));
            vecDx *= dirVecd;
            vecDz *= dirVecd;
        }

        rtg.position += new Vector3(vecDx, 0, vecDz);
        rtg.Rotate(Vector3.up * (-dPhi) * Mathf.Rad2Deg);  // 회전 방향 반대여서 부호 음수 처리
    }

    void Trolley_OP()
    {
        trolley.Translate(Vector3.right * Time.deltaTime * GM.cmdTrolleyVel[iSelf]);
    }
    
    void Hoist_OP()
    {
        var force = 0.0065f;
        var speed = GM.cmdSpreaderVel[iSelf] * 138f;

        //var con_force = 0.0065f;

        force = landedContainer ? 0 : force;
        //con_force = (Container_inf[i].GetComponent<Container_landed>().Con_landed[i]) ? 0 : con_force;

        for (short j = 0; j < disc.Length; j++)
        {
            if (j == 5 || j == 11)
            {
                disc[j].Rotate(Vector3.forward * speed * Time.deltaTime, Space.World);
            }
            else if (j == 0 || j == 6)
            {
                disc[j].Rotate(Vector3.back * speed * Time.deltaTime, Space.World);
            }
            else if (j == 7 || j == 9 || j == 12)
            {
                disc[j].Rotate(Vector3.right * speed * Time.deltaTime, Space.World);
            }
            else if (j == 2 || j == 4)
            {
                disc[j].Rotate(Vector3.left * speed * Time.deltaTime, Space.World);
            }
            else if (j == 3 || j == 10)
            {
                disc[j].Rotate(Vector3.up * speed * Time.deltaTime, Space.Self);
            }
            else if (j == 1 || j == 8)
            {
                disc[j].Rotate(Vector3.down * speed * Time.deltaTime, Space.Self);
            }
        }

        if (speed < 0)
        {
            // spreader.Translate(Vector3.up * Time.deltaTime * speed * force);
            spreaderCam.Translate(Vector3.up * Time.deltaTime * speed * force, Space.World);
            hoistPos = landedContainer ? hoistPos + (speed / 130) * Time.deltaTime : spreader.position.y;    // 착지하면 spreader는 멈추지만 wire length는 계속 증가
            if (locked)
            {
                // Container_inf[i].transform.Translate(Vector3.up * Time.deltaTime * speed * force);
            }
        }
        else
        {
            // spreader.Translate(Vector3.up * Time.deltaTime * 0);
            // Container_inf[i].transform.Translate(Vector3.up * Time.deltaTime * 0);
            hoistPos = (landedContainer) ? hoistPos + (speed / 130) * Time.deltaTime : spreader.position.y;
            if (!landedContainer)
            {
                spreaderCam.Translate(Vector3.up * Time.deltaTime * speed * force, Space.World);
            }
        }
    }

    void MicroMotion_OP()
    {
        // 기계적 범위 안에서 움직이도록 하기 위함
        if ((microMotion[0].localPosition.x <= 0.25f && GM.cmdMM0Vel[iSelf] > 0) || (microMotion[0].localPosition.x >= -0.25f && GM.cmdMM0Vel[iSelf] < 0))
        {
            microMotion[0].Translate(Vector3.right * Time.deltaTime * GM.cmdMM0Vel[iSelf]);
        }
        if ((microMotion[1].localPosition.z <= 0.25f && GM.cmdMM1Vel[iSelf] > 0) || (microMotion[1].localPosition.z >= -0.25f && GM.cmdMM1Vel[iSelf] < 0))
        {
            microMotion[1].Translate(Vector3.back * Time.deltaTime * GM.cmdMM1Vel[iSelf]);
        }
        if ((microMotion[2].localPosition.x <= 0.25f && GM.cmdMM2Vel[iSelf] > 0) || (microMotion[2].localPosition.x >= -0.25f && GM.cmdMM2Vel[iSelf] < 0))
        {
            microMotion[2].Translate(Vector3.right * Time.deltaTime * GM.cmdMM2Vel[iSelf]);
        }
        if ((microMotion[3].localPosition.z <= 0.25f && GM.cmdMM3Vel[iSelf] > 0) || (microMotion[3].localPosition.z >= -0.25f && GM.cmdMM3Vel[iSelf] < 0))
        {
            microMotion[3].Translate(Vector3.back * Time.deltaTime * GM.cmdMM3Vel[iSelf]);
        }
    }
    
    void Feet_OP()
    {
        // spreader feet dx
        float spreaderFeetdx = Time.deltaTime * spreaderFeetVel;

        if (GM.cmd20ft[iSelf])
        {
            // shift spreader_0
            if (feet[0].localPosition.z > -target20feet)
            {
                feet[0].Translate(Vector3.back * spreaderFeetdx);
            }

            // shift spreader_1
            if (feet[1].localPosition.z < target20feet)
            {
                feet[1].Translate(Vector3.forward * spreaderFeetdx);
            }
        }

        else if (GM.cmd40ft[iSelf])
        {
            // shift spreader_0
            if (feet[0].localPosition.z < -target40feet)
            {
                feet[0].Translate(Vector3.forward * spreaderFeetdx);
            }

            // shift spreader_1
            if (feet[1].localPosition.z > target40feet)
            {
                feet[1].Translate(Vector3.back * spreaderFeetdx);
            }
        }
    }
    
    void TwistLock_OP()
    {
        // Lock. 반복실행 방지 코드 추가.
        if (GM.cmdTwlLock[iSelf] && (cmdTwlLockOld[iSelf] != GM.cmdTwlLock[iSelf]))
        {
            Debug.Log("Lock");

            // update value
            cmdTwlLockOld[iSelf] = GM.cmdTwlLock[iSelf];
            cmdTwlUnlockOld[iSelf] = false;
            GM.cmdTwlUnlock[iSelf] = false;

            // landedContainer == true 시 Container 체결
            if (landedContainer)
            {
                container = twlLand[0].GetComponent<Landed>().container;   // 컨테이너 정보 가져오기
                Debug.Log($"Container: {container.name}");
                container.AddComponent<FixedJoint>(); // FixedJoint 추가
                containerFixedJoint = container.GetComponent<FixedJoint>(); // FixedJoint 변수에 저장
                containerFixedJoint.connectedBody = spreader.GetComponent<Rigidbody>(); // spreader와 연결
                containerFixedJoint.breakForce = Mathf.Infinity; // 충분히 큰 값
                containerFixedJoint.breakTorque = Mathf.Infinity; // 충분히 큰 값
            }
        }

        else if (GM.cmdTwlUnlock[iSelf] && (cmdTwlUnlockOld[iSelf] != GM.cmdTwlUnlock[iSelf]))
        {
            Debug.Log("Unlock");

            cmdTwlUnlockOld[iSelf] = GM.cmdTwlUnlock[iSelf];
            cmdTwlLockOld[iSelf] = false;
            GM.cmdTwlLock[iSelf] = false;

            // 컨테이너와 spreader 연결 해제
            Destroy(containerFixedJoint);
        }
    }

    // Landed 판단 후 Cable 늘어지는 효과 주는 부분?
    void Landed()
    {
        int landedCount = 0;

        for (int j = 0; j < twlLand.Length; j++)
        {
            if (twlLand[j].GetComponent<Landed>().landed_sensor)
            {
                landedCount++;
            }
        }

        //// Landed 여부

        // twlLand의 개수가 4개이므로, 모두 landed 되면 true
        landedContainer = landedCount == twlLand.Length;

        // spreader position y값이 landedHeight보다 낮으면 spreader 바닥이 지면에 닿았다고 판단
        landedFloor = spreader.position.y < landedHeight;

        // Landed 값 바뀌었을 때
        if ((landedContainer != landedContainerOld) || (landedFloor != landedFloorOld))
        {
            // update value
            landedContainerOld = landedContainer;
            landedFloorOld = landedFloor;

            // 하나라도 landed 되면 cable 늘어짐 효과 주기
            if (landedContainer || landedFloor)
            {
                for (int j = 0; j < cables.Length; j++)
                {
                    cables[j].GetComponent<Cable>().loosenessScale = 1;
                }
            }

            else
            {
                for (int j = 0; j < cables.Length; j++)
                {
                    cables[j].GetComponent<Cable>().loosenessScale = 0;
                }
            }
        }        
    }

    void InitLaserPos(float gqp, float ygap)
    {
        // spreader position
        Vector3 spreader_pos = spreader.position;

        //x, z preset
        float SideLaser = 0.43f;
        float Front_Back_Laser = 5.677f;
        float gapMultiplier = gqp * 0.001f;
        float ygapMultiplier = ygap * 0.001f;

        //container size
        float Container_Z = 12.19f;
        float Container_X = 2.43f;

        float halfContainerZ = Container_Z * 0.5f;
        float halfContainerX = Container_X * 0.5f;

        float y_cal = spreader_pos.y + ygapMultiplier;

        Vector3[] LaserPostion = new Vector3[]
        {
            new Vector3(spreader_pos.x - SideLaser, y_cal, spreader_pos.z + gapMultiplier + halfContainerZ),
            new Vector3(spreader_pos.x + halfContainerX + gapMultiplier, y_cal, spreader_pos.z + Front_Back_Laser),
            new Vector3(spreader_pos.x + halfContainerX + gapMultiplier, y_cal, spreader_pos.z - Front_Back_Laser),
            new Vector3(spreader_pos.x + SideLaser, y_cal, spreader_pos.z - gapMultiplier - halfContainerZ),
            new Vector3(spreader_pos.x - halfContainerX - gapMultiplier, y_cal, spreader_pos.z - Front_Back_Laser),
            new Vector3(spreader_pos.x - halfContainerX - gapMultiplier, y_cal, spreader_pos.z + Front_Back_Laser),
        };

        // Laser
        for (short i = 0; i < LaserPostion.Length; i++)
        {
            laser[i].position = LaserPostion[i];
        }
    }

    void InitCameraPos(float xgap, float ygap, float zgap)
    {
        // spreader position
        Vector3 spreader_pos = spreader.position;

        //X, Y ,Z Presset
        float z = 11.985f;
        float x = 2.259f;

        //multiplier 0.001
        float xgap_cal = xgap * 0.001f;
        float ygap_cal = ygap * 0.001f;
        float zgap_cal = zgap * 0.001f;

        Vector3[] cameraPostion = new Vector3[]
        {
            new Vector3(spreader_pos.x + x * 0.5f - xgap_cal, spreader_pos.y + ygap_cal , spreader_pos.z + z * 0.5f + zgap_cal),
            new Vector3(spreader_pos.x + x * 0.5f - xgap_cal, spreader_pos.y + ygap_cal , spreader_pos.z - z * 0.5f - zgap_cal),
            new Vector3(spreader_pos.x - x * 0.5f + xgap_cal, spreader_pos.y + ygap_cal , spreader_pos.z - z * 0.5f - zgap_cal),
            new Vector3(spreader_pos.x - x * 0.5f + xgap_cal, spreader_pos.y + ygap_cal , spreader_pos.z + z * 0.5f + zgap_cal),
        };

        for (short i = 0; cameraPostion.Length > i; i++)
        {
            cam[i].position = cameraPostion[i];
        }
    }

    void InitSPSSPos(float xgap, float ygap, float zgap)
    {
        // Object position
        Vector3 gantry_pos = rtg.position;
        Vector3 trolley_pos = trolley.position;

        //multiplier
        float xgap_cal = xgap * 0.001f;
        float ygap_cal = ygap * 0.001f;
        float zgap_cal = zgap * 0.001f;

        Vector3[] SPSSPostion = new Vector3[]
        {
            new Vector3(trolley_pos.x + xgap_cal, ygap_cal, gantry_pos.z + zgap_cal),
            new Vector3(trolley_pos.x + xgap_cal, ygap_cal, gantry_pos.z),
            new Vector3(trolley_pos.x - xgap_cal, ygap_cal, gantry_pos.z - zgap_cal),
            new Vector3(trolley_pos.x - xgap_cal, ygap_cal, gantry_pos.z),
        };

        for (short i = 0; SPSSPostion.Length > i; i++)
        {
            SPSS[i].position = SPSSPostion[i];
        }
    }
}
