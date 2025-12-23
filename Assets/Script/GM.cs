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
    public static Define.CraneType CraneType
    {
        get { return settingParams.craneType; }
        set
        {
            Debug.Log($"Crane Type changed to {value}");
            if (settingParams.craneType != value)
            {
                settingParams.craneType = value;
                OnChangeCraneType?.Invoke();
            }
        }
    }

    public static bool CmdWithPLC
    {
        get { return settingParams.cmdWithPLC; }
        set
        {
            Debug.Log($"CmdWithPLC changed to {value}");
            if (settingParams.cmdWithPLC != value)
            {
                settingParams.cmdWithPLC = value;
            }
        }
    }

    // UI_TruckControlPopup의 Truck List 업데이트
    public static Action OnUpdateTruckList;
    public static void UpdateTruckList()
    {
        OnUpdateTruckList?.Invoke();
    }

    // TruckContrl을 위한 Truck 선택. 선택된 Truck을 미세조정
    private static TruckController _selectedTruck;
    public static TruckController SelectedTruck
    {
        get { return _selectedTruck; }
        set
        {
            _selectedTruck = value;
            OnSelectTruck?.Invoke();
        }
    }
    public static Action OnSelectTruck;

    // Crane 선택. 선택된 Crane을 조정
    private static DrawingCrane _selectedCrane;
    public static DrawingCrane SelectedCrane
    {
        get { return _selectedCrane; }
        set
        {
            if (_selectedCrane != value)
            {
                _selectedCrane = value;
            }
            OnSelectCrane?.Invoke();
        }
    }
    // Camera 선택. 선택된 Camera를 조정
    public static Action OnSelectCrane;

    // 프로그램 시작 처음만 데이터 불러오기
    public static bool isDataLoaded = false;

    public static CommPLC[] plc;

    public static List<DrawingCrane> listCranes;

    public static string craneTypeStr = "ARMG";
    // static values
    public static short readDBNum;
    public static short readLength;
    public static short writeDBNum;
    public static short writeStartIdx;
    public static short writeLength;

    public static StackProfile stackProfile = new();
    public static List<TaskInfo> listTaskInfo = new();

    public const float yard_x_interval = 2.843f;
    public const float yard_y_interval = 2.83f;
    public const float yard_y_interval_Test = 2.93f;
    public const float yard_z_interval = 12.96f;
    public const float yardWSxInterval = -17.935f;
    public const float yardLSxInterval = 18.432f;
    public const float containerYPosOnTruck = 2.7f;

    public const float TruckSpawnPosZ = 270f;

    // command data
    public static CraneDataBase[] arrayCraneDataBase;

    public static float cmdTruckVel;

    public static ContainerInfoSO info40ft = new();
    public static GameObject containersParent;
    public static GameObject trucksParent;
    public static GameObject cranesParent;

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
        cranesParent = GameObject.Find("Crane");
        nameCranes = new string[cranesParent.transform.childCount];
        for (int i = 0; i < cranesParent.transform.childCount; i++)
        {
            nameCranes[i] = cranesParent.transform.GetChild(i).name;
        }

        // Truck
        trucksParent = GameObject.Find("Trucks");
        nameTrucks = new string[trucksParent.transform.childCount];
        for (int i = 0; i < trucksParent.transform.childCount; i++)
        {
            nameTrucks[i] = trucksParent.transform.GetChild(i).name;
        }

        containersParent = GameObject.Find("Containers");

        // Read DB array
        arrayCraneDataBase = new CraneDataBase[nameCranes.Length];
        for (int i = 0; i < nameCranes.Length; i++)
        {
            arrayCraneDataBase[i] = new CraneDataBase();
        }

        // init Stack profile
        stackProfile.InitStackProfile();
    }

    // 디버깅용: byte[] → string 변환


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

    public static void Clear()
    {
        SelectedCrane = null;
        SelectedTruck = null;

        if (CmdWithPLC)
        {
            for (int i = 0; i < GM.plc.Length; i++)
                plc[i].Disconnect();
            // init plc
            plc = new CommPLC[0];
        }
    }
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
    public float keyTruckSpeed = 0.1f;
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
    public List<int[]> listPos = new();     // [i_row, i_bay, i_tier]
                                            // Container ID로 idx 접근하여 추출
                                            // Scene에서 row, bay, tier 파악 용이.
                                            // TOS에서 Bay 별 Container 그리기 용이.
                                            // 순서는? 무작위? listID와 index만 일치시키면?
                                            // task
    public List<GameObject> listContainerGO = new(); // Scene 내 Container GameObject 배열

    public Action OnStackProfileChange;

    // Stack profile 초기화
    public void InitStackProfile()
    {
        listID.Clear();
        listPos.Clear();
        listContainerGO.Clear();
        arrTier = new int[lengthRow, lengthBay];
    }

    // stackprofile에서 container 정보삭제
    // idx를 받는 경우 
    public bool RemoveContainer(string strContainerID)
    {
        // 0. 이름으로 부터 idx 찾아오기
        int idx = FindContainerIndex(strContainerID);

        if (idx < 0 || idx >= listID.Count) return false;

        // 1. 위치 정보 추출 및 arrTier 갱신
        int[] pos = listPos[idx];
        int iRow = pos[0];
        int iBay = pos[1];
        int iTier = pos[2];

        // 해당 위치의 적재 단수 감소 (Stacking 로직 유지)
        if (arrTier[iRow, iBay] > 0)
        {
            arrTier[iRow, iBay]--;
        }

        // 2. 동기화된 리스트들에서 일괄 삭제
        listID.RemoveAt(idx);
        listPos.RemoveAt(idx);
        listContainerGO.RemoveAt(idx);

        // 3. 결과로그 출력
        Debug.Log($"<color=red>[RemoveContainer]</color> from StackProfile: " +
              $"ID: {strContainerID}, Index: {idx}, Position: [iRow:{iRow}, iBay:{iBay}, iTier:{iTier}], " +
              $"Remaining Stack Count: {arrTier[iRow, iBay]}");

        OnStackProfileChange.Invoke();
        return true;
    }

    public bool UpdateContainerStack(string containerID, int targetRow, int targetBay, GameObject go = null)
    {
        int existingIdx = FindContainerIndex(containerID);

        if (existingIdx != -1)
        {
            // 이미 존재하는 경우: 이동(Move) 로직 수행
            return _moveContainer(containerID, targetRow, targetBay);
        }
        else
        {
            // 존재하지 않는 경우: 신규 추가(Add) 로직 수행
            return _addContainer(containerID, targetRow, targetBay, go);
        }
    }

    // stackprofile내에서 container 이동
    bool _moveContainer(string strContainerID, int nextiRow, int nextiBay)
    {
        int idx = FindContainerIndex(strContainerID);

        // 1. 유효성 검사 및 실패 로그
        if (idx < 0 || idx >= listID.Count)
        {
            Debug.LogError($"[StackProfile] Move Failed: Container ID '{strContainerID}' not found in list.");
            return false;
        }

        // 2. 기존 위치(Source) 정보 추출
        int[] prevPos = listPos[idx];
        int prevRow = prevPos[0];
        int prevBay = prevPos[1];
        int prevTier = prevPos[2];

        // 3. 기존 위치 데이터 업데이트
        if (arrTier[prevRow, prevBay] > 0)
        {
            arrTier[prevRow, prevBay]--;
        }
        else
        {
            Debug.LogWarning($"[StackProfile] Data Inconsistency: Source position [{prevRow}, {prevBay}] tier is already 0.");
            return false;
        }

        // 4. 신규 위치(Destination) 정보 업데이트
        int nextTier = arrTier[nextiRow, nextiBay];

        // 최대 적재 높이(lengthTier) 초과 여부 체크 (기획적 안정성 확보)
        if (nextTier >= lengthTier)
        {
            Debug.LogError($"[StackProfile] Move Warning: Destination [{nextiRow}, {nextiBay}] exceeds max tier ({lengthTier}).");
            return false;
        }

        listPos[idx] = new int[] { nextiRow, nextiBay, nextTier };
        arrTier[nextiRow, nextiBay]++;

        // 5. 결과 로그 출력 
        Debug.Log($"<color=yellow>[MoveContainer]</color> ID: <b>{strContainerID}</b>\n" +
                  $"From: <color=#32CD32>[R:{prevRow}, B:{prevBay}, T:{prevTier}]</color> " +
                  $"→ To: <color=#1E90FF>[R:{nextiRow}, B:{nextiBay}, T:{nextTier}]</color>\n" +
                  $"Yard Status: Source Tier ({arrTier[prevRow, prevBay]}), Dest Tier ({arrTier[nextiRow, nextiBay]})");

        OnStackProfileChange.Invoke();
        return true;
    }
    // stackprfile에 다른 컨테이너 추가
    bool _addContainer(string strContainerID, int addiRow, int addiBay, GameObject containerGO = null)
    {
        // 1. 중복 검사: 이미 존재하는 ID인지 확인 (데이터 무결성 확보)
        if (FindContainerIndex(strContainerID) != -1)
        {
            Debug.LogError($"[StackProfile] Add Failed: Container ID '{strContainerID}' already exists in StackProfile.");
            return false;
        }

        // 2. 적재 가능 여부 확인 및 Tier 계산
        // 현재 해당 Row, Bay에 쌓여있는 단수가 새로운 컨테이너의 Tier가 됨
        int addiTier = arrTier[addiRow, addiBay];

        if (addiTier >= lengthTier)
        {
            Debug.LogWarning($"[StackProfile] Stack Warning: Destination [{addiRow}, {addiBay}] is full (Current:{addiTier} / Max:{lengthTier}). Adding anyway.");
            return false;
        }

        // 3. 데이터 추가 (ListID, ListPos, ListContainerGO 동기화)
        // string을 byte[]로 변환하여 저장
        byte[] idBytes = System.Text.Encoding.Default.GetBytes(strContainerID);
        listID.Add(idBytes);

        // 위치 정보 저장 [Row, Bay, Tier]
        listPos.Add(new int[] { addiRow, addiBay, addiTier });

        // 생성된 GameObject 참조 저장 (외부에서 생성하여 넘겨준다고 가정)
        listContainerGO.Add(containerGO);

        // 4. SPSS(arrTier) 갱신
        arrTier[addiRow, addiBay]++;

        // 5. 결과 로그 출력 (추적성 확보)
        Debug.Log($"<color=cyan>[AddContainer]</color> to StackProfile: " +
                  $"ID: <b>{strContainerID}</b>, Position: [R:{addiRow}, B:{addiBay}, T:{addiTier}], " +
                  $"Total Stack Count: {arrTier[addiRow, addiBay]}");

        OnStackProfileChange?.Invoke();
        return true;
    }

    public bool TryGetGridIndexFromCenter(Vector3 containerWorldPos, Vector3 yardOriginPos, out int outRow, out int outBay)
    {
        // 1. 야드 원점으로부터의 상대 거리 계산
        Vector3 relativePos = containerWorldPos - yardOriginPos;


        float offsetBay = relativePos.z + (GM.yard_z_interval * 0.5f);
        float offsetRow = relativePos.x + (GM.yard_x_interval * 0.5f);

        int calcBay = Mathf.FloorToInt(offsetBay / GM.yard_z_interval);
        int calcRow = Mathf.FloorToInt(offsetRow / GM.yard_x_interval);

        // 3. 유효 범위 검사
        if (calcRow >= 0 && calcRow < lengthRow && calcBay >= 0 && calcBay < lengthBay)
        {
            outRow = calcRow;
            outBay = calcBay;
            return true;
        }

        outRow = -1;
        outBay = -1;
        return false;
    }



    public string ByteArrayToString(byte[] arr)
    {
        char[] chars = new char[arr.Length];
        for (int i = 0; i < arr.Length; i++)
        {
            chars[i] = (char)arr[i];
        }
        return new string(chars);
    }

    public int FindContainerIndex(string strContainerID)
    {
        // find idx
        string cnid;
        int idx = -1;
        for (int i = 0; i < listID.Count; i++)
        {
            cnid = ByteArrayToString(listID[i]);
            if (cnid == strContainerID)
            {
                idx = i;
                break;
            }
        }
        return idx;
    }



}