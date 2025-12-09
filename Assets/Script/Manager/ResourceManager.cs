using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

public class ResourceManager 
{
    Dictionary<string, Object> _loadedResources = new Dictionary<string, Object>();

    public T Load<T>(string resourceKey) where T : Object
    {
        //�ε�    
        if (_loadedResources.TryGetValue(resourceKey, out Object resource))
            return resource as T;

        if (typeof(T) == typeof(Sprite))
        {
            if (_loadedResources.TryGetValue(resourceKey, out Object temp))
            {
                return temp as T;
            }

            resourceKey = resourceKey + ".sprite";
            if (_loadedResources.TryGetValue(resourceKey, out Object temp2))
            {
                return temp2 as T;
            }
            
        }

        Debug.Log($"[ResourceManager] Load Fail No Resource : {resourceKey}");
        return null;
    }

    public GameObject Instantiate(string resourceKey, Transform parent = null, bool polling = false)
    {
        GameObject prefab = Load<GameObject>(resourceKey);
        if (prefab == null)
            return null;

        if(polling)
            return Managers.Pool.Pop(prefab);

        GameObject go =  Object.Instantiate(prefab, parent);
        go.name = prefab.name;
        return go;
    }

    public void Destroy(GameObject go, float delay = 0f)
    {
        if (go == null)
            return;

        if(Managers.Pool.Push(go))
            return;

        Object.Destroy(go, delay);
    }

    public void LoadAsync<T>(string resourceKey, Action<T> callback = null) where T : Object
    {
        if (_loadedResources.TryGetValue(resourceKey, out Object resource))
        {
            callback?.Invoke(resource as T);
            return;
        }

        string loadKey = resourceKey;
        if (resourceKey.Contains(".sprite"))
            loadKey = $"{resourceKey}[{resourceKey.Replace(".sprite", "")}]";
           
        // ���ҽ� �񵿱� �ε�
        Addressables.LoadAssetAsync<T>(loadKey).Completed += handle =>
        {
            _loadedResources.Add(resourceKey, handle.Result);
            callback?.Invoke(handle.Result );
        };
    }

    public void LoadAllAsync<T>(string label, Action<string, int, int> callback) where T : Object
    { 
        Addressables.LoadResourceLocationsAsync(label,typeof(T)).Completed += handle =>
        {
            var locations = handle.Result;
            int totalCount = locations.Count;
            int loadedCount = 0;
            foreach (var location in locations)
            {
                if (location.PrimaryKey.Contains(".sprite"))
                {
                    LoadAsync<Sprite>(location.PrimaryKey, (obj) =>
                    {
                        loadedCount++;
                        callback?.Invoke(location.PrimaryKey, loadedCount, totalCount);
                    });
                }
                else {
                    LoadAsync<T>(location.PrimaryKey, resource =>
                    {
                        loadedCount++;
                        callback?.Invoke(location.PrimaryKey, loadedCount, totalCount);
                    });
                }
                    
            }
        };
    }
}
