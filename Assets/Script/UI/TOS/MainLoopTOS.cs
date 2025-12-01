using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // Dropdown을 사용하기 위한 네임스페이스
using System.Text.RegularExpressions;

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
    public Button btnReset;

    //public TextMeshPro textApply;
    public TMP_Text textApply;

    public GameObject containerBlock;
    public Transform containersTemp;
    public GameObject[] containerPrefabs;

    public GameObject[] listTaskObj;


    string strCrane, strContainerID, strSource, strDestination;
    const string defaultStrContainerID = "??";
    const string defaultStrSource = "??";
    const string defaultStrDestination = "??";

    public enum StateUI { Normal, SrcSelected, Ready }
    public static StateUI stateUI { get; private set; } = StateUI.Normal;


    enum StateContainerBlock { Active, Deactive, Null }
    Color colorActive = new(255f, 255f, 0f, 255f);
    Color colorDeactive = new(210f, 210f, 210f, 255f);
    Color colorNull = new(0f, 0f, 0f, 255f);
    Transform[,] containerTr;

    Transform btnSource, btnDestination;
    const int defaultIdx = -1;
    int iRowSource = defaultIdx, iRowDestination = defaultIdx;
    int iBaySource = defaultIdx, iBayDestination = defaultIdx;

    // task
    List<TaskInfo> listTaskInfo = new();
    int jobID = 0;


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
        // init values
        InitDropdownData();

        //// UI setting
        // Add Listeners
        AddListeners();

        // Init
        Reset();
    }

    void AddListeners()
    {
        // Drowdown Listener
        // 선택된 값이 변경될 때 호출될 이벤트를 등록합니다
        DropdownInputCrane.onValueChanged.AddListener(OnDdInputCraneValueChanged);
        DropdownInputBay.onValueChanged.AddListener(OnDdInputBayValueChanged);

        // arrow Button Listener
        if (btnSelectCraneUp) btnSelectCraneUp.onClick.AddListener(OnBtnSelectCraneUp);
        if (btnSelectCraneDown) btnSelectCraneDown.onClick.AddListener(OnBtnSelectCraneDown);
        if (btnSelectBayUp) btnSelectBayUp.onClick.AddListener(OnBtnSelectBayUp);
        if (btnSelectBayDown) btnSelectBayDown.onClick.AddListener(OnBtnSelectBayDown);

        // Apply button
        if (btnApply) btnApply.onClick.AddListener(OnBtnApply);

        // Reset button
        if (btnReset) btnReset.onClick.AddListener(Reset);

        // task list
        for (int i = 0; i < listTaskObj.Length; i++)
        {
            int idx = i;    // int 새로 생성해야 값이 제대로 들어감.
            listTaskObj[idx].GetComponent<Button>().onClick.AddListener(() => OnBtnTaskList(idx));
        }


        // containerBlock setting
        // 버튼마다 기능 부여
        containerTr = new Transform[GM.lengthRow + 2, GM.lengthTier];   // row : += WS, LS 2개 추가
        Transform row;
        int iRow = 0;     // init
        for (int i = 0; i < containerBlock.transform.childCount; i++)
        {
            // get row
            row = containerBlock.transform.GetChild(i);

            // get container block unit
            // child가 없는 Border Line GameObject는 loop 돌지 않음.
            int iTier = 0;    // init
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
                    containerTr[iRow, iTier++] = containerBlockUnit;
                }
            }

            // update
            // only idxTier != 0
            // hild가 없는 Border Line을 제외하고 모두 해당
            iRow = (iTier != 0) ? (iRow + 1) : iRow;
        }

        string[] wsls = { "WS", "LS" };
        foreach (string strRow in wsls)
        {
            Transform containerBlockUnit = containerBlock.transform.Find(strRow).Find("ID");
            containerBlockUnit.GetComponent<Button>().onClick.AddListener(() => OnBtnIDwsls(containerBlockUnit));
        }
    }

    // TOS Task 명령 작업이 끝나면 초기화하는 함수
    void Reset()
    {
        RollbackDestination();
        RollbackSource();
        InitTextData();
        UpdateStackProfileUI();
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
        for (int i = 0; i < GM.lengthBay; i++)
        {
            options.Add(new Dropdown.OptionData($"#{i}"));
        }

        // 생성된 옵션을 드롭다운에 설정합니다
        DropdownInputBay.options = options;
    }

    void InitTextData()
    {
        // default values
        // strCrane 의 기본값은 인덱스 0번째 크레인.
        strContainerID = defaultStrContainerID;
        strSource = defaultStrSource;
        strDestination = defaultStrDestination;

        // apply
        UpdateApplyText();

        // RefreshTaskList
        RefreshTaskList();
    }

    void RefreshTaskList()
    {
        // init task text
        for (int i = 0; i < listTaskObj.Length; i++)
            GetTaskContent(i).text = "";

        // add task data
        // loop count가 taskObject 개수 초과하지 않는 선에서
        int loopCount = Math.Clamp(listTaskInfo.Count, 0, listTaskObj.Length);
        for (int i = 0; i < loopCount; i++)
            GetTaskContent(i).text = listTaskInfo[i].getContent();
    }

    TMP_Text GetTaskContent(int i)
        => listTaskObj[i].transform.Find("Text (TMP)").GetComponent<TMP_Text>();

    void BtnApplyOnOff(bool interactable)
    {
        // interactable
        btnApply.interactable = interactable;

        // Enabled
        if (interactable)
        {
            // color
            // btnApply.GetComponent<Image>().color = colorApplyActive;
            btnApply.transform.Find("Text (TMP)").GetComponent<TMP_Text>().color = Color.black;
        }

        // disabled
        else
        {
            // color
            // btnApply.GetComponent<Image>().color = colorApplyDeactive;
            btnApply.transform.Find("Text (TMP)").GetComponent<TMP_Text>().color = Color.white;
        }
    }

    // Apply text structure
    void UpdateApplyText()
    {
        // make text structure
        textApply.text = $" {strCrane}\n #{strContainerID}\n";
        textApply.text += $" {strSource} -> {strDestination}";
    }

    // Dropdown 값이 변경될 때 호출되는 함수
    void OnDdInputCraneValueChanged(int index)
    {
        // select crane
        strCrane = DropdownInputCrane.options[index].text;
        UpdateApplyText();

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

    // Dropdown Bay 값 바뀔 때 호출되는 함수
    void OnDdInputBayValueChanged(int index)
    {
        // stack profile update
        UpdateStackProfileUI();
    }

    // button event 함수
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

    // task 눌렀을 때 삭제되는 기능
    // 메세지 박스 출력 후 삭제 여부 선택해야 삭제되도록
    void OnBtnTaskList(int idx)
    {
        try
        {
            // delete task
            listTaskInfo.RemoveAt(idx);
            RefreshTaskList();
        }
        catch
        {
            // no item
            Debug.Log("No Item");
        }
    }

    void OnBtnIDwsls(Transform btn)
    {
        if (stateUI == StateUI.Normal)
        {
            // get data
            string strIRow = btn.transform.parent.name;  // 부모 이름 접근. Row        
            int iRow = dictRow[strIRow];
            int iBay = DropdownInputBay.value;
            int iTier = GM.stack_profile[iRow, iBay] - 1;   // iTier = Tier - 1

            // Container 없으면 추가
            if (iTier < 0)
            {
                // make GameObject
                GameObject newObject = Container.mkRandomPrefab(containerPrefabs);
                newObject.transform.SetParent(containersTemp);

                // update data
                iTier = 0;
                int[] sp = { iRow, iBay, iTier, 0 };
                GM.list_stack_profile.Add(sp);
                GM.stack_profile[iRow, iBay] = 1;
            }

            // Container 있으면 삭제
            else
            {
                //// delete data

                // delete GameObject
                string containerIDFormat = btn.transform.parent.Find("Tier").GetChild(0).GetComponent<TMP_Text>().text;
                string strContainerIDTemp = Regex.Replace(containerIDFormat, @"\s", "");
                Destroy(containersTemp.Find(strContainerIDTemp).gameObject);

                // find idx
                int idxDelete = -1;
                for (int i = 0; i < GM.listContainerID.Count; i++)
                {
                    string cnid = GM.ByteArrayToString(GM.listContainerID[i]);
                    if (cnid == strContainerIDTemp)
                    {
                        idxDelete = i;
                        break;
                    }
                }

                // delete data
                GM.listContainerID.RemoveAt(idxDelete);
                GM.list_stack_profile.RemoveAt(idxDelete);
                GM.stack_profile[iRow, iBay] = 0;
            }

            // update UI
            UpdateStackProfileUI();
        }
    }

    void OnBtnRow(Transform btn)
    {
        // get data
        string strIRow = btn.transform.parent.name;  // 부모 이름 접근. Row
        int iRow = dictRow[strIRow];
        int iBay = DropdownInputBay.value;
        int iTier = GM.stack_profile[iRow, iBay] - 1;   // iTier = Tier - 1

        // strContainerLoc
        string strContainerLoc = mkStrContainerLoc(strIRow, iBay, iTier);

        // Normal state에서 Source 선택
        if (stateUI == StateUI.Normal) UpdateSource(strIRow);

        // SrcSelected에서 Destination 선택
        else if (stateUI == StateUI.SrcSelected)
        {
            // 같은 row, bay 눌렀으면 취소
            if (strContainerLoc == strSource) RollbackSource();

            // 다른 row, bay 눌렀으면 Destination 결정
            // 준비 완료
            // tier 초과하지 않았을 때만
            else
                if (iTier < GM.lengthTier - 1) UpdateDestination(strIRow);
        }

        // Ready 상태에서 선택
        else if (stateUI == StateUI.Ready)
        {

            // tier 초과하지 않았을 때만
            // 한 층 더 쌓을 수 있을 때
            if (iTier < GM.lengthTier - 1)
            {
                // iTier = iTier + 1
                strContainerLoc = mkStrContainerLoc(strIRow, iBay, ++iTier);

                // 기존 Dst와 같은 row, bay 눌렀으면 취소
                // rollback -> SrcSelected
                if (strContainerLoc == strDestination) RollbackDestination();

                // 기존 Dst와 다른 row, bay 눌렀으면 Destination 변경
                // StateUI 는 변화 없음
                else
                {
                    // destination 변경
                    // source와 destination이 다를 때만
                    if ((iRowSource != iRow) || (iBaySource != iBay))
                    {
                        RollbackDestination();
                        UpdateDestination(strIRow);
                    }
                }
            }
        }

        // update ApplyText
        UpdateApplyText();
    }

    void UpdateSource(string strIRow)
    {
        // iRow, iBay
        int iRow = dictRow[strIRow];
        int iBay = DropdownInputBay.value;

        // iTier = Tier - 1
        int iTier = GM.stack_profile[iRow, iBay] - 1;

        // not empty row
        if (iTier >= 0)
        {
            // 해당 row의 최상단 Container 선택
            btnSource = containerTr[iRow, iTier];
            btnSource.GetComponent<Image>().color = colorActive;    // active

            //////////////////////////
            // update data
            string containerIDFormat = btnSource.GetChild(0).GetComponent<TMP_Text>().text;
            strContainerID = Regex.Replace(containerIDFormat, @"\s", "");
            iRowSource = iRow;
            iBaySource = iBay;
            strSource = mkStrContainerLoc(strIRow, iBay, iTier);
            stateUI = StateUI.SrcSelected;
        }
    }

    void UpdateDestination(string strIRow)
    {
        // iRow, iBay
        int iRow = dictRow[strIRow];
        int iBay = DropdownInputBay.value;

        // iTier = Tier - 1
        // Destination은 한층 더 높은 값으로 해야 하므로 + 1
        // iTier = Tier - 1 (+ 1) = Tier
        int iTier = GM.stack_profile[iRow, iBay];

        // 해당 row의 최상단 Container 한 칸 위 선택
        btnDestination = containerTr[iRow, iTier];
        btnDestination.GetComponent<Image>().color = colorActive;    // active

        //////////////////////////
        // update data
        iRowDestination = iRow;
        iBayDestination = iBay;
        strDestination = mkStrContainerLoc(strIRow, iBay, iTier);
        stateUI = StateUI.Ready;

        // Enable btnApply
        BtnApplyOnOff(true);
    }

    void RollbackSource()
    {
        // update data
        iRowSource = defaultIdx;
        iBaySource = defaultIdx;
        strContainerID = defaultStrContainerID;
        strSource = defaultStrSource;
        stateUI = StateUI.Normal;

        // deactive
        if (btnSource) btnSource.GetComponent<Image>().color = colorDeactive;

        // update button
        btnSource = null;
    }

    void RollbackDestination()
    {
        // Load iBay
        int iBay = DropdownInputBay.value;

        // 기존 Destination의 Bay와 같은 경우에만 Null
        // Bay가 다르다면 기존 Destination container가 없으므로 할 필요 없음.
        // Bay와 관계 없이 null 하게 되면 다른 Bay의 deactive 상태였던 Container가 Null 될 수 있음.
        // Destination block은 빈 block이 선택되므로 colorNull로 설정.
        // btnDestination이 없는 초기에도 ftn을 사용하기 위해 발동 조건에 btnDestination을 &&로 추가
        if (btnDestination && (iBayDestination == iBay)) btnDestination.GetComponent<Image>().color = colorNull;

        // update data
        iRowDestination = defaultIdx;
        iBayDestination = defaultIdx;
        strDestination = defaultStrDestination;
        stateUI = StateUI.SrcSelected;

        // update button
        btnDestination = null;

        // disable btnApply
        BtnApplyOnOff(false);
    }

    void OnBtnApply()
    {
        // generate Task
        TaskInfo taskInfo = new();

        // set task data
        taskInfo.jobID = jobID++;
        taskInfo.strCrane = strCrane;
        taskInfo.strContainerID = strContainerID;
        taskInfo.strSource = strSource;
        taskInfo.strDestination = strDestination;
        taskInfo.registerTime = DateTime.Now;

        taskInfo.getJobType();
        taskInfo.block = 21;    // block 8G

        // adding task
        listTaskInfo = listTaskInfo.Prepend(taskInfo).ToList();

        // initialization
        Reset();
    }

    // update current stack profile UI
    void UpdateStackProfileUI()
    {
        //// Deactive
        // init data
        for (int iRow = 0; iRow < GM.lengthRow; iRow++)
        {
            for (int iTier = 0; iTier < containerTr.GetLength(1); iTier++)
                deactiveContainer(containerTr[iRow, iTier]);
        }
        // WS, LS 별도 적용
        deactiveContainer(containerTr[dictRow["WS"], 0]);
        deactiveContainer(containerTr[dictRow["LS"], 0]);

        //// Enabled

        // 현재 bay
        int iBayNow = DropdownInputBay.value;

        // 해당 bay로 containerID 배치 및 enabled
        for (int i = 0; i < GM.list_stack_profile.Count; i++)
        {
            // [i_row, i_bay, i_tier, containerStatus]
            int[] containerData = GM.list_stack_profile[i];
            int iRow = containerData[0];
            int iBay = containerData[1];
            int iTier = containerData[2];

            // bay 같을 때 enabled
            if (iBayNow == iBay)
            {
                string ctID = GM.ByteArrayToString(GM.listContainerID[i]);
                enabledContainer(containerTr[iRow, iTier], ctID);
            }
        }

        //// Active

        // source, destination button
        if (btnSource && (iBaySource == iBayNow)) btnSource.GetComponent<Image>().color = colorActive;
        if (btnDestination && (iBayDestination == iBayNow)) btnDestination.GetComponent<Image>().color = colorActive;
    }

    void enabledContainer(Transform cntr, string ctID)
    {
        // containerID 배치
        string containerIDFormat = $"{ctID.Substring(0, 4)}\n{ctID.Substring(4, 4)}\n{ctID.Substring(8, 3)}";
        cntr.GetChild(0).GetComponent<TMP_Text>().text = containerIDFormat;

        // 색 변경
        cntr.GetComponent<Image>().color = colorDeactive;
    }

    void deactiveContainer(Transform cntr)
    {
        cntr.GetComponent<Image>().color = colorNull;
        cntr.GetChild(0).GetComponent<TMP_Text>().text = "";
    }

    // container 위치 string 생성
    string mkStrContainerLoc(string strIRow, int iBay, int iTier)
    {
        // convert row
        int iRow = dictRow[strIRow];

        // WS, LS 구분
        string output = (iRow < GM.lengthRow) ? $"{strIRow}{iBay}-{iTier + 1}" : $"{strIRow}{iBay}";

        return output;
    }
}

public enum StateTask { Done, Working, Standby }
public class TaskInfo
{
    public int jobID;
    public int jobType;
    public int block;   // Block : 8G = 21

    public string strCrane;
    public string strContainerID;
    public string strSource;
    public string strDestination;

    public StateTask stateTask = StateTask.Standby;
    public DateTime registerTime;
    public DateTime startTime;
    public DateTime endTime;
    public TimeSpan duration;

    public string getContent()
    {
        // make text structure
        string content = $" {GM.TimeToString(registerTime)}\n";
        content += $" {strCrane}, Job ID: {jobID}\n #{strContainerID}\n";
        content += $" {strSource} -> {strDestination}";

        return content;
    }

    public void getJobType()
    {
        // 1: Shuffling(Yard -> Yard)
        // 2: GateIn(LS or WS -> Yard), 3: GateOut(Yard -> LS ow WS),
        // 4: DischargeIn, 5: LoadOut, 6: Move, 7: Park , 8: InternalMove
        if (strSource.Contains("S")) jobType = 2;
        else if (strDestination.Contains("S")) jobType = 3;
        else jobType = 1;

        Debug.Log($"Job Type : {jobType}");
    }
}