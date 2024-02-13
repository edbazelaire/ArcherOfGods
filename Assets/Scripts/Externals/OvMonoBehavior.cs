using System;
using Tools;
using UnityEngine;

public class OvMonoBehavior : MonoBehaviour
{
    protected virtual void Start()
    {
        if (Debugger.Instance == null || Debugger.Instance.IsRegistered(this))
            return;

        // register commands of the current class
        Debugger.Instance.RegisterClass(this);
    }

    protected virtual void OnDestroy()
    {
        if (Debugger.Instance == null || ! Debugger.Instance.IsRegistered(this))
            return;

        Debugger.Instance.UnregisterClass(this);
    }

    protected virtual void OnEnable()
    {
        
    }

    protected virtual void OnDisable()
    {
       
    }
}
