using UnityEngine;
using System.Collections.Generic;

// GameManager, GameMaster
public class GM : MonoBehaviour
{
    // command with PLC
    public static bool cmdWithPLC = false;
    [HideInInspector] public static string[] nameRTGCs, nameQCs, nameTrucks;

    [Header("Container_Preset")]
    public static short bay = 16;
    public static short row = 5;
    public static short tier = 6;
    public static int[,] stack_profile;


    public const float yard_start_val = 8.32f;
    public const float yard_x_interval = 2.840f;
    public const float yard_y_interval = 2.83f;
    public const float yard_z_interval = 12.96f;

    // command data
    public static float[] cmdGantryVelBWD, cmdGantryVelFWD, cmdTrolleyVel, cmdSpreaderVel;
    public static float[] cmdMM0Vel, cmdMM1Vel, cmdMM2Vel, cmdMM3Vel;
    public static bool[] cmd20ft, cmd40ft, cmd45ft, cmdTwlLock, cmdTwlUnlock;

    // settings
    public static SettingParams settingParams = new();

    // crane position
    public static Vector3[] cranePOS = {
        new Vector3(0, 0, 0),
        new Vector3(0, 0, 100),
        new Vector3(0, 0, 200),
        new Vector3(0, 0, 300),
    };

    void Awake()
    {
        // init variables
        InitVar();

        // Application.targetFrameRate = 30;
    }

    public static void InitVar()
    {
        // temp
        GameObject crane;
        string craneType;

        // ARTG
        craneType = "ARTG";
        crane = GameObject.Find(craneType);
        nameRTGCs = new string[crane.transform.childCount];
        for (int i = 0; i < crane.transform.childCount; i++)
        {
            nameRTGCs[i] = crane.transform.GetChild(i).name;
        }

        // QC
        crane = GameObject.Find("QC");
        nameQCs = new string[crane.transform.childCount];
        for (int i = 0; i < crane.transform.childCount; i++)
        {
            nameQCs[i] = crane.transform.GetChild(i).name;
        }

        // Truck
        GameObject truck = GameObject.Find("Truck");
        nameTrucks = new string[truck.transform.childCount];
        for (int i = 0; i < truck.transform.childCount; i++)
        {
            nameTrucks[i] = truck.transform.GetChild(i).name;
        }

        // Read DB array
        cmdGantryVelFWD = new float[nameRTGCs.Length];
        cmdGantryVelBWD = new float[nameRTGCs.Length];
        cmdTrolleyVel = new float[nameRTGCs.Length];
        cmdSpreaderVel = new float[nameRTGCs.Length];
        cmdMM0Vel = new float[nameRTGCs.Length];
        cmdMM1Vel = new float[nameRTGCs.Length];
        cmdMM2Vel = new float[nameRTGCs.Length];
        cmdMM3Vel = new float[nameRTGCs.Length];
        cmd20ft = new bool[nameRTGCs.Length];
        cmd40ft = new bool[nameRTGCs.Length];
        cmd45ft = new bool[nameRTGCs.Length];
        cmdTwlLock = new bool[nameRTGCs.Length];
        cmdTwlUnlock = new bool[nameRTGCs.Length];

        // stack profile
        stack_profile = new int[row, bay];

    }

    public static void DestroyVar()
    {
        // Crane
        GameObject crane = GameObject.Find("Crane");
        for (int i = 0; i < crane.transform.childCount; i++)
        {
            Destroy(crane.transform.GetChild(i));
        }

        // // Truck
        // GameObject truck = GameObject.Find("Truck");
        // for (int i = 0; i < truck.transform.childCount; i++)
        // {
        //     Destroy(truck.transform.GetChild(i));
        // }
    }
}

// ----------------- 설정 데이터 모델 -----------------
// SimulatorSettings: 설정 데이터 모델
// data load, save 용이하도록 structure로 정의
public class SettingParams
{
    public List<string> listIP = new() { "192.168.100.101" };   // 기본 IP 주소. 최소 1개 이상.
    public float keyGantrySpeed = 0.5f;
    public float keyTrolleySpeed = 0.5f;
    public float keySpreaderSpeed = 0.25f;
    public float keyMMSpeed = 0.05f;
    public float lidarMaxDistance_m = 100f;
    public float lidarFovHorizontal_deg = 90f;
    public float lidarFovVertical_deg = 30f;
    public float lidarResHorizontal_deg = 0.2f;
    public float lidarResVertical_deg = 0.2f;
    public float lidarNoiseStd = 0.01f;
    public float laserMaxDistance_m = 50f;
    public int yardContainerNumberEA = 100;
}