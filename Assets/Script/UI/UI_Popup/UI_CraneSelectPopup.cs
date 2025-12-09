using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_CraneSelectPopup : UI_Popup
{
   
   public enum Buttons
    {
        Btn_RMGC,
        Btn_RTGC,
        Btn_QC,
        Btn_Back
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindButton(typeof(Buttons));

        GetButton((int)Buttons.Btn_RMGC).onClick.AddListener(OnClickRMGC);
        GetButton((int)Buttons.Btn_RTGC).onClick.AddListener(OnClickRTGC);
        GetButton((int)Buttons.Btn_QC).onClick.AddListener(OnClickQC);
        GetButton((int)Buttons.Btn_Back).onClick.AddListener(OnClickBack);

        return true;
    }

    private void OnClickRMGC()
    {
        GM.craneType = Define.CraneType.RMGC;
        GM.craneTypeStr = "RMGC";
        Debug.Log($"Crane type set to: {GM.craneTypeStr}, ");
        Managers.UI.ClosePopupUI(this);
    }

    private void OnClickRTGC()
    {
        GM.craneType = Define.CraneType.RTGC;
        GM.craneTypeStr = "RTGC";
        Debug.Log($"Crane type set to: {GM.craneTypeStr}, ");
        Managers.UI.ClosePopupUI(this);
    }

    private void OnClickQC()
    {
        GM.craneType = Define.CraneType.QC;
        GM.craneTypeStr = "QC";
        Debug.Log($"Crane type set to: {GM.craneTypeStr}, ");
        Managers.UI.ClosePopupUI(this);
    }

    private void OnClickBack()
    {
        Managers.UI.ClosePopupUI(this);
    }
}