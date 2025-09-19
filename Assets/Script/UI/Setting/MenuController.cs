using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 메인 메뉴 UI 컨트롤러.
/// - 타이틀/버튼 레퍼런스를 인스펙터에서 연결
/// - 각 버튼 클릭 시 모드 설정, 설정 패널 열기, 종료 등 수행
/// - 선택 시 즉시 시뮬레이터 씬으로 전환할지 여부를 옵션으로 제어
/// </summary>
public class MenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;   // 상단 타이틀 텍스트
    [SerializeField] private Button btnKeyboard;          // "Keyboard Mode" 버튼
    [SerializeField] private Button btnPLC;               // "PLC Mode" 버튼
    [SerializeField] private Button btnSetting;           // "Setting" 버튼
    [SerializeField] private Button btnQuit;              // "Quit" 버튼
    [SerializeField] private GameObject settingPanel;       // setting 패널 오브젝트
    [SerializeField] private GameObject gameManager;        // GameManager 오브젝트
    [SerializeField] private GameObject containerPreset;    // 컨테이너 프리셋 오브젝트. 컨테이너 생성


    void Start()
    {
        // 버튼 클릭 이벤트 연결
        // OnEnable/OnDisable에서 Add/Remove를 관리하면
        // 오브젝트 활성/비활성 반복 시 중복 Add 방지에 안전합니다.
        if (btnKeyboard) btnKeyboard.onClick.AddListener(OnKeyboardMode);
        if (btnPLC) btnPLC.onClick.AddListener(OnPLCMode);
        if (btnSetting) btnSetting.onClick.AddListener(OnSetting);
        if (btnQuit) btnQuit.onClick.AddListener(OnQuit);

        // load setting data
        settingPanel.GetComponent<SettingsPanelBinder>().LoadFromDisk();
    }

    /// <summary>
    /// Keyboard Mode 선택 시 호출
    /// </summary>
    public void OnKeyboardMode()
    {
        GameMode.Set(GameMode.Mode.Keyboard);    // 전역 모드 저장
        Debug.Log("Mode set to: Keyboard");

        GM.cmdWithPLC = false; // PLC 모드 비활성화
        Debug.Log("PLC mode disabled");

        StartSimulation(); // 시뮬레이션 시작
    }

    /// <summary>
    /// PLC Mode 선택 시 호출
    /// </summary>
    public void OnPLCMode()
    {
        GameMode.Set(GameMode.Mode.PLC);         // 전역 모드 저장
        Debug.Log("Mode set to: PLC");

        GM.cmdWithPLC = true; // PLC 모드 활성화
        Debug.Log("PLC mode enabled");

        StartSimulation(); // 시뮬레이션 시작
    }

    public void StartSimulation()
    {
        gameObject.SetActive(false); // 현재 메뉴 숨김

        // container 생성
        containerPreset.GetComponent<Container>().enabled = true;

        // OrganizingData 켜기
        // SettingsPanel에서 변경한 키보드 모드 속도값 초기화 위해
        gameManager.GetComponent<MainLoopARTG>().enabled = true;
    }

    /// <summary>
    /// 설정 버튼 클릭 시 호출
    /// 실제로는 설정 패널을 열거나 별도 씬/팝업을 띄우도록 구현하세요.
    /// </summary>
    public void OnSetting()
    {
        // 예: SettingsPanel.SetActive(true);
        Debug.Log("Open Settings (TODO: 연결할 패널/팝업)");

        gameObject.SetActive(false); // 현재 메뉴 숨김
        if (settingPanel)
        {
            settingPanel.SetActive(true); // 설정 패널 활성화
        }
        else
        {
            Debug.LogWarning("Setting panel not assigned in inspector!");
        }
    }

    /// <summary>
    /// 종료 버튼 클릭 시 호출
    /// 에디터와 빌드 환경에서의 동작을 분기 처리합니다.
    /// </summary>
    public void OnQuit()
    {
#if UNITY_EDITOR
        // 에디터에서는 플레이 모드 종료
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 실제 빌드에서는 애플리케이션 종료
        Application.Quit();
#endif
    }
}
