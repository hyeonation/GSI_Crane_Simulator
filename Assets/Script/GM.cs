using UnityEngine;
using System.Collections.Generic;
using System;

// GameManager, GameMaster
public static class GM
{
    // command with PLC
    [HideInInspector] public static string[] nameCranes, nameTrucks;

   

    [Header("Crane Type")]
    public static Action OnChangeCraneType;
    public static Define.CraneType craneType
    {
        get { return settingParams.craneType; }
        set
        {
            if (settingParams.craneType != value)
            {
                settingParams.craneType = value;
                OnChangeCraneType?.Invoke();
            }
        }
    }

    public static string craneTypeStr = "ARMG";
    // static values
    public static short readDBNum;
    public static short readLength;
    public static short writeDBNum;
    public static short writeStartIdx;
    public static short writeLength;

    public static StackProfile stackProfile = new();
    public static List<TaskInfo> listTaskInfo = new();

    public const float yard_x_interval = 2.840f;
    public const float yard_y_interval = 2.83f;
    public const float yard_z_interval = 12.96f;
    public const float yardWSxInterval = -19;
    public const float yardLSxInterval = 19;
    public const float containerYPosOnTruck = 2.7f;

    public const float TruckSpawnPosZ = 270f;

    // command data
    public static float[] cmdGantryVelBWD, cmdGantryVelFWD, cmdTrolleyVel, cmdSpreaderVel;
    public static float[] cmdMM0Vel, cmdMM1Vel, cmdMM2Vel, cmdMM3Vel;
    public static bool[] cmd20ft, cmd40ft, cmd45ft, cmdTwlLock, cmdTwlUnlock;

    public static ContainerInfoSO info40ft = new();

    // settings
    public static SettingParams settingParams = new();

    // dateTime
    public static DateTime dateTimeNow;
    public static string TimeToString(DateTime datetime)
        => datetime.ToString("yy'/'MM'/'dd HH:mm:ss");

    // crane position
    public static Vector3[] cranePOS = {
        new Vector3(0, 0, 0),
        new Vector3(0, 0, 100),
        new Vector3(0, 0, 200),
        new Vector3(0, 0, 300),
    };

    public static void InitVar()
    {
        // Crane
        GameObject crane = GameObject.Find("Crane");
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

    public static int FindContainerIndex(string strContainerID)
    {
        // find idx
        string cnid;
        int idx = -1;
        for (int i = 0; i < stackProfile.listID.Count; i++)
        {
            cnid = ByteArrayToString(stackProfile.listID[i]);
            if (cnid == strContainerID)
            {
                idx = i;
                break;
            }
        }
        return idx;
    }

    // public static void DestroyVar()
    // {
    //     // Crane
    //     GameObject crane = GameObject.Find("Crane");
    //     for (int i = 0; i < crane.transform.childCount; i++)
    //     {
    //         Destroy(crane.transform.GetChild(i));
    //     }

    //     // // Truck
    //     // GameObject truck = GameObject.Find("Truck");
    //     // for (int i = 0; i < truck.transform.childCount; i++)
    //     // {
    //     //     Destroy(truck.transform.GetChild(i));
    //     // }
    // }
}

// ----------------- 설정 데이터 모델 -----------------
// SimulatorSettings: 설정 데이터 모델
// data load, save 용이하도록 structure로 정의
public class SettingParams
{
    public List<string> listIP = new() { "192.168.100.80" };   // 기본 IP 주소. 최소 1개 이상.
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
    public bool cmdWithPLC = false;
    public Define.CraneType craneType = Define.CraneType.RMGC;


    // 설정 UI에서 변경가능한 값만 업데이트
    // 설정 UI에서 변경 불가능한 값(예: cmdWithPLC, craneType)은 제외
    public void UpdateUISettings(SettingParams newParams)
    {
        listIP = newParams.listIP;
        keyGantrySpeed = newParams.keyGantrySpeed;
        keyTrolleySpeed = newParams.keyTrolleySpeed;
        keySpreaderSpeed = newParams.keySpreaderSpeed;
        keyMMSpeed = newParams.keyMMSpeed;
        lidarMaxDistance_m = newParams.lidarMaxDistance_m;
        lidarFovHorizontal_deg = newParams.lidarFovHorizontal_deg;
        lidarFovVertical_deg = newParams.lidarFovVertical_deg;
        lidarResHorizontal_deg = newParams.lidarResHorizontal_deg;
        lidarResVertical_deg = newParams.lidarResVertical_deg;
        lidarNoiseStd = newParams.lidarNoiseStd;
        laserMaxDistance_m = newParams.laserMaxDistance_m;
        yardContainerNumberEA = newParams.yardContainerNumberEA;
    }
}


// Stack profile 일괄 관리하기 위해 class 정의
public class StackProfile
{
    public short lengthRow = 9;
    public short lengthBay = 16;
    public short lengthTier = 6;
    public List<byte[]> listID = new();
    public int[,] arrTier;      // SPSS 역할. [row, bay] = tier(stack count)
                                // TOS Container 선택 시 해당 row 최상단 접근 위해
                                // Yard Overview 그릴 때도 사용 가능
    public List<int[]> listPos;     // [i_row, i_bay, i_tier]
                                    // Container ID로 idx 접근하여 추출
                                    // Scene에서 row, bay, tier 파악 용이.
                                    // TOS에서 Bay 별 Container 그리기 용이.
                                    // 순서는? 무작위? listID와 index만 일치시키면?
                                    // task
    public List<GameObject> listContainerGO = new(); // Scene 내 Container GameObject 배열

}