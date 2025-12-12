using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Toggle_Truck : UI_Base
{
    #region Enums
    public enum Texts
    {
        Txt_Job,
        Txt_CraneName,
        Txt_TruckName,
        Txt_TruckStatus,
        
    }
    #endregion

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindText(typeof(Texts));
        this.gameObject.GetComponent<UnityEngine.UI.Toggle>().onValueChanged.AddListener(OnClick);


        return true;
    }

    public void SetInfo(string job, string craneName, string truckName, string truckStatus)
    {
        GetText((int)Texts.Txt_Job).text = job;
        GetText((int)Texts.Txt_CraneName).text = craneName;
        GetText((int)Texts.Txt_TruckName).text = truckName;
        GetText((int)Texts.Txt_TruckStatus).text = truckStatus;
    }

    void OnClick(bool isOn)
    {
        GM.SelectedTruck = Managers.Object.GetGroup<TruckController>().FirstOrDefault(tc => tc.truckName == GetText((int)Texts.Txt_TruckName).text);
        GM.SelectedTruck.isSelected = isOn;

        if (!isOn)
        {
            GM.SelectedTruck = null;
        }
    }
}
