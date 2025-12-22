using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_TOS_RTGC : UI_Base
{
    #region Enums


    public enum Buttons
    {
        Btn_Menu,
        ClickBlockPanel,
        Btn_Back,
        Btn_Home,
        Btn_TruckControl,
        Btn_CameraControl,
        Btn_SelectCraneUp,
        Btn_SelectCraneDown,
        Btn_SelectBayUp,
        Btn_SelectBayDown,
        Btn_Apply,
        Btn_Reset

    }

    public enum GameObjects
    {
        Sidebar,
        SidebarPanel,
        DropdownInputCrane,
        DropdownInputBay

    }

    public enum Texts
    {
        Txt_Apply,
    }

    #endregion

    void Start()
    {
        #region Bindings
        BindButton((Type)typeof(Buttons));
        BindObject((Type)typeof(GameObjects));
        BindText((Type)typeof(Texts));

        #endregion
    }
}
