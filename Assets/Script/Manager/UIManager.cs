using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager
{
    int _order = 20;
    int _toastOrder = 500;
    Stack<UI_Popup> _popupStack = new Stack<UI_Popup>();
    Stack<UI_Toast> _toastStack = new Stack<UI_Toast>();
    UI_Scene _sceneUI = null;

    public UI_Scene SceneUI
    {
        get { return _sceneUI; }
    }
    public GameObject Root
    {
        get
        {
            GameObject root = GameObject.Find("@UI_Root");
            if (root == null)
            {
                root = new GameObject("@UI_Root");
            }
            return root;
        }
    }

    public void SetCanvas(GameObject go, bool sort = true, int sortOrder = 0, bool isToast = false)
    {
        Canvas canvas = Util.GetOrAddComponent<Canvas>(go);
        if(canvas == null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
        }

        CanvasScaler cs = go.GetOrAddComponent<CanvasScaler>();
        if (cs == null)
        {
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(Define.SCREEN_WIDTH, Define.SCREEN_HEIGHT);
        }
        go.GetOrAddComponent<GraphicRaycaster>();

        if (sort)
           { canvas.sortingOrder = _order;
            _order++;
        }
        else
            canvas.sortingOrder = sortOrder;

        if (isToast)
        {
            _toastOrder++;
            canvas.sortingOrder = _toastOrder;
        }
    }

    public T MakeWorldSpaceUI<T>(Transform parent = null, string name =null ) where T : UI_Base
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        
        GameObject go = Managers.Resource.Instantiate($"{name}");
        if (parent !=null)
            go.transform.SetParent(parent);
        Canvas canvas = go.GetOrAddComponent<Canvas>(); 
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        return Util.GetOrAddComponent<T>(go);
    }

    public T MakeSubItem<T>(Transform parent = null, string name = null, bool pooling = false, int order = 0) where T : UI_Base
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"{name}",parent, pooling);
        if (order != 0)
            {Canvas canvas = go.GetOrAddComponent<Canvas>();
            canvas.sortingOrder = order;
        }
        go.transform.SetParent(parent);
        return Util.GetOrAddComponent<T>(go);
    }

    public T ShowSceneUI<T>(string name = null) where T : UI_Scene
    {
       
        if ( string.IsNullOrEmpty(name))
            name = typeof(T).Name;
        
        GameObject go = Managers.Resource.Instantiate($"{name}", Root.transform);
        T sceneUI = Util.GetOrAddComponent<T>(go);
        _sceneUI = sceneUI;

        return sceneUI;

    }

    public T ShowPopupUI<T>(string name = null) where T : UI_Popup
    {
        
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"{name}");
        T popup = Util.GetOrAddComponent<T>(go);
        _popupStack.Push(popup);
        go.transform.SetParent(Root.transform);

        return popup;
    }



    public void ClosePopupUI(UI_Popup popup)
    {
        if (_popupStack.Count == 0)
            return;

        if (_popupStack.Peek() != popup)
        {
            Debug.Log("Close Popup Failed!");
            return;
        }
        ClosePopupUI();
    }

    public void ClosePopupUI()
    {
        if (_popupStack.Count == 0)
            return;
        _order--;
        UI_Popup popup = _popupStack.Pop();
        Managers.Resource.Destroy(popup.gameObject);
        popup = null;
    }

    public void CloseUI(UI_Base uI_Base)
    {
        Managers.Resource.Destroy(uI_Base.gameObject);
    }

    public UI_Popup GetTopPopup()
    {
        if (_popupStack.Count == 0)
            return null;
        return _popupStack.Peek();
    }


    public void CloseAllPopupUI()
    {
        while (_popupStack.Count > 0)
            ClosePopupUI();
    }

    public int GetPopupCount()
    {
        return _popupStack.Count;
    }

    public void Clear()
    {
        CloseAllPopupUI();
        Time.timeScale = 1;
        _sceneUI = null;
    }


    public UI_Toast ShowToast(string msg)
    {
        string name = typeof(UI_Toast).Name;
        GameObject go = Managers.Resource.Instantiate($"{name}");
        UI_Toast popup = Util.GetOrAddComponent<UI_Toast>(go);
        popup.SetInfo(msg);
        _toastStack.Push(popup);
        go.transform.SetParent(Root.transform);
        CoroutineManager.StartCoroutine(CoCloseToastUI());
        return popup;
    }

    IEnumerator CoCloseToastUI()
    {
       yield return new WaitForSeconds(1f);
       CloseToastUI();
    }

    public void CloseToastUI()
    {
        if (_toastStack.Count == 0)
            return;

        UI_Toast toast = _toastStack.Pop();
        Managers.Resource.Destroy(toast.gameObject);
        toast = null;
        _toastOrder--;
    }

   

}
