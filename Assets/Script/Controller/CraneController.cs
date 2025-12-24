using System.Collections.Generic;
using UnityEngine;
using System;
using Filo;

public class CraneController : BaseController
{
    [Header("Laser Preset")]
    [Tooltip("Laser Preset")]
    public float laser_x_gap = 40f;
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

    [Header("연결(PLC)")]
    [SerializeField]
    CranePlcReadData readSnapshot;
    private CranePLCController plcController;

    [HideInInspector]
    public string nameSelf;
    [HideInInspector]
    public Transform craneBody, trolley, spreader, rtg_B, rtg_F;
    protected Rigidbody _rbSpreader;
    public Camera camTrolley1, camTrolley2;

    [HideInInspector]
    public Transform[] discs, SPSS, microMotion, twlLand, twlLock, laser, feet, cam;
    [HideInInspector]
    public GameObject[] cables;

    [HideInInspector]
    public bool landedContainer, landedFloor, locked;
    [HideInInspector]
    public float hoistPos, gantryLength;

    const float target20ft = 0; // 0m shift
    const float target40ft = 3; // default
    const float target45ft = 3.75f; // 3.75m shift
    const float spreaderFeetVel = 3.3f / 23;    // 어떤 계산식이지?
    const float landedHeight = 0.36f;   // spreader 바닥 landed 높이. flipper로 공중에 뜨기 때문.
    float ftOPTarget = target40ft;  // default
    float ftOPTargetOld = target40ft;  // default
    int ftOPDir;
    public bool isSelectedCrane;
    public List<CameraController> listCameraController = new List<CameraController>();
    public List<CameraController> listPLZCameraController_Girder;
    public CameraController selectedPLZCamera;
    private SpreaderController spreaderController;

    protected short cmdCamIndex1, cmdCamIndex2, cmdCamIndex3, cmdCamIndex4;

    #region Camera Index Property
    public short CmdCamIndex1
    {
        get { return cmdCamIndex1; }

        set
        {
            if (cmdCamIndex1 != value)
            {
                cmdCamIndex1 = value;
                SetCameraViewport(0, cmdCamIndex1);
            }
        }
    }

    public short CmdCamIndex2
    {
        get { return cmdCamIndex2; }

        set
        {
            if (cmdCamIndex2 != value)
            {
                cmdCamIndex2 = value;
                SetCameraViewport(1, cmdCamIndex2);
            }
        }
    }
    public short CmdCamIndex3
    {
        get { return cmdCamIndex3; }

        set
        {
            if (cmdCamIndex3 != value)
            {
                cmdCamIndex3 = value;
                SetCameraViewport(2, cmdCamIndex3);
            }
        }
    }
    public short CmdCamIndex4
    {
        get { return cmdCamIndex4; }

        set
        {
            if (cmdCamIndex4 != value)
            {
                cmdCamIndex4 = value;
                SetCameraViewport(3, cmdCamIndex4);
            }
        }
    }

    #endregion
    void Start()
    {
        plcController = GetComponent<CranePLCController>();

        // init array
        SPSS = new Transform[4];
        twlLand = new Transform[4];
        twlLock = new Transform[4];
        laser = new Transform[6];
        feet = new Transform[2];
        microMotion = new Transform[4];
        cam = new Transform[4];

        FindObject();
        InitValues();

        // Crane selected change event subscribe
        GM.OnSelectCrane -= OnCraneSelectedChange;
        GM.OnSelectCrane += OnCraneSelectedChange;
        OnCraneSelectedChange();

    }

    public virtual void InitValues()
    {
        // 변수 계산
        gantryLength = Vector3.Magnitude(rtg_F.position - rtg_B.position);
    }

    void Update()
    {
        // read from PLC
        if (plcController.isConnected)
        {
            readSnapshot = plcController.GetReadDataSnapshot();
            Trolley_OP(readSnapshot.sT_Vel);
            Gantry_OP(readSnapshot.sG_Vel_Forward, readSnapshot.sG_Vel_Backward);
            Hoist_OP(readSnapshot.sH_Vel);
            MicroMotion_OP(readSnapshot.MM_1_Vel, readSnapshot.MM_2_Vel, readSnapshot.MM_3_Vel, readSnapshot.MM_4_Vel);
            Feet_OP(readSnapshot._20FT, readSnapshot._40FT, readSnapshot._45FT);
            TwistLock_OP(readSnapshot.TL_Lock, readSnapshot.TL_Unlock);
            Landed();
        }
    }



