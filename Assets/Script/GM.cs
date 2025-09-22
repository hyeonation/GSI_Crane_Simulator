using UnityEngine;
using System.Collections.Generic;
using System;

// GameManager, GameMaster
public class GM : MonoBehaviour
{
    // command with PLC
    public static bool cmdWithPLC = false;
    [HideInInspector] public static string[] nameCranes, nameTrucks;

    public short rowMax = 5;
    public short bayMax = 16;
    public short tierMax = 6;
    public float startVal = 8.32f;

    [Header("Container_Preset")]
    public static short lengthRow = 5;
    public static short lengthBay = 16;
    public static short lengthTier = 6;
    public static int[,] stack_profile;     // SPSS 역할
    public static List<byte[]> listContainerID = new();
    public static List<int[]> list_stack_profile;   // [i_row, i_bay, i_tier, containerStatus]



    public static float yard_start_val = 8.32f;
    public const float yard_x_interval = 2.840f;
    public const float yard_y_interval = 2.83f;
    public const float yard_z_interval = 12.96f;

    // command data
    public static float[] cmdGantryVelBWD, cmdGantryVelFWD, cmdTrolleyVel, cmdSpreaderVel;
    public static float[] cmdMM0Vel, cmdMM1Vel, cmdMM2Vel, cmdMM3Vel;
    public static bool[] cmd20ft, cmd40ft, cmd45ft, cmdTwlLock, cmdTwlUnlock;

    // settings
    public static SettingParams settingParams = new();

    // dateTime
    public static DateTime dateTimeNow;

    // container name


    // crane position
    public static Vector3[] cranePOS = {
        new Vector3(0, 0, 0),
        new Vector3(0, 0, 100),
        new Vector3(0, 0, 200),
        new Vector3(0, 0, 300),
    };

    void Awake()
    {

        // init
        lengthRow = rowMax;
        lengthBay = bayMax;
        lengthTier = tierMax;
        yard_start_val = startVal;

        // init variables
        InitVar();

        // Application.targetFrameRate = 30;
    }

    public static void InitVar()
    {
        // temp
        GameObject crane;
        string craneType;

        // Crane
        craneType = "Crane";
        crane = GameObject.Find(craneType);
        nameCranes = new string[crane.transform.childCount];
        for (int i = 0; i < crane.transform.childCount; i++)
        {
            nameCranes[i] = crane.transform.GetChild(i).name;
        }

        // Truck
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

        // stack profile
        stack_profile = new int[lengthRow, lengthBay];

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



    // 디버깅용: byte[] → string 변환
    public static string ByteArrayToString(byte[] arr)
    {
        char[] chars = new char[arr.Length];
        for (int i = 0; i < arr.Length; i++)
        {
            chars[i] = (char)arr[i];
        }
        return new string(chars);
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
    public int yardContainerNumberEA = 400;
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