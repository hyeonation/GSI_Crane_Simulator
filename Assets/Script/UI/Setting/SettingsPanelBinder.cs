using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Setting UI(스크린샷 구조)에 맞춘 바인더:
/// - TMP_InputField ↔ 내부 Settings 데이터 연결
/// - 입력 검증(숫자/범위, IP 형식)
/// - Apply 버튼으로 저장/이벤트 발행
/// - 시작 시 자동 로드
/// 저장 위치: Application.persistentDataPath/settings.json
/// </summary>
public class SettingsPanelBinder : MonoBehaviour
{

    // ----------------- UI 요소들 -----------------
    [Header("PLC Mode")]
    [SerializeField] private GameObject plcIPInputFieldPrefab; // IP 입력 필드 프리팹
    [SerializeField] private GameObject plcModePanel; // IP 입력 필드들을 담을 컨테이너
    [SerializeField] private RectTransform contentsPanel;  // Contents 패널 RectTransform. 화면 refresh 위함
    [HideInInspector] private List<GameObject> listIPObject; // IP 입력 필드 오브젝트 리스트

    [Header("Keyboard Mode")]
    [SerializeField] private TMP_InputField keyGantrySpeed;
    [SerializeField] private TMP_InputField keyTrolleySpeed;
    [SerializeField] private TMP_InputField keySpreaderSpeed;
    [SerializeField] private TMP_InputField keyMMSpeed;

    [Header("SPSS LiDAR")]
    [SerializeField] private TMP_InputField lidarMaxDistance_m;
    [SerializeField] private TMP_InputField lidarFovHorizontal_deg;
    [SerializeField] private TMP_InputField lidarFovVertical_deg;
    [SerializeField] private TMP_InputField lidarResHorizontal_deg;
    [SerializeField] private TMP_InputField lidarResVertical_deg;
    [SerializeField] private TMP_InputField lidarNoiseStd;

    [Header("Laser")]
    [SerializeField] private TMP_InputField laserMaxDistance_m;

    [Header("Yard")]
    [SerializeField] private TMP_InputField yardContainerNumberEA;

    [Header("Controls")]
    [SerializeField] private Button btnApply;
    [SerializeField] private GameObject menuControllerPanel;       // menuControllerPanel 패널 오브젝트
    [SerializeField] private Button btnAddIP; // IP 추가 버튼
    [SerializeField] private Button btnRemoveIP; // IP 제거 버튼

    // get, set 방식이 굳이 필요 없어서 주석 처리
    // public static SettingParams Current { get; private set; } = new SimulatorSettings();
    public SettingParams current = new();
    public SettingParams settingDefault = new();
    [SerializeField]
    private Button btn_init;

    // ----------------- 범위(필요시 인스펙터에서 조정) -----------------
    [Header("Validation Ranges")]
    [SerializeField] private Vector2 range_keyGantrySpeed = new Vector2(0f, 10f);
    [SerializeField] private Vector2 range_keyTrolleySpeed = new Vector2(0f, 10f);
    [SerializeField] private Vector2 range_keySpreaderSpeed = new Vector2(0f, 10f);
    [SerializeField] private Vector2 range_keyMMSpeed = new Vector2(0f, 10f);
    [SerializeField] private Vector2 range_LidarMax_m = new Vector2(1f, 10000f);
    [SerializeField] private Vector2 range_FovH_deg = new Vector2(1f, 360f);
    [SerializeField] private Vector2 range_FovV_deg = new Vector2(1f, 180f);
    [SerializeField] private Vector2 range_Res_deg = new Vector2(0.01f, 10f);
    [SerializeField] private Vector2 range_Noise = new Vector2(0f, 100f);
    [SerializeField] private Vector2 range_LaserMax_m = new Vector2(1f, 10000f);
    [SerializeField] private Vector2Int range_Containers = new Vector2Int(0, 1000000);

    // ----------------- 파일 저장 경로 -----------------
    // Application.persistentDataPath/settings.json
    // Unity의 영속적 데이터 경로를 사용하여 설정 파일을 저장
    private static string FilePath =>
        Path.Combine(Application.persistentDataPath, "settings.json");

