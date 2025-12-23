using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UI_TruckControlPopup : UI_Popup
{

    #region Enums
    public enum GameObjects
    {
        TruckContent
    }

    public enum Texts
    {
        Txt_SelectedTruck
    }

    public enum Buttons
    {
        Btn_Close,
        Btn_TruckDisconnect
    }
    #endregion

    // [핵심] 트럭 데이터와 UI 아이템을 매핑하여 관리하는 Dictionary
    private Dictionary<TruckController, Toggle_Truck> _activeTruckItems = new Dictionary<TruckController, Toggle_Truck>();

    GameObject truckContent;

    ToggleGroup toggleGroup;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        truckContent = GetObject((int)GameObjects.TruckContent);
        toggleGroup = truckContent.GetComponent<ToggleGroup>();
        GetButton((int)Buttons.Btn_Close).onClick.AddListener(OnClickClose);
        GetButton((int)Buttons.Btn_TruckDisconnect).onClick.AddListener(OnClickTruckDisconnect);


        GM.OnUpdateTruckList -= Refresh;
        GM.OnUpdateTruckList += Refresh;
        SetTruckContent();

        GM.OnSelectTruck -= UpdateSelectedTruckInfo;
        GM.OnSelectTruck += UpdateSelectedTruckInfo;
        UpdateSelectedTruckInfo();
        return true;
    }

    void Refresh()
    {
        SetTruckContent();
    }


    void SetTruckContent()
    {
        // 1. 현재 유효한 트럭 데이터 가져오기 (Source of Truth)
        HashSet<TruckController> currentTrucks = Managers.Object.GetGroup<TruckController>()
            .Where(truck => truck.gameObject.activeSelf)
            .ToHashSet();

        // 2. [제거 로직] 더 이상 유효하지 않은(사라진) 트럭의 UI 제거
        // Dictionary를 순회하면서 삭제할 키를 별도 리스트에 담아야 에러(InvalidOperationException)가 발생하지 않음
        List<TruckController> trucksToRemove = new List<TruckController>();

        foreach (var kvp in _activeTruckItems)
        {
            // 현재 데이터 리스트에 키(트럭)가 존재하지 않는다면 제거 대상
            if (!currentTrucks.Contains(kvp.Key))
            {
                trucksToRemove.Add(kvp.Key);
            }
        }

        foreach (var truck in trucksToRemove)
        {
            // UI 오브젝트 파괴 및 딕셔너리에서 제거
            if (_activeTruckItems[truck] != null)
            {
                Managers.Resource.Destroy(_activeTruckItems[truck].gameObject); // Managers.Resource.Destroy 사용 권장 (혹은 GameObject.Destroy)
            }
            _activeTruckItems.Remove(truck);
        }

        // 3. [생성 및 갱신 로직] 현재 유효한 트럭 리스트 순회
        foreach (var truck in currentTrucks)
        {
            Toggle_Truck btnTruck;

            // 이미 UI가 생성되어 있는 경우 -> 가져와서 갱신만 수행
            if (_activeTruckItems.TryGetValue(truck, out btnTruck))
            {
                // UI가 null인 경우(예외적 파괴) 방어 코드
                if (btnTruck == null)
                {
                    btnTruck = CreateNewTruckItem(truck);
                }
            }
            else
            {
                // UI가 없는 경우 -> 새로 생성 후 Dictionary에 등록
                btnTruck = CreateNewTruckItem(truck);
            }

            // 4. [공통] 정보 갱신 (View Update)
            UpdateTruckItemUI(btnTruck, truck);
        }
    }


    private Toggle_Truck CreateNewTruckItem(TruckController truck)
    {
        Toggle_Truck btnTruck = Managers.UI.MakeSubItem<Toggle_Truck>(truckContent.transform);
        btnTruck.GetComponent<UnityEngine.UI.Toggle>().group = toggleGroup;

        // 딕셔너리에 등록 (캐싱)
        _activeTruckItems.Add(truck, btnTruck);

        return btnTruck;
    }


    private void UpdateTruckItemUI(Toggle_Truck btnTruck, TruckController truck)
    {
        string truckStatus = truck.IsArrived ? "Arrived" : "In Transit";

        // 기존 SetInfo 호출
        btnTruck.SetInfo(truck.Job.ToString(), truck.CraneName, truck.name, truckStatus);

        // Interactable 상태 갱신
        var toggle = btnTruck.GetComponent<UnityEngine.UI.Toggle>();
        if (toggle != null)
        {
            toggle.interactable = truck.IsArrived;
        }
    }

    void UpdateSelectedTruckInfo()
    {
        if (GM.SelectedTruck != null)
        {
            GetText((int)Texts.Txt_SelectedTruck).text = GM.SelectedTruck.truckName;
        }
        else
        {
            GetText((int)Texts.Txt_SelectedTruck).text = "None";
        }
    }

    void OnClickClose()
    {
        Managers.UI.ClosePopupUI(this);
    }

    void OnClickTruckDisconnect()
    {
        toggleGroup.SetAllTogglesOff();
    }



    void OnDestroy()
    {
        GM.OnUpdateTruckList -= Refresh;
        GM.OnSelectTruck -= UpdateSelectedTruckInfo;
    }
}
