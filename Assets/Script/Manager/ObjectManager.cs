using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 게임 내 모든 동적 오브젝트(플레이어, 몬스터 등)의 생성, 파괴, 관리를 총괄합니다.
/// 타입 기반 제네릭 컬렉션을 사용하여 확장성이 매우 뛰어납니다.
/// </summary>
public class ObjectManager
{
    // 모든 종류의 오브젝트를 타입별로 관리하는 하나의 딕셔너리
    private Dictionary<Type, HashSet<BaseController>> _allObjects = new Dictionary<Type, HashSet<BaseController>>();


    /// <summary>
    /// 특정 타입의 프리팹을 지정된 위치에 생성합니다.
    /// </summary>
    /// <typeparam name="T">생성할 오브젝트의 컨트롤러 타입 (BaseController 상속)</typeparam>
    /// <param name="position">생성 위치</param>
    /// <param name="prefabName">Resources 폴더 내의 프리팹 이름. 비어있으면 타입 이름으로 대체</param>
    /// <returns>생성된 오브젝트의 컨트롤러 컴포넌트</returns>
    public T Spawn<T>(Vector3 position, string PrefabName, Transform parent = null, bool polling = false) where T : BaseController
    {
        
        // DataManager���� �˸°� �����;���
        GameObject go = Managers.Resource.Instantiate(PrefabName, parent, polling);
        if (go == null)
        {
            Debug.LogError($"[ObjectManager] Failed to instantiate prefab: {PrefabName}");
            return null;
        }

        go.transform.position = position;
        T controller = go.GetOrAddComponent<T>();
        return controller;
    }

    /// <summary>
    /// 특정 게임 오브젝트를 파괴합니다.
    /// 실제 목록 제거는 BaseController의 OnDestroy에서 자동으로 처리됩니다.
    /// </summary>
    public void Despawn<T>(T obj) where T : BaseController
    {
        if (obj == null) return;
        Unregister(obj);
        Managers.Resource.Destroy(obj.gameObject);
    }

    /// <summary>
    /// BaseController로부터 호출되어 해당 오브젝트를 타입에 맞는 그룹에 등록합니다.
    /// </summary>
    public void Register<T>(T obj) where T : BaseController
    {
        Type type = obj.GetType();

       

        if (!_allObjects.ContainsKey(type))
        {
            _allObjects.Add(type, new HashSet<BaseController>());
        }
        _allObjects[type].Add(obj);
    }

    /// <summary>
    /// BaseController로부터 호출되어 해당 오브젝트를 그룹에서 등록 해제합니다.
    /// </summary>
    public void Unregister<T>(T obj) where T : BaseController
    {
        Type type = obj.GetType();
     

        if (_allObjects.ContainsKey(type))
        {
            _allObjects[type].Remove(obj);
        }
    }

    /// <summary>
    /// 특정 타입의 모든 오브젝트 그룹(HashSet)을 안전하게 복사하여 반환합니다.
    /// </summary>
    public HashSet<T> GetGroup<T>() where T : BaseController
    {
        Type type = typeof(T);
        if (_allObjects.ContainsKey(type))
        {
            return new HashSet<T>(_allObjects[type].Cast<T>());
        }
        return new HashSet<T>(); // 없는 타입의 그룹일 경우 빈 HashSet 반환
    }

    public ContainerController SpawnRandomContainer(Vector3 position)
    {
        int randomIndex = UnityEngine.Random.Range(0, Define.containerPrefabs.Length);
        string selectedPrefab = Define.containerPrefabs[randomIndex];
        return Spawn<ContainerController>(position, selectedPrefab);
    }

    /// <summary>
    /// 씬 전환 또는 게임 종료 시 모든 관리 오브젝트를 파괴하고 목록을 초기화합니다.
    /// </summary>
    public void Clear()
    {
        var all = new List<BaseController>();
        foreach (var pair in _allObjects)
        {
            all.AddRange(pair.Value);
        }

        foreach (var obj in all)
        {
            //Managers.Resource.Destroy(obj.gameObject);
        }

        _allObjects.Clear();
       
    }
    

    

  
}