    // ----------------- Unity lifecycle -----------------
    void Awake()
    {
        // 버튼 연결
        if (btnApply) btnApply.onClick.AddListener(ApplyAndSave);
        if (btnAddIP) btnAddIP.onClick.AddListener(AddIP);
        if (btnRemoveIP) btnRemoveIP.onClick.AddListener(RemoveIP);
        if (btn_init) btn_init.onClick.AddListener(SettingsParamsInit);

        // UI 채우기
        InitUIFromData();
        InitPlaceholder(); // 플레이스홀더 설정
        ClearAllErrorStates();

        // UI 갱신
        // panel 제거할 때는 추가할 때와 다르게 UI Update가 한 frame 늦게 적용되어
        // 다음과 같은 조치
        StartCoroutine(RefreshUI());
    }

    // 게임 오브젝트가 파괴될 때 이벤트 해제
    void OnDestroy()
    {
        if (btnApply) btnApply.onClick.RemoveListener(ApplyAndSave);
        if (btnAddIP) btnAddIP.onClick.RemoveListener(AddIP);
        if (btnRemoveIP) btnRemoveIP.onClick.RemoveListener(RemoveIP);
    }

    // ----------------- UI -> Data 적용 & 저장 -----------------
    public void ApplyAndSave()
    {
        ClearAllErrorStates();

        // 입력값 읽기 + 검증
        bool ok = TryReadAllFields(out SettingParams updated);
        if (!ok)
        {
            Debug.Log("[SettingsPanelBinder] 입력값에 오류가 있어 저장하지 않았습니다.");
            return;
        }

        // 반영
        current = updated;

        // UI에서 읽은 값을 데이터 모델에 반영
        GM.settingParams.UpdateUISettings(current);
        
        // 저장
        SaveToDisk();

        Debug.Log("[SettingsPanelBinder] 설정 적용/저장 완료: " + FilePath);

        gameObject.SetActive(false); // 현재 SettingsPanel 숨김
        if (menuControllerPanel)
        {
            menuControllerPanel.SetActive(true); // 메뉴 컨트롤러 패널 활성화
        }
        else
        {
            Debug.LogWarning("MenuControllerPanel not assigned in inspector!");
        }
    }

