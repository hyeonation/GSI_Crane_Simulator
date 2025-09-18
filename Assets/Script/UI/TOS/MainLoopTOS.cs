using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Dropdown을 사용하기 위한 네임스페이스

public class MainLoopTOS : MonoBehaviour
{
    public Dropdown dropdownCrane; // 인스펙터에서 드롭다운을 할당합니다

    void Start()
    {
        // 드롭다운 옵션 목록을 생성합니다
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();

        for (int i = 0; i < GM.nameCranes.Length; i++)
        {
            options.Add(new Dropdown.OptionData(GM.nameCranes[i]));
        }

        // 생성된 옵션을 드롭다운에 설정합니다
        dropdownCrane.options = options;

        // 선택된 값이 변경될 때 호출될 이벤트를 등록합니다
        dropdownCrane.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    // 드롭다운 값이 변경될 때 호출되는 함수
    void OnDropdownValueChanged(int index)
    {
        // index는 선택된 옵션의 순서(0부터 시작)입니다
        Debug.Log("선택된 인덱스: " + index);
        Debug.Log("선택된 텍스트: " + dropdownCrane.options[index].text);

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
}