using System;
using Tools;
using UnityEngine;

public class MObject : MonoBehaviour
{

    #region Init & End

    protected virtual void Start()
    {
        // register commands of the current class
        if (Debugger.Instance != null && ! Debugger.Instance.IsRegistered(this))
            Debugger.Instance.RegisterClass(this);
    }

    protected virtual void OnDestroy()
    {
        if (Debugger.Instance != null && !Debugger.Instance.IsRegistered(this))
            Debugger.Instance.UnregisterClass(this);

        UnRegisterListeners();
    }

    public virtual void Initialize()
    {
        FindComponents();
        RegisterListeners();
    }
    protected virtual void FindComponents() { }
    protected virtual void RegisterListeners() 
    {
        UnRegisterListeners();
    }

    protected virtual void UnRegisterListeners() { }

    #endregion




    protected virtual void OnEnable()
    {
        
    }

    protected virtual void OnDisable()
    {
       
    }
}
