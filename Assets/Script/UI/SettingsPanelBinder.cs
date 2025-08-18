using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    [Header("PLC Mode")]
    [SerializeField] private TMP_InputField plcIp;

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
    [SerializeField] private Button applyButton;
    [SerializeField] private GameObject menuControllerPanel;       // menuControllerPanel 패널 오브젝트

    // ----------------- 설정 데이터 모델 -----------------
    [Serializable]
    public class SimulatorSettings
    {
        public string plcIp = "192.168.100.101";
        public float lidarMaxDistance_m = 100f;
        public float lidarFovHorizontal_deg = 90f;
        public float lidarFovVertical_deg = 30f;
        public float lidarResHorizontal_deg = 0.2f;
        public float lidarResVertical_deg = 0.2f;
        public float lidarNoiseStd = 0.01f;
        public float laserMaxDistance_m = 50f;
        public int yardContainerNumberEA = 100;
    }

    public static SimulatorSettings Current { get; private set; } = new SimulatorSettings();

    /// <summary>외부에서 구독: Apply 직후 최신 설정 전달</summary>
    public static event Action<SimulatorSettings> OnApplied;

    // ----------------- 범위(필요시 인스펙터에서 조정) -----------------
    [Header("Validation Ranges")]
    [SerializeField] private Vector2 range_LidarMax_m = new Vector2(1f, 10000f);
    [SerializeField] private Vector2 range_FovH_deg = new Vector2(1f, 360f);
    [SerializeField] private Vector2 range_FovV_deg = new Vector2(1f, 180f);
    [SerializeField] private Vector2 range_Res_deg = new Vector2(0.01f, 10f);
    [SerializeField] private Vector2 range_Noise = new Vector2(0f, 100f);
    [SerializeField] private Vector2 range_LaserMax_m = new Vector2(1f, 10000f);
    [SerializeField] private Vector2Int range_Containers = new Vector2Int(0, 1000000);

    // ----------------- 파일 저장 경로 -----------------
    private static string FilePath =>
        Path.Combine(Application.persistentDataPath, "settings.json");

    // ----------------- Unity lifecycle -----------------
    void Awake()
    {
        // 버튼 연결
        if (applyButton) applyButton.onClick.AddListener(ApplyAndSave);

        // 저장된 설정 로드 → UI 채우기
        LoadFromDisk();
        PopulateUIFromData();
        ClearAllErrorStates();
    }

    void OnDestroy()
    {
        if (applyButton) applyButton.onClick.RemoveListener(ApplyAndSave);
    }

    // ----------------- UI -> Data 적용 & 저장 -----------------
    public void ApplyAndSave()
    {
        ClearAllErrorStates();

        // 입력값 읽기 + 검증
        bool ok = TryReadAllFields(out var updated);
        if (!ok)
        {
            Debug.LogWarning("[SettingsPanelBinder] 입력값에 오류가 있어 저장하지 않았습니다.");
            return;
        }

        // 반영
        Current = updated;

        // 저장
        SaveToDisk(Current);

        // 브로드캐스트
        OnApplied?.Invoke(Current);

        Debug.Log("[SettingsPanelBinder] 설정 적용/저장 완료: " + FilePath);

        gameObject.SetActive(false); // 현재 메뉴 숨김
        if (menuControllerPanel)
        {
            menuControllerPanel.SetActive(true); // 메뉴 컨트롤러 패널 활성화
        }
        else
        {
            Debug.LogWarning("MenuControllerPanel not assigned in inspector!");
        }
    }

    // ----------------- Data -> UI -----------------
    private void PopulateUIFromData()
    {
        Set(plcIp, Current.plcIp);

        Set(lidarMaxDistance_m, Current.lidarMaxDistance_m);
        Set(lidarFovHorizontal_deg, Current.lidarFovHorizontal_deg);
        Set(lidarFovVertical_deg, Current.lidarFovVertical_deg);
        Set(lidarResHorizontal_deg, Current.lidarResHorizontal_deg);
        Set(lidarResVertical_deg, Current.lidarResVertical_deg);
        Set(lidarNoiseStd, Current.lidarNoiseStd);

        Set(laserMaxDistance_m, Current.laserMaxDistance_m);
        Set(yardContainerNumberEA, Current.yardContainerNumberEA);
    }

    // ----------------- UI 읽기 + 검증 -----------------
    private bool TryReadAllFields(out SimulatorSettings s)
    {
        s = new SimulatorSettings();

        // PLC IP
        string ip = GetText(plcIp);
        if (!IsValidIp(ip))
        {
            MarkInvalid(plcIp, "Invalid IP");
            return false;
        }
        s.plcIp = ip;

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

    // ----------------- Helpers: TMP 값 입출력 -----------------
    private static string GetText(TMP_InputField f) => f ? f.text.Trim() : "";
    private static void Set(TMP_InputField f, string v) { if (f) f.SetTextWithoutNotify(v ?? ""); }
    private static void Set(TMP_InputField f, float v) { if (f) f.SetTextWithoutNotify(v.ToString("0.###", CultureInfo.InvariantCulture)); }
    private static void Set(TMP_InputField f, int v)   { if (f) f.SetTextWithoutNotify(v.ToString(CultureInfo.InvariantCulture)); }

    // 숫자 파싱 + 범위 검증(+ 에러 표시)
    private bool TryFloat(TMP_InputField f, Vector2 range, out float value)
    {
        value = 0f;
        if (!f || string.IsNullOrWhiteSpace(f.text)) { MarkInvalid(f, "Required"); return false; }

        if (!float.TryParse(f.text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        { MarkInvalid(f, "Number"); return false; }

        value = Mathf.Clamp(value, range.x, range.y);
        // 다시 표시(클램프 결과를 UI에 반영하고 싶다면 주석 해제)
        // f.SetTextWithoutNotify(value.ToString("0.###", CultureInfo.InvariantCulture));
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
        // 필요하면 툴팁/라벨 표시 로직 추가 가능
    }

    private void ClearAllErrorStates()
    {
        ResetColor(plcIp);

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
    private static void SaveToDisk(SimulatorSettings s)
    {
        try
        {
            var json = JsonUtility.ToJson(s, true);
            File.WriteAllText(FilePath, json, Encoding.UTF8);
        }
        catch (Exception e)
        {
            Debug.LogError("[SettingsPanelBinder] 저장 실패: " + e.Message);
        }
    }

    private static void LoadFromDisk()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath, Encoding.UTF8);
                var loaded = JsonUtility.FromJson<SimulatorSettings>(json);
                if (loaded != null) Current = loaded;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[SettingsPanelBinder] 로드 실패(기본값 사용): " + e.Message);
            Current = new SimulatorSettings();
        }
    }
}