    public virtual void FindObject()
    {

        nameSelf = gameObject.name;

        craneBody = gameObject.transform.Find("Body");
        var gantry = craneBody.transform.Find("Gantry");
        rtg_B = gantry.transform.Find("B_Position");
        rtg_F = gantry.transform.Find("F_Position");

        var cable = craneBody.transform.Find($"Cable");
        cables = new GameObject[cable.transform.childCount - 1];
        for (short j = 0; j < cables.Length; j++)
        {
            var cableTransform = cable.transform.Find($"Cable{j}");
            cables[j] = cableTransform.gameObject;
        }

        // Get Objects From Trolley
        trolley = craneBody.transform.Find("Trolley");
        var disc = trolley.transform.Find("Disc");
        discs = new Transform[disc.transform.childCount];

        for (short j = 0; j < discs.Length; j++)
        {
            discs[j] = disc.transform.Find($"Disc{j}");
        }

        var spss = trolley.transform.Find("SPSS");
        for (short j = 0; j < SPSS.Length; j++)
        {
            SPSS[j] = spss.transform.Find($"Lidar{j}");
        }
        // spreaderCam = trolley.transform.Find("Get_View_Camera");
        Transform camTrolley = trolley.transform.Find("Camera");
        camTrolley1 = camTrolley.Find("Trolley1").GetComponent<Camera>();
        camTrolley2 = camTrolley.Find("Trolley2").GetComponent<Camera>();

        // Get Objects From Spreader
        spreader = gameObject.transform.Find("Spreader");
        _rbSpreader = spreader.GetComponent<Rigidbody>();
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

        // camera controllers
        listCameraController.Clear();
        foreach (var camCtrl in gameObject.GetComponentsInChildren<CameraController>())
        {
            listCameraController.Add(camCtrl);
        }

        spreaderController = spreader.GetComponent<SpreaderController>();
    }

    public virtual void Gantry_OP(float velFWD, float velBWD)
    {

        float dPhi, vecDx, vecDz, theta, vecdLength, dirVecd;

        theta = (-craneBody.eulerAngles[1] + 90) * Mathf.Deg2Rad;  // 회전 방향 반대여서 부호 음수 처리

        // 두 속도 같을 때
        if (velBWD == velFWD)
        {
            dPhi = 0;
            vecDx = velBWD * Mathf.Cos(theta) * Time.deltaTime;
            vecDz = velBWD * Mathf.Sin(theta) * Time.deltaTime;
        }

        else
        {
            dPhi = (velFWD - velBWD) * Time.deltaTime / gantryLength;

            vecdLength = gantryLength * Math.Abs((velFWD + velBWD) / (2 * (velFWD - velBWD)));
            vecDx = vecdLength * (Mathf.Sin(theta) * (1 - Mathf.Cos(dPhi)) - Mathf.Cos(theta) * Mathf.Sin(dPhi));
            vecDz = vecdLength * (-Mathf.Sin(theta) * Mathf.Sin(dPhi) + Mathf.Cos(theta) * (Mathf.Cos(dPhi) - 1));

            dirVecd = Math.Sign(Math.Abs(velBWD) - Math.Abs(velFWD));
            vecDx *= dirVecd;
            vecDz *= dirVecd;
        }

        craneBody.position += new Vector3(vecDx, 0, vecDz);
        craneBody.Rotate(Vector3.up * (-dPhi) * Mathf.Rad2Deg);  // 회전 방향 반대여서 부호 음수 처리
    }

    void Trolley_OP(float vel)
    {
        trolley.Translate(Vector3.forward * Time.deltaTime * vel);
    }

    public virtual void Hoist_OP(float hoistVel)
    {
        // 흔들림 방지를 위한 보정값
        float force = 0.011f;
        // float force = 0.0065f;
        var speed = hoistVel * 138f;

        // force = (landedContainer && !cmdTwlLock) ? 0 : force;
        // disc를 이용하여 spreader움직임
        for (short j = 0; j < discs.Length; j++)
        {
            if (j == 5 || j == 11)
            {
                discs[j].Rotate(Vector3.forward * speed * Time.fixedDeltaTime, Space.World);
            }
            else if (j == 0 || j == 6)
            {
                discs[j].Rotate(Vector3.back * speed * Time.fixedDeltaTime, Space.World);
            }
            else if (j == 7 || j == 9 || j == 12)
            {
                discs[j].Rotate(Vector3.right * speed * Time.fixedDeltaTime, Space.World);
            }
            else if (j == 2 || j == 4)
            {
                discs[j].Rotate(Vector3.left * speed * Time.fixedDeltaTime, Space.World);
            }
            else if (j == 3 || j == 10)
            {
                discs[j].Rotate(Vector3.up * speed * Time.fixedDeltaTime, Space.Self);
            }
            else if (j == 1 || j == 8)
            {
                discs[j].Rotate(Vector3.down * speed * Time.fixedDeltaTime, Space.Self);
            }
        }

        // 내려갈때 disc로만 이동시 흔들림문제 발생. 안정적이지 않음
        if (speed < 0)
        {
            // spreader를 직접 이동시켜 흔들림이 보정
            Vector3 moveStep = Vector3.up * Time.fixedDeltaTime * speed * force;
            _rbSpreader.MovePosition(_rbSpreader.position + moveStep);
            // _ropeSlack = true;
        }
        else
        {
            // 계속 누르다가 올려올려 하면 살짝 통하고 튕김. 점프?
            // _ropeSlack = false;
        }
    }

