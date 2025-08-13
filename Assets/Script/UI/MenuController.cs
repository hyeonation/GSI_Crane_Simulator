using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// 메인 메뉴 UI 컨트롤러.
/// - 타이틀/버튼 레퍼런스를 인스펙터에서 연결
/// - 각 버튼 클릭 시 모드 설정, 설정 패널 열기, 종료 등 수행
/// - 선택 시 즉시 시뮬레이터 씬으로 전환할지 여부를 옵션으로 제어
/// </summary>
public class MenuController : MonoBehaviour
{
    [Header("UI References (인스펙터에서 드래그 연결)")]
    [SerializeField] private TextMeshProUGUI titleText;   // 상단 타이틀 텍스트
    [SerializeField] private Button btnKeyboard;          // "Keyboard Mode" 버튼
    [SerializeField] private Button btnPLC;               // "PLC Mode" 버튼
    [SerializeField] private Button btnSetting;           // "Setting" 버튼
    [SerializeField] private Button btnQuit;              // "Quit" 버튼

    [Header("Behaviour Config")]
    [Tooltip("모드 선택 시 로드할 시뮬레이터 씬 이름 (Build Settings에 등록 필요)")]
    [SerializeField] private string simulatorSceneName = "Simulator";

    [Tooltip("모드 선택 시 즉시 시뮬레이터 씬을 로드할지 여부")]
    [SerializeField] private bool loadSceneOnModeSelect = true;

    private void OnEnable()
    {
        // 버튼 클릭 이벤트 연결
        // OnEnable/OnDisable에서 Add/Remove를 관리하면
        // 오브젝트 활성/비활성 반복 시 중복 Add 방지에 안전합니다.
        if (btnKeyboard) btnKeyboard.onClick.AddListener(OnKeyboardMode);
        if (btnPLC)      btnPLC.onClick.AddListener(OnPLCMode);
        if (btnSetting)  btnSetting.onClick.AddListener(OnSetting);
        if (btnQuit)     btnQuit.onClick.AddListener(OnQuit);
    }

    private void OnDisable()
    {
        // 리스너 정리(메모리 누수/중복 호출 방지)
        if (btnKeyboard) btnKeyboard.onClick.RemoveListener(OnKeyboardMode);
        if (btnPLC)      btnPLC.onClick.RemoveListener(OnPLCMode);
        if (btnSetting)  btnSetting.onClick.RemoveListener(OnSetting);
        if (btnQuit)     btnQuit.onClick.RemoveListener(OnQuit);
    }

    /// <summary>
    /// 버튼에 달린 TextMeshProUGUI 자식 라벨을 찾아 텍스트를 설정.
    /// Button 하위에 Label(TMP)이 있다는 전제입니다.
    /// </summary>
    private void SetButtonLabel(Button b, string text)
    {
        if (!b) return;

        // GetComponentInChildren(true): 비활성 자식도 검색
        var tmp = b.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp) tmp.text = text;
    }

    /// <summary>
    /// Keyboard Mode 선택 시 호출
    /// </summary>
    public void OnKeyboardMode()
    {
        GameMode.Set(GameMode.Mode.Keyboard);    // 전역 모드 저장
        Debug.Log("Mode set to: Keyboard");

        // 즉시 시뮬레이터 씬 로드 옵션이 켜져 있으면 이동
        TryLoadSimulatorScene();
    }

    /// <summary>
    /// PLC Mode 선택 시 호출
    /// </summary>
    public void OnPLCMode()
    {
        GameMode.Set(GameMode.Mode.PLC);         // 전역 모드 저장
        Debug.Log("Mode set to: PLC");
        TryLoadSimulatorScene();
    }

    /// <summary>
    /// 설정 버튼 클릭 시 호출
    /// 실제로는 설정 패널을 열거나 별도 씬/팝업을 띄우도록 구현하세요.
    /// </summary>
    public void OnSetting()
    {
        // 예: SettingsPanel.SetActive(true);
        Debug.Log("Open Settings (TODO: 연결할 패널/팝업)");
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

    /// <summary>
    /// 옵션에 따라 시뮬레이터 씬을 로드.
    /// - 씬 이름이 비어있지 않아야 하며
    /// - Build Settings에 해당 씬이 등록되어 있어야 합니다.
    /// </summary>
    private void TryLoadSimulatorScene()
    {
        if (!loadSceneOnModeSelect) return;
        if (string.IsNullOrEmpty(simulatorSceneName)) return;

        // 씬 로드(싱글). 필요 시 Additive로 변경 가능
        SceneManager.LoadScene(simulatorSceneName);
    }
}
