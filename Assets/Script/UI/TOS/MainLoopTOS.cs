using System;
using System.Collections;
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

    string strCrane, strContainerID, strSource, strDestination;
    const string defaultStrContainerID = "??";
    const string defaultStrSource = "??";
    const string defaultStrDestination = "??";

    void Start()
    {
        // init
        Initialization();

        btnApply.interactable = false;

    }

    void Initialization()
    {
        // init values
        InitDropdownData();
        InitTextData();


        // Add Listeners
        AddListeners();
    }

    void InitTextData()
    {
        // default values
        // strCrane 의 기본값은 인덱스 0번째 크레인.
        strContainerID = defaultStrContainerID;
        strSource = defaultStrSource;
        strDestination = defaultStrDestination;

        // apply
        SetTextApply();
    }

    void SetTextApply()
    {
        // make text structure
        textApply.text = $" {strCrane}\n {strContainerID}\n";
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
    }

    // 드롭다운 값이 변경될 때 호출되는 함수
    void OnDdInputCraneValueChanged(int index)
    {

        // select crane
        strCrane = DropdownInputCrane.options[index].text;
        SetTextApply();

        // // index는 선택된 옵션의 순서(0부터 시작)입니다
        // Debug.Log("선택된 인덱스: " + index);
        // Debug.Log("선택된 텍스트: " + DropdownInputCrane.options[index].text);

        // 인덱스 값에 따라 다른 동작을 수행합니다
        switch (index)
        {
            case 0:
                Debug.Log("옵션 1이 선택되었습니다.");
                break;
            case 1:
                Debug.Log("옵션 2가 선택되었습니다.");
                break;
            case 2:
                Debug.Log("옵션 3이 선택되었습니다.");
                break;
        }
    }

    void OnDdInputBayValueChanged(int index)
    {
        // index는 선택된 옵션의 순서(0부터 시작)입니다
        Debug.Log("선택된 인덱스: " + index);
        Debug.Log("선택된 텍스트: " + DropdownInputBay.options[index].text);

        // 인덱스 값에 따라 다른 동작을 수행합니다
        switch (index)
        {
            case 0:
                Debug.Log("옵션 1이 선택되었습니다.");
                break;
            case 1:
                Debug.Log("옵션 2가 선택되었습니다.");
                break;
            case 2:
                Debug.Log("옵션 3이 선택되었습니다.");
                break;
        }
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


}