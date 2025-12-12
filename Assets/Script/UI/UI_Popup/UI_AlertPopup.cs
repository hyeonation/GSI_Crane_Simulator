using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UI_AlertPopup : UI_Popup
{
    #region enums
    public enum Buttons
    {
        Btn_Cancel,
        Btn_Ok,
    }

    public enum Texts
    {
        Txt_Title,
        Txt_Desc,
    }
    #endregion

    private Action<bool> _callbackAction;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindButton(typeof(Buttons));
        BindText(typeof(Texts));

        GetButton((int)Buttons.Btn_Cancel).onClick.AddListener(() => OnButtonClick(false));
        GetButton((int)Buttons.Btn_Ok).onClick.AddListener(() => OnButtonClick(true));

        return true;
    }

    public void SetAlert(string title, string desc, Action<bool> callbackAction = null)
    {
        GetText((int)Texts.Txt_Title).text = title;
        GetText((int)Texts.Txt_Desc).text = desc;

        _callbackAction = callbackAction;
       
    }

    private void OnButtonClick(bool result)
    {
        _callbackAction?.Invoke(result);
        _callbackAction = null; 
    }
}
