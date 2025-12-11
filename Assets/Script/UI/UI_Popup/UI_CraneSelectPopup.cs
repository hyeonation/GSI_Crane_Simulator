using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.Pkcs;
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
        GM.CraneType = Define.CraneType.RMGC;
        GM.craneTypeStr = GM.CraneType.ToString();
        Managers.UI.ClosePopupUI(this);
    }

    private void OnClickRTGC()
    {
        GM.CraneType = Define.CraneType.RTGC;
        GM.craneTypeStr = GM.CraneType.ToString();
        Managers.UI.ClosePopupUI(this);
    }

    private void OnClickQC()
    {
        GM.CraneType = Define.CraneType.QC;
        GM.craneTypeStr = GM.CraneType.ToString();
        Managers.UI.ClosePopupUI(this);
    }

    private void OnClickBack()
    {
        Managers.UI.ClosePopupUI(this);
    }
}