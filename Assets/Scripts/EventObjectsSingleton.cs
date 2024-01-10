using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventObjectsSingleton : MonoBehaviour
{
    public static EventObjectsSingleton instance;

    private void Awake() 
    {
        if(instance == null) instance = this;
    }

    private static T GetEventObjectComponent<T>() where T : Component
    {
        if (instance == null)
            return null;

        return instance.GetComponentInChildren<T>();
    }

    public static BreadBoxController GetBreadBoxController() => GetEventObjectComponent<BreadBoxController>();
    public static OvenController GetOvenController() => GetEventObjectComponent<OvenController>();
    public static CounterController GetCounterController() => GetEventObjectComponent<CounterController>();
}
