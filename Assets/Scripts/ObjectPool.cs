using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPool : MonoBehaviour
{
    public readonly List<GameObject> usingObject = new List<GameObject>();
    public readonly Queue<GameObject> objectQueue = new Queue<GameObject>();
    public GameObject poolObject;
    public Transform poolObjectParent;
    public int initSize;
    public string objName;

    private void Start() 
    {
        ObjectPoolInit(initSize);
    }

    public void ObjectPoolInit(int initSize = 0)
    {
        for (int i = 0; i < initSize; i++)
        {
            GameObject obj = CreateObject();
            objectQueue.Enqueue(obj);
        }
    }

    public GameObject CreateObject()
    {
        GameObject obj = Instantiate(poolObject, Vector3.zero, Quaternion.identity);
        obj.name = objName;
        obj.transform.SetParent(poolObjectParent);
        obj.SetActive(false);

        return obj;
    }

    public GameObject GetObject()
    {
        GameObject obj = null;

        if(objectQueue.Count > 0)
        {
            obj = objectQueue.Dequeue();
        }
        else
        {
            obj = CreateObject();
        }
        obj.SetActive(true);
        usingObject.Add(obj);
        
        return obj;
    }

    public void ReturnObject(GameObject obj)
    {
        if(obj == null) return;

        if(usingObject.Contains(obj)) usingObject.Remove(obj);
        
        obj.transform.SetParent(poolObjectParent);
        obj.SetActive(false);
        objectQueue.Enqueue(obj);
    }

    public void ReturnAllObjectInUsingObject()
    {
        for(int i = 0; i < usingObject.Count; i++)
        {
            ReturnObject(usingObject[i]);
        }
    }
}