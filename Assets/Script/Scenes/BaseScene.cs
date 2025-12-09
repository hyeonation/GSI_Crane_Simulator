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
        // EventSystem이 씬에 존재하지 않으면 생성
        EventSystem eventSystem = GameObject.FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject go = new GameObject { name = "@EventSystem" };
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }

        
    }

    public abstract void Clear();
}
