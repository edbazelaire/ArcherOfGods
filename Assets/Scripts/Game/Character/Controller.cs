using System.Collections;
using System.Collections.Generic;
using Tools;
using UnityEngine;

public class Controller : MonoBehaviour
{
    Movement    m_Movement;
    Life        m_Life;
    HealthBar   m_HealthBar;

    #region Setup 

    /// <summary>
    /// Initialize the controller
    /// </summary>
    public void Initialize(HealthBar healthBar)
    {
        m_Life = GetComponent<Life>();
        m_Movement = GetComponent<Movement>();

        Checker.NotNull(m_Life);
        Checker.NotNull(m_Movement);    

        SetHealthBar(healthBar);
    }

    /// <summary>
    /// Set the health bar for this controller
    /// </summary>
    /// <param name="healthBar"></param>
    public void SetHealthBar(HealthBar healthBar)
    {
        m_HealthBar = healthBar;
        m_HealthBar.SetMaxHealth(m_Life.InitialHp);

        m_Life.OnHealthChanged += m_HealthBar.SetHealth;
    }

    #endregion


}
