using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System;
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
    [SerializeField] private Button btnPLC;
    [SerializeField] private TextMeshProUGUI btnPLCText;            // "PLC Mode" 버튼
    [SerializeField] private Dropdown dropdownControlMode;    // Input Bay 드롭다운
    [SerializeField] private Button btnStart;
    [SerializeField] private Button btnSetting;           // "Setting" 버튼
    [SerializeField] private Button btnQuit;              // "Quit" 버튼
    [SerializeField] private GameObject settingPanel;       // setting 패널 오브젝트
    [SerializeField] private GameObject gameManager;        // GameManager 오브젝트
    [SerializeField] private GameObject containerPreset;    // 컨테이너 프리셋 오브젝트. 컨테이너 생성
    [SerializeField] private GameObject btnExit;              // "Exit" 버튼


    void Start()
    {
        // 버튼 클릭 이벤트 연결
        // OnEnable/OnDisable에서 Add/Remove를 관리하면
        // 오브젝트 활성/비활성 반복 시 중복 Add 방지에 안전합니다.
        if (btnPLC) btnPLC.onClick.AddListener(OnCraneSelect);
        if (btnSetting) btnSetting.onClick.AddListener(OnSetting);
        if (btnQuit) btnQuit.onClick.AddListener(OnQuit);
        btnStart.onClick.AddListener(StartSimulation);
        // if (btnExit) btnExit.GetComponent<Button>().onClick.AddListener(OnQuit);

        // load setting data
        if (GM.isDataLoaded == false)
        {
            settingPanel.GetComponent<SettingsPanelBinder>().LoadFromDisk();
            GM.isDataLoaded = true;
            Debug.Log("SettingsParams Data Loaded");
        }

        GM.OnChangeCraneType -= OnChangeCraneType;
        GM.OnChangeCraneType += OnChangeCraneType;
        OnChangeCraneType();

        string[] enumNames = Enum.GetNames(typeof(Define.ControlMode));

        // 2. LINQ의 Select 구문으로 문자열 배열을 Dropdown.OptionData 리스트로 변환합니다.
        List<Dropdown.OptionData> options = enumNames
            .Select(name => new Dropdown.OptionData(name))
            .ToList();

        // 3. 생성된 옵션을 드롭다운에 설정합니다.
        dropdownControlMode.options = options;

        dropdownControlMode.onValueChanged.AddListener(ondropdownControlModeValueChanged);
        // 드랍다운 초기값 설정
        if (GM.CmdWithPLC)
            dropdownControlMode.value = enumNames.ToList().IndexOf(Define.ControlMode.PLC.ToString());
        else
            dropdownControlMode.value = enumNames.ToList().IndexOf(Define.ControlMode.Keyboard.ToString());
    }



    public void StartSimulation()
    {
        // save setting data
        // 현재는 Crane Type 변경 시점, control mode 변경 시점에서 저장 2025-12-11
        // settingPanel.GetComponent<SettingsPanelBinder>().SaveToDisk();

        // Crane Type에 맞게 씬 전환
        if (GM.CraneType == Define.CraneType.RTGC)
        {
            Managers.Scene.LoadScene(Define.SceneType.RTGC);
        }

        else if (GM.CraneType == Define.CraneType.RMGC)
        {

            Managers.Scene.LoadScene(Define.SceneType.RMGC);
        }

        else if (GM.CraneType == Define.CraneType.QC)
        {

            // TODO : 임시 Test 씬으로 전환
            Managers.Scene.LoadScene(Define.SceneType.Test);

        }
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

    private void OnCraneSelect()
    {
        Managers.UI.ShowPopupUI<UI_CraneSelectPopup>();
    }

    private void OnChangeCraneType()
    {
        btnPLCText.text = $"{GM.CraneType.ToString()}";

        // save setting data
        settingPanel.GetComponent<SettingsPanelBinder>().SaveToDisk();
    }

    private void ondropdownControlModeValueChanged(int value)
    {
        string selectedMode = dropdownControlMode.options[value].text;
        if (Util.ParseEnum<Define.ControlMode>(selectedMode) == Define.ControlMode.Keyboard)
        {
            GM.CmdWithPLC = false;
        }
        else if (Util.ParseEnum<Define.ControlMode>(selectedMode) == Define.ControlMode.PLC)
        {
            GM.CmdWithPLC = true;
        }

        // save setting data
        settingPanel.GetComponent<SettingsPanelBinder>().SaveToDisk();
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
