﻿using System.Collections;
using UnityEngine;


public class MObjectTemplate : MObject
{
    #region Members

    #endregion


    #region Init & End

    protected override void FindComponents()
    {
        base.FindComponents();
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    protected override void SetUpUI()
    {
        base.SetUpUI();
    }

    #endregion


    #region GUI Manipulators

    #endregion


    #region Listeners

    protected override void RegisterListeners()
    {
        base.RegisterListeners();
    }

    protected override void UnRegisterListeners()
    {
        base.UnRegisterListeners();
    }

    #endregion
}
