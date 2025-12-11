using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_TruckControlPopup : UI_Popup
{
    
    #region Enums
    public enum GameObjects
    {
        TruckContent
    }
    #endregion

    GameObject truckContent;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        truckContent = GetObject((int)GameObjects.TruckContent);


            
        GM.UI_OnUpdateTruckList += Refresh;
        SetTruckContent();
        return true;
    }

    void Refresh()
    {
        SetTruckContent();
    }

    void SetTruckContent()
    {
        // truckContent init
        foreach (Transform child in truckContent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        // get truck list from Managers.Object. 활성화된 트럭만 가져오기
        HashSet<TruckController> trucks = Managers.Object.GetGroup<TruckController>().Where(truck => truck.gameObject.activeSelf).ToHashSet();

        // for each truck, create Btn_Truck prefab and set info
        foreach (var truck in trucks)
        {
            Btn_Truck btnTruck = Managers.UI.MakeSubItem<Btn_Truck>(truckContent.transform);
            string truckStatus = truck.IsArrived ? "Arrived" : "In Transit";
            btnTruck.SetInfo(truck.Job, truck.CraneName, truck.name, truckStatus);
        }
        
    }

    void OnDestroy()
    {
        GM.UI_OnUpdateTruckList -= Refresh;
    }
}
