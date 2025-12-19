using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public abstract class BaseScene : MonoBehaviour
{
    public  Define.SceneType SceneType { get; } = Define.SceneType.Unknown;
    private void Awake()
    {
        Init();
    }

    protected virtual void Init()
    {
       
       
    }

    public abstract void Clear();
}
