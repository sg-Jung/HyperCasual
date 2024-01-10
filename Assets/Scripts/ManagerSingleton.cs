using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerSingleton : MonoBehaviour
{
    public static ManagerSingleton instance;

    private void Awake() 
    {
        if(instance == null) instance = this;
    }

    private static T GetManagerComponent<T>() where T : Component
    {
        if (instance == null)
            return null;

        return instance.GetComponentInChildren<T>();
    }

    public static T GetObjectPool<T>(string name) where T : Component
    {
        if (instance == null)
            return null;

        ObjectPool[] objectPools = instance.GetComponentsInChildren<ObjectPool>();
        foreach (ObjectPool objectPool in objectPools)
        {
            if (objectPool.name == name)
            {
                T component = objectPool.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }
            }
        }
        return null;
    }

    public static CustomerManager GetCustomerManager() => GetManagerComponent<CustomerManager>();
}
