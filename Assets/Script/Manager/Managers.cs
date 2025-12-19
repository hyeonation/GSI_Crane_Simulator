using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers s_instance; // ���ϼ��� ����ȴ�
    public static Managers Instance 
    { 
        get 
        { 
            // 게임 종료 중이라면 null을 반환하여 객체 생성을 원천 차단
            if (s_isQuitting) 
            {
                return null;    
            }
            
            Init(); 
            return s_instance; 
        } 
    }
    static bool s_isQuitting = false;

    #region Managers
    UIManager _ui = new UIManager();
    ObjectManager _object = new ObjectManager();
    ResourceManager _resource = new ResourceManager();
    PoolManager _pool = new PoolManager();
    PLCManager _plc = new PLCManager();
    

    SceneManagerEx _scene = new SceneManagerEx();
    public static UIManager UI { get { return Instance?._ui; } }
    public static ObjectManager Object { get { return Instance?._object; } }
    public static ResourceManager Resource { get { return Instance?._resource; } }
    public static PoolManager Pool { get { return Instance?._pool; } }
    public static SceneManagerEx Scene { get { return Instance?._scene; } }
    public static PLCManager PLC { get { return Instance?._plc; } }
    #endregion

    public static void Init()
    {
        // 종료 중이면 초기화 로직 수행 안 함
        if (s_isQuitting) return;

        if (s_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            s_instance = go.GetComponent<Managers>();

            // [심화] 혹시 모를 매니저 초기화 로직이 있다면 여기서 호출
            // s_instance._pool.Init(); 
        }
    }

    private void Start()
    {
        Managers.Resource.LoadAllAsync<Object>("PreLoad", (key, count, totalcount) =>
        {
            Debug.Log($"Loaded character prefab: {key} .. {count}/{totalcount}");
            
        });
    }
    
    private void OnApplicationQuit()
    {
        s_isQuitting = true;
    }
}
