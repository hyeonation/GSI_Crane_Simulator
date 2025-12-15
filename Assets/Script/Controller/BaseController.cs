using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BaseController : MonoBehaviour 
{

    bool _isInit = false;

    private void Awake()
    {
        Init();
    }

    public virtual bool Init()
    {
        if (_isInit)
            return false;

        Managers.Object.Register(this);
        _isInit = true;
        return true;
        
    }

    public virtual void SetInfo()
    {

    }


    protected virtual void OnEnable()
    {
        Managers.Object.Register(this);
    }

    
    // protected virtual void  OnDisable() {
    //     Managers.Object.Unregister(this);
    // }

    protected virtual void OnDestroy() {
        if (Managers.Instance != null) 
        {
            Managers.Object.Unregister(this);
        }
    }
    


}
