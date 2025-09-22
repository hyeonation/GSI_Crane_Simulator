using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // Dropdown을 사용하기 위한 네임스페이스

public class MainLoopTOS : MonoBehaviour
{
    [Header("Dropdown")]
    public Dropdown DropdownInputCrane; // 인스펙터에서 드롭다운을 할당합니다
    public Dropdown DropdownInputBay; // 인스펙터에서 드롭다운을 할당합니다

    [Header("Button")]
    public Button btnSelectCraneUp;
    public Button btnSelectCraneDown;
    public Button btnSelectBayUp;
    public Button btnSelectBayDown;
    public Button btnApply;
    public Button btnTask1;

    //public TextMeshPro textApply;
    public TMP_Text textApply;

    public GameObject containerBlock;



    string strCrane, strContainerID, strSource, strDestination;
    const string defaultStrContainerID = "??";
    const string defaultStrSource = "??";
    const string defaultStrDestination = "??";

    public enum StateUI { None, SelectSrc, Ready }
    public static StateUI stateUI { get; private set; } = StateUI.None;


    enum StateContainerBlock { Active, Deactive, Null }
    Color colorActive = new Color(255f, 255f, 0f, 0f);
    Color colorDeactive = new Color(210f, 210f, 210f, 255f);
    Color colorNull = new Color(0f, 0f, 0f, 255f);

    Transform[,] containerTr;

    readonly Dictionary<string, int> dictRow = new()
    {
        {"A", 0},
        {"B", 1},
        {"C", 2},
        {"D", 3},
        {"E", 4},
        {"F", 5},
        {"G", 6},
        {"H", 7},
        {"J", 8},
        {"WS", 9},
        {"LS", 10},
    };

    void Start()
    {
        //// UI setting
        // Add Listeners
        AddListeners();

        // Init TOS
        InitTOS();

        UpdateStackProfileUI();



        // Transform rowA = containerBlock.transform.Find("Border Line");

        // try
        // {
        //     Debug.Log(rowA.GetChild(0).name);
        // }
        // catch
        // {
        //     Debug.Log("not error");
        // }



        // for (int i = 0; i < rowA.childCount; i++)
        // {
        //     Debug.Log(rowA.GetChild(i).name);
        // }

        // Transform bb = rowA.transform.Find("Tier");
        // Debug.Log(bb == null);
        // Image cc = bb.GetComponent<Image>();
        // cc.color = Color.red;

        // Button aaa = rowA.transform.Find("Tier 1").GetComponent<Button>();
        // aaa.onClick.AddListener(() => OnBtnRow(bb));

        // aa.GetComponent<Button>().transition.

        // disabled btnApply
        btnApply.interactable = false;


    }

    // TOS Task 명령 작업이 끝나면 초기화하는 함수
    void InitTOS()
    {
        // init values
        InitDropdownData();
        InitTextData();


    }

    void InitTextData()
    {
        // default values
        // strCrane 의 기본값은 인덱스 0번째 크레인.
        strContainerID = defaultStrContainerID;
        strSource = defaultStrSource;
        strDestination = defaultStrDestination;

        // apply
        SetApplyText();
    }

    void SetApplyText()
    {
        // make text structure
        textApply.text = $" {strCrane}\n #{strContainerID}\n";
        textApply.text += $" {strSource} -> {strDestination}";
    }

    void InitDropdownData()
    {
        // Declare temp
        List<Dropdown.OptionData> options;

        ///////////////////// 
        /// Crane
        // 드롭다운 옵션 목록을 생성합니다
        options = new List<Dropdown.OptionData>();
        for (int i = 0; i < GM.nameCranes.Length; i++)
        {
            options.Add(new Dropdown.OptionData(GM.nameCranes[i]));
        }

        // 생성된 옵션을 드롭다운에 설정합니다
        DropdownInputCrane.options = options;
        OnDdInputCraneValueChanged(0);  // dropdown 초기화

        ///////////////////// 
        /// Bay
        // 드롭다운 옵션 목록을 생성합니다
        options = new List<Dropdown.OptionData>();
        for (int i = 0; i < GM.bay; i++)
        {
            options.Add(new Dropdown.OptionData($"#{i}"));
        }

        // 생성된 옵션을 드롭다운에 설정합니다
        DropdownInputBay.options = options;
    }

    void AddListeners()
    {
        // Drowdown Listener
        // 선택된 값이 변경될 때 호출될 이벤트를 등록합니다
        DropdownInputCrane.onValueChanged.AddListener(OnDdInputCraneValueChanged);
        DropdownInputBay.onValueChanged.AddListener(OnDdInputBayValueChanged);

        // Button Listener
        if (btnSelectCraneUp) btnSelectCraneUp.onClick.AddListener(OnBtnSelectCraneUp);
        if (btnSelectCraneDown) btnSelectCraneDown.onClick.AddListener(OnBtnSelectCraneDown);
        if (btnSelectBayUp) btnSelectBayUp.onClick.AddListener(OnBtnSelectBayUp);
        if (btnSelectBayDown) btnSelectBayDown.onClick.AddListener(OnBtnSelectBayDown);

        // containerBlock setting
        // 버튼마다 기능 부여
        containerTr = new Transform[GM.row + 2, GM.tier];   // row : += WS, LS 2개 추가
        Transform row;
        int idxRow = 0;     // init
        for (int i = 0; i < containerBlock.transform.childCount; i++)
        {
            // get row
            row = containerBlock.transform.GetChild(i);

            // get container block unit
            // child가 없는 Border Line GameObject는 loop 돌지 않음.
            int idxTier = 0;    // init
            for (int j = 0; j < row.childCount; j++)
            {
                // get container block unit
                Transform containerBlockUnit = row.GetChild(j);

                // Only Tier
                if (containerBlockUnit.name.Contains("Tier"))
                {
                    // 버튼마다 기능 부여
                    containerBlockUnit.GetComponent<Button>().onClick.AddListener(() => OnBtnRow(containerBlockUnit));

                    // set containerTr
                    containerTr[idxRow, idxTier++] = containerBlockUnit;
                }
            }

            // update
            // only idxTier != 0
            idxRow = (idxTier != 0) ? (idxRow + 1) : idxRow;
        }
    }

