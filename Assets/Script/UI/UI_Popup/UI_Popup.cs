using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI; // 1. ���ӽ����̽� �߰�

public class UI_Popup : UI_Base
{
    private GraphicRaycaster _raycaster;
    private EventSystem _eventSystem;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Managers.UI.SetCanvas(gameObject, true);

        _raycaster = GetComponentInParent<Canvas>().gameObject.GetOrAddComponent<GraphicRaycaster>();
        _eventSystem = EventSystem.current;

        return true;
    }

    

    public void ClosePopup()
    {
        Managers.UI.ClosePopupUI(this);
    }
}