    void MicroMotion_OP(float MM0Vel, float MM1Vel, float MM2Vel, float MM3Vel)
    {
        // 기계적 범위 안에서 움직이도록 하기 위함
        if ((microMotion[0].localPosition.x <= 0.25f && MM0Vel > 0) || (microMotion[0].localPosition.x >= -0.25f && MM0Vel < 0))
        {
            microMotion[0].Translate(Vector3.right * Time.deltaTime * MM0Vel);
        }
        if ((microMotion[1].localPosition.z <= 0.25f && MM1Vel > 0) || (microMotion[1].localPosition.z >= -0.25f && MM1Vel < 0))
        {
            microMotion[1].Translate(Vector3.back * Time.deltaTime * MM1Vel);
        }
        if ((microMotion[2].localPosition.x <= 0.25f && MM2Vel > 0) || (microMotion[2].localPosition.x >= -0.25f && MM2Vel < 0))
        {
            microMotion[2].Translate(Vector3.right * Time.deltaTime * MM2Vel);
        }
        if ((microMotion[3].localPosition.z <= 0.25f && MM3Vel > 0) || (microMotion[3].localPosition.z >= -0.25f && MM3Vel < 0))
        {
            microMotion[3].Translate(Vector3.back * Time.deltaTime * MM3Vel);
        }
    }

    void Feet_OP(bool _20FT, bool _40FT, bool _45FT)
    {
        if (_20FT) ftOPTarget = target20ft;
        else if (_40FT) ftOPTarget = target40ft;
        else if (_45FT) ftOPTarget = target45ft;

        // Target position 바뀌었을 때 1회만
        float diff;
        if (ftOPTarget != ftOPTargetOld)
        {
            // store direction
            diff = ftOPTarget - feet[0].localPosition.z;
            ftOPDir = Math.Sign(diff);

            // update old value
            ftOPTargetOld = ftOPTarget;
        }

        // Targat과 차이
        diff = ftOPTarget - feet[0].localPosition.z;

        // 방향 같을 때 이동
        if (Math.Sign(diff) == ftOPDir)
        {
            // shift spreader feet
            for (int i = 0; i < feet.Length; i++)
            {
                feet[i].Translate(Vector3.forward * ftOPDir * spreaderFeetVel * Time.deltaTime);
            }
        }
    }

    void TwistLock_OP(bool isTwlLock, bool isTwlUnlock)
    {
        spreaderController.UpdateTwistLockLogic(isTwlLock, isTwlUnlock);
    }

    // Landed 판단 후 Cable 늘어지는 효과 주는 부분?
    void Landed()
    {
        landedContainer = spreaderController.IsLanded;

        // spreader position y값이 landedHeight보다 낮으면 spreader 바닥이 지면에 닿았다고 판단
        landedFloor = spreader.position.y < landedHeight;

        // Landed 값 바뀌었을 때
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

    // Crane selected change

    // 분활된 화면에 맞게 카메라 넣기
    public virtual void SetCameraViewport(int viewport, int camIdx)
    {
        // 각 Crane 종류에 맞게 오버라이드 필요
    }

    protected virtual void PLZCamera_OP()
    {
        // 선택된 크레인이 아니면 종료
        if (!isSelectedCrane) return;

        // selectedPLZCamera = selected_Cam ? listPLZCameraController_Girder[0] : listPLZCameraController_Girder[1];

        if (selectedPLZCamera == null) return;

        // selectedPLZCamera.SetCameraPan(panLeft, panRight);
        // selectedPLZCamera.SetCameraTilt(tiltUp, tiltDown);
        // selectedPLZCamera.SetCameraCW(cw, ccw);
        // selectedPLZCamera.SetCamerZoom(zoomIn, zoomOut);

    }

    void OnCraneSelectedChange()
    {
        isSelectedCrane = GM.SelectedCrane == this;

        // 선택된 크레인일 경우
        if (isSelectedCrane)
        {
            //카메라 On
            foreach (var camCtrl in listCameraController)
            {
                camCtrl.CameraOn();
            }
        }
        // 선택된 크레인이 아닐경우
        else
        {
            //카메라 Off
            foreach (var camCtrl in listCameraController)
            {
                camCtrl.CameraOff();
            }
        }

    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        GM.OnSelectCrane -= OnCraneSelectedChange;
    }

}