    // 드롭다운 값이 변경될 때 호출되는 함수
    void OnDdInputCraneValueChanged(int index)
    {

        // select crane
        strCrane = DropdownInputCrane.options[index].text;
        SetApplyText();

        // // index는 선택된 옵션의 순서(0부터 시작)입니다
        // Debug.Log("선택된 인덱스: " + index);
        // Debug.Log("선택된 텍스트: " + DropdownInputCrane.options[index].text);

        // // 인덱스 값에 따라 다른 동작을 수행합니다
        // switch (index)
        // {
        //     case 0:
        //         Debug.Log("옵션 1이 선택되었습니다.");
        //         break;
        //     case 1:
        //         Debug.Log("옵션 2가 선택되었습니다.");
        //         break;
        //     case 2:
        //         Debug.Log("옵션 3이 선택되었습니다.");
        //         break;
        // }
    }

    void OnDdInputBayValueChanged(int index)
    {
        // stack profile update
        UpdateStackProfileUI();
    }

    void OnBtnSelectCraneUp() => OnBtnUpDownEvent(DropdownInputCrane, '-');
    void OnBtnSelectCraneDown() => OnBtnUpDownEvent(DropdownInputCrane, '+');
    void OnBtnSelectBayUp() => OnBtnUpDownEvent(DropdownInputBay, '+');
    void OnBtnSelectBayDown() => OnBtnUpDownEvent(DropdownInputBay, '-');

    void OnBtnUpDownEvent(Dropdown dropdown, char direction)
    {
        // present index
        int idx = dropdown.value;

        // next index
        int nextIdx = (direction == '+') ? (idx + 1) : (idx - 1);

        // load nextIndex to dropdown.
        // adding limit
        dropdown.value = Math.Clamp(nextIdx, 0, dropdown.options.Count);
    }

    string mkStrContainerLoc(Transform btn)
    {
        // output
        string output;

        string strRow = btn.transform.parent.name;  // 부모 이름 접근. Row
        int row = dictRow[strRow];
        int bay = DropdownInputBay.value;

        // Normal
        if (row < GM.row)
        {
            int tier = GM.stack_profile[row, bay];
            output = $"{strRow}{bay}-{tier}";
        }

        // WS or LS
        else
        {
            output = $"{strRow}{bay}";
        }

        return output;
    }

    void OnBtnRow(Transform btn)
    {

        // Image cc = btn.GetComponent<Image>();
        // cc.color = Color.yellow;

        // input
        string strContainerLoc = mkStrContainerLoc(btn);

        // Source 선택
        if (stateUI == StateUI.None)
        {
            // make data
            // strContainerID = 
            strSource = strContainerLoc;

            // update stateUI
            stateUI = StateUI.SelectSrc;
        }

        else if (stateUI == StateUI.SelectSrc)
        {
            // 같은 row, bay 눌렀으면 취소
            if (strContainerLoc == strSource)
            {
                strSource = defaultStrSource;
                stateUI = StateUI.None;
            }

            // 다른 row, bay 눌렀으면 준비 완료
            else
            {
                strDestination = strContainerLoc;
                stateUI = StateUI.Ready;

                // Enable btnApply
                btnApply.interactable = true;
            }
        }

        else if (stateUI == StateUI.Ready)
        {
            // 같은 row, bay 눌렀으면 취소
            // rollback
            if (strContainerLoc == strDestination)
            {
                // disable btnApply
                btnApply.interactable = false;

                strDestination = defaultStrDestination;
                stateUI = StateUI.SelectSrc;
            }

            // 다른 row, bay 눌렀으면 Destination 변경
            // StateUI 는 변화 없음
            else
            {
                strDestination = strContainerLoc;
            }
        }
    }

    void OnBtnApply()
    {
        // load data

        // update stack profile data

        // initialization
        btnApply.interactable = false;
        strSource = defaultStrSource;
        strDestination = defaultStrDestination;
        stateUI = StateUI.None;

    }

    // update current stack profile UI
    void UpdateStackProfileUI()
    {

        // init data
        for (int i = 0; i < containerTr.GetLength(0); i++)
        {
            for (int j = 0; j < containerTr.GetLength(1); j++)
            {
                try
                {
                    containerTr[i, j].GetComponent<Image>().color = colorNull;
                }
                catch
                {
                    // WS, LS 초과하는 인덱스 처리
                    break;
                }
            }
        }

        // 현재 bay
        int iBayNow = DropdownInputBay.value;

        // 해당 bay로 containerID 배치 및 활성화
        int iRow, iBay, iTier;
        Transform cntr;
        for (int i = 0; i < GM.list_stack_profile.Count; i++)
        {
            // [i_row, i_bay, i_tier, containerStatus]
            int[] containerData = GM.list_stack_profile[i];
            iRow = containerData[0];
            iBay = containerData[1];
            iTier = containerData[2];

            // bay 같을 때
            if (iBayNow == iBay)
            {
                // transform 추출
                cntr = containerTr[iRow, iTier];

                // containerID 배치
                cntr.name = GM.ByteArrayToString(GM.listContainerID[i]);

                // 색 변경
                cntr.GetComponent<Image>().color = colorDeactive;
            }
        }



    }

}