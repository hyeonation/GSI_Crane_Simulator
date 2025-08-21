using UnityEngine;
using System.Collections.Generic;

// GameManager, GameMaster
public class GM : MonoBehaviour
{
    // command with PLC
    public static bool cmdWithPLC = false;
    public static List<string> listIP = new(); // PLC IP list
    [HideInInspector] public static string[] nameCranes, nameTrucks;

    [HideInInspector] public static bool playSimulation = false; // 시뮬레이션 실행 여부

    [Header("Container_Preset")]
    public static short bay = 16;
    public static short row = 5;
    public static short tier = 5;

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

    void Awake()
    {
        // init variables
        InitVar();

        Application.targetFrameRate = 30;
    }

    static void InitVar()
    {
        // Crane name
        GameObject crane = GameObject.Find("Crane");
        nameCranes = new string[crane.transform.childCount];
        for (int i = 0; i < crane.transform.childCount; i++)
        {
            nameCranes[i] = crane.transform.GetChild(i).name;
        }

        // Truck name
        GameObject truck = GameObject.Find("Truck");
        nameTrucks = new string[truck.transform.childCount];
        for (int i = 0; i < truck.transform.childCount; i++)
        {
            nameTrucks[i] = truck.transform.GetChild(i).name;
        }

        // Read DB array
        cmdGantryVelFWD = new float[nameCranes.Length];
        cmdGantryVelBWD = new float[nameCranes.Length];
        cmdTrolleyVel = new float[nameCranes.Length];
        cmdSpreaderVel = new float[nameCranes.Length];
        cmdMM0Vel = new float[nameCranes.Length];
        cmdMM1Vel = new float[nameCranes.Length];
        cmdMM2Vel = new float[nameCranes.Length];
        cmdMM3Vel = new float[nameCranes.Length];
        cmd20ft = new bool[nameCranes.Length];
        cmd40ft = new bool[nameCranes.Length];
        cmd45ft = new bool[nameCranes.Length];
        cmdTwlLock = new bool[nameCranes.Length];
        cmdTwlUnlock = new bool[nameCranes.Length];
    }
}

// ----------------- 설정 데이터 모델 -----------------
// SimulatorSettings: 설정 데이터 모델
// data load, save 용이하도록 structure로 정의
public class SettingParams
{
    public List<string> listIP = new() { "192.168.100.101" };   // 기본 IP 주소. 최소 1개 이상.
    public float lidarMaxDistance_m = 100f;
    public float lidarFovHorizontal_deg = 90f;
    public float lidarFovVertical_deg = 30f;
    public float lidarResHorizontal_deg = 0.2f;
    public float lidarResVertical_deg = 0.2f;
    public float lidarNoiseStd = 0.01f;
    public float laserMaxDistance_m = 50f;
    public int yardContainerNumberEA = 100;
}