    // ------------- PLC IP Add/Remove --------------
    public void AddIP()
    {
        // IP 입력 필드 추가
        if (listIPObject == null) listIPObject = new List<GameObject>();

        GameObject newIPField = Instantiate(plcIPInputFieldPrefab, plcModePanel.transform);
        newIPField.transform.SetParent(plcModePanel.transform, false); // 부모 설정

        newIPField.transform.Find("SubTitle").GetComponent<TextMeshProUGUI>().text = $" Crane {listIPObject.Count + 1}"; // 서브타이틀 업데이트
        newIPField.transform.Find("InputField(TMP)").GetComponent<TMP_InputField>().text = settingDefault.listIP[0];

        listIPObject.Add(newIPField); // 리스트에 추가

        // UI 갱신
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentsPanel);
    }

    public void RemoveIP()
    {
        // 마지막 IP 입력 필드 제거
        // 최소 1개는 남겨두기
        if (listIPObject.Count > 1)
        {
            int lastIndex = listIPObject.Count - 1;
            Destroy(listIPObject[lastIndex]);
            listIPObject.RemoveAt(lastIndex);
        }
        else
        {
            Debug.LogWarning("No IP fields to remove.");
        }

        // UI 갱신
        // panel 제거할 때는 추가할 때와 다르게 UI Update가 한 frame 늦게 적용되어
        // 다음과 같은 조치
        StartCoroutine(RefreshUI());
    }

    // 화면 갱신. 변화 후 다음 frame에 적용
    IEnumerator RefreshUI()
    {
        yield return new WaitForEndOfFrame(); // 다음 프레임까지 대기
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentsPanel);
    }

    private void InitPlaceholder()
    {
        // IP 입력 필드 플레이스홀더 설정
        TMP_InputField inputField;
        foreach (GameObject ipObject in listIPObject)
        {
            inputField = ipObject.transform.Find("InputField(TMP)").GetComponent<TMP_InputField>();
            inputField.placeholder.GetComponent<TextMeshProUGUI>().text = settingDefault.listIP[0]; // 플레이스홀더 설정
        }

        // 키보드 속도 플레이스홀더 설정
        keyGantrySpeed.placeholder.GetComponent<TextMeshProUGUI>().text = $"Range: {range_keyGantrySpeed.x} - {range_keyGantrySpeed.y}";
        keyTrolleySpeed.placeholder.GetComponent<TextMeshProUGUI>().text = $"Range: {range_keyTrolleySpeed.x} - {range_keyTrolleySpeed.y}";
        keySpreaderSpeed.placeholder.GetComponent<TextMeshProUGUI>().text = $"Range: {range_keySpreaderSpeed.x} - {range_keySpreaderSpeed.y}";
        keyMMSpeed.placeholder.GetComponent<TextMeshProUGUI>().text = $"Range: {range_keyMMSpeed.x} - {range_keyMMSpeed.y}";

        // LiDAR 플레이스홀더 설정
        lidarMaxDistance_m.placeholder.GetComponent<TextMeshProUGUI>().text = $"Range: {range_LidarMax_m.x} - {range_LidarMax_m.y}";
        lidarFovHorizontal_deg.placeholder.GetComponent<TextMeshProUGUI>().text = $"Range: {range_FovH_deg.x} - {range_FovH_deg.y}";
        lidarFovVertical_deg.placeholder.GetComponent<TextMeshProUGUI>().text = $"Range: {range_FovV_deg.x} - {range_FovV_deg.y}";
        lidarResHorizontal_deg.placeholder.GetComponent<TextMeshProUGUI>().text = $"Range: {range_Res_deg.x} - {range_Res_deg.y}";
        lidarResVertical_deg.placeholder.GetComponent<TextMeshProUGUI>().text = $"Range: {range_Res_deg.x} - {range_Res_deg.y}";
        lidarNoiseStd.placeholder.GetComponent<TextMeshProUGUI>().text = $"Range: {range_Noise.x} - {range_Noise.y}";

        // Laser 플레이스홀더 설정
        laserMaxDistance_m.placeholder.GetComponent<TextMeshProUGUI>().text = $"Range:  {range_LaserMax_m.x} - {range_LaserMax_m.y}";

        // Yard 플레이스홀더 설정
        yardContainerNumberEA.placeholder.GetComponent<TextMeshProUGUI>().text = $"Range: {range_Containers.x} - {range_Containers.y}";
    }

    // ----------------- Data -> UI -----------------
    // Currnet 설정 데이터를 UI에 반영
    private void InitUIFromData()
    {
        // listIPObject 초기화
        listIPObject = new List<GameObject>();

        // 현재 설정에 따라 IP 입력 필드 생성
        int idx = 0;
        TMP_InputField inputField;
        foreach (string ip in current.listIP)
        {
            // IP Field 생성
            AddIP();

            // 저장된 ip를 입력 필드에 설정
            inputField = listIPObject[idx++].transform.Find("InputField(TMP)").GetComponent<TMP_InputField>();
            Set(inputField, ip);
            inputField.placeholder.GetComponent<TextMeshProUGUI>().text = settingDefault.listIP[0]; // 플레이스홀더 설정
        }

        // 키보드 속도 설정
        Set(keyGantrySpeed, current.keyGantrySpeed);
        Set(keyTrolleySpeed, current.keyTrolleySpeed);
        Set(keySpreaderSpeed, current.keySpreaderSpeed);
        Set(keyMMSpeed, current.keyMMSpeed);

        Set(lidarMaxDistance_m, current.lidarMaxDistance_m);
        Set(lidarFovHorizontal_deg, current.lidarFovHorizontal_deg);
        Set(lidarFovVertical_deg, current.lidarFovVertical_deg);
        Set(lidarResHorizontal_deg, current.lidarResHorizontal_deg);
        Set(lidarResVertical_deg, current.lidarResVertical_deg);
        Set(lidarNoiseStd, current.lidarNoiseStd);

        Set(laserMaxDistance_m, current.laserMaxDistance_m);
        Set(yardContainerNumberEA, current.yardContainerNumberEA);
    }

    // ----------------- UI 읽기 + 검증 -----------------
    // UI 데이터를 Current에 입력
    private bool TryReadAllFields(out SettingParams s)
    {
        // 초기화
        s = new SettingParams
        {
            // PLC IP
            listIP = new List<string>()
        };

        // 유효한 IP 형식인지 확인
        foreach (GameObject ipObject in listIPObject)
        {
            TMP_InputField inputField = ipObject.transform.Find("InputField(TMP)").GetComponent<TMP_InputField>();
            string textIP = GetText(inputField);
            if (!IsValidIp(textIP))
            {
                MarkInvalid(inputField, "Invalid IP");
                return false;
            }
            s.listIP.Add(textIP);
        }

        // 중복 IP 검사
        int idx = 0;
        bool hasDuplicate = false;
        foreach (string ip in s.listIP)
        {
            // 중복 검사
            List<string> listIPcopy = new List<string>(s.listIP);
            listIPcopy.RemoveAt(idx); // 현재 IP를 제외하고 검사

            // 중복된 IP가 있을 때
            if (listIPcopy.Contains(ip))
            {
                TMP_InputField inputField = listIPObject[idx].transform.Find("InputField(TMP)").GetComponent<TMP_InputField>();
                MarkInvalid(inputField, "Invalid IP");
                hasDuplicate = true;
            }

            // 인덱스 증가
            idx++;
        }

        // 중복 IP가 있으면 false 반환
        if (hasDuplicate)
        {
            Debug.Log("[SettingsPanelBinder] 중복된 IP가 있습니다.");
            return false;
        }

        // 키보드 속도
        if (!TryFloat(keyGantrySpeed, range_keyGantrySpeed, out s.keyGantrySpeed)) return false;
        if (!TryFloat(keyTrolleySpeed, range_keyTrolleySpeed, out s.keyTrolleySpeed)) return false;
        if (!TryFloat(keySpreaderSpeed, range_keySpreaderSpeed, out s.keySpreaderSpeed)) return false;
        if (!TryFloat(keyMMSpeed, range_keyMMSpeed, out s.keyMMSpeed)) return false;

        // LiDAR
        if (!TryFloat(lidarMaxDistance_m, range_LidarMax_m, out s.lidarMaxDistance_m)) return false;
        if (!TryFloat(lidarFovHorizontal_deg, range_FovH_deg, out s.lidarFovHorizontal_deg)) return false;
        if (!TryFloat(lidarFovVertical_deg, range_FovV_deg, out s.lidarFovVertical_deg)) return false;
        if (!TryFloat(lidarResHorizontal_deg, range_Res_deg, out s.lidarResHorizontal_deg)) return false;
        if (!TryFloat(lidarResVertical_deg, range_Res_deg, out s.lidarResVertical_deg)) return false;
        if (!TryFloat(lidarNoiseStd, range_Noise, out s.lidarNoiseStd)) return false;

        // Laser
        if (!TryFloat(laserMaxDistance_m, range_LaserMax_m, out s.laserMaxDistance_m)) return false;

        // Yard
        if (!TryInt(yardContainerNumberEA, range_Containers, out s.yardContainerNumberEA)) return false;

        return true;
    }
    void SettingsParamsInit()
    {
        GM.settingParams = new SettingParams();
        GM.CraneType = GM.settingParams.craneType;
        GM.CmdWithPLC = GM.settingParams.cmdWithPLC;
    }

    // ----------------- Helpers: TMP 값 입출력 -----------------
    private static string GetText(TMP_InputField f) => f ? f.text.Trim() : "";
    private static void Set(TMP_InputField f, string v) { if (f) f.SetTextWithoutNotify(v ?? ""); }
    private static void Set(TMP_InputField f, float v) { if (f) f.SetTextWithoutNotify(v.ToString("0.###", CultureInfo.InvariantCulture)); }
    private static void Set(TMP_InputField f, int v) { if (f) f.SetTextWithoutNotify(v.ToString(CultureInfo.InvariantCulture)); }

    // 숫자 파싱 + 범위 검증(+ 에러 표시)
    private bool TryFloat(TMP_InputField f, Vector2 range, out float value)
    {
        value = 0f;
        if (!f || string.IsNullOrWhiteSpace(f.text)) { MarkInvalid(f, "Required"); return false; }

        if (!float.TryParse(f.text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        { MarkInvalid(f, "Number"); return false; }

        // 범위 검증
        if (value < range.x || value > range.y)
        {
            MarkInvalid(f, $"Range: {range.x} - {range.y}");
            return false;
        }

        // 범위 내로 클램프
        value = Mathf.Clamp(value, range.x, range.y);
        f.SetTextWithoutNotify(value.ToString("0.###", CultureInfo.InvariantCulture));

        return true;
    }

    private bool TryInt(TMP_InputField f, Vector2Int range, out int value)
    {
        value = 0;
        if (!f || string.IsNullOrWhiteSpace(f.text)) { MarkInvalid(f, "Required"); return false; }

        if (!int.TryParse(f.text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
        { MarkInvalid(f, "Integer"); return false; }

        value = Mathf.Clamp(value, range.x, range.y);
        // f.SetTextWithoutNotify(value.ToString(CultureInfo.InvariantCulture));
        return true;
    }

    // 간단한 IPv4 검사(0~255.0~255.0~255.0~255)
    private static readonly Regex IpRegex = new Regex(
        @"^(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)\." +
        @"(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)\." +
        @"(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)\." +
        @"(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)$",
        RegexOptions.Compiled);

    private static bool IsValidIp(string ip) => !string.IsNullOrEmpty(ip) && IpRegex.IsMatch(ip);

    // ----------------- 에러 표시(라이트) -----------------
    [Header("Error Visual")]
    [SerializeField] private Color invalidColor = new Color(1f, 0.35f, 0.35f, 1f); // 연한 빨강
    [SerializeField] private Color validColor = Color.white;

    private void MarkInvalid(TMP_InputField f, string _reason = "")
    {
        if (!f) return;
        var img = f.GetComponent<Image>(); // Input 백그라운드에 Image가 있다고 가정
        if (img) img.color = invalidColor;
        Debug.Log(_reason);
        // 필요하면 툴팁/라벨 표시 로직 추가 가능
    }

    // ----------------- 에러 상태 초기화 -----------------
    // 모든 입력 필드의 에러 상태 초기화
    // (색상 초기화)
    private void ClearAllErrorStates()
    {
        TMP_InputField inputField;
        foreach (GameObject ipObject in listIPObject)
        {
            // 저장된 ip를 입력 필드에 설정
            inputField = ipObject.transform.Find("InputField(TMP)").GetComponent<TMP_InputField>();
            ResetColor(inputField);
        }

        ResetColor(lidarMaxDistance_m);
        ResetColor(lidarFovHorizontal_deg);
        ResetColor(lidarFovVertical_deg);
        ResetColor(lidarResHorizontal_deg);
        ResetColor(lidarResVertical_deg);
        ResetColor(lidarNoiseStd);

        ResetColor(laserMaxDistance_m);
        ResetColor(yardContainerNumberEA);
    }

    private void ResetColor(TMP_InputField f)
    {
        if (!f) return;
        var img = f.GetComponent<Image>();
        if (img) img.color = validColor;
    }

    // ----------------- 저장/로드 -----------------
    public void SaveToDisk()
    {
        try
        {
            var json = JsonUtility.ToJson(GM.settingParams, true);
            File.WriteAllText(FilePath, json, Encoding.UTF8);
            Debug.Log("[SettingsParams] 설정 저장 완료: " + FilePath);
        }
        catch (Exception e)
        {
            Debug.LogError("[SettingsParams] 저장 실패: " + e.Message);
        }
    }

    public void LoadFromDisk()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath, Encoding.UTF8);
                var loaded = JsonUtility.FromJson<SettingParams>(json);
                if (loaded != null) current = loaded;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[SettingsParams] 로드 실패(기본값 사용): " + e.Message);
            current = new SettingParams();
        }

        // 전역 설정에 반영
        GM.settingParams = current;
    }
}
