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

    void Update()
    {
        if (Managers.UI.GetTopPopup() != this)
            return;

        // 2. ���콺 Ŭ�� Ȯ�� ����� ���ο� Input System ������� ����
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        PointerEventData eventData = new PointerEventData(_eventSystem);
        eventData.position = Mouse.current.position.ReadValue(); // ���콺 ��ġ�� ���ο� ��� ���

        List<RaycastResult> results = new List<RaycastResult>();
        _raycaster.Raycast(eventData, results);

        bool isClickOnPopup = false;
        foreach (var result in results)
        {
            if (result.gameObject.transform.IsChildOf(this.transform))
            {
                isClickOnPopup = true;
                break;
            }
        }

        if (!isClickOnPopup)
        {
            ClosePopup();
        }
    }

    public void ClosePopup()
    {
        Managers.UI.ClosePopupUI(this);
    }
}