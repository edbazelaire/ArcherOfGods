using System;
using UnityEngine;

public class Life : MonoBehaviour
{
    // initial health points
    public int InitialHp;
    // thrown when the character dies
    public Action DiedEvent;
    // thrown when the character's health changes
    public Action<int> HealthChangedEvent;

    Controller m_Controller;

    int m_Hp;
    public int Hp { get { return m_Hp; } }  
    public bool IsAlive { get { return m_Hp > 0; } }

    // Start is called before the first frame update
    void Start()
    {
        m_Hp = InitialHp;
        m_Controller = GetComponent<Controller>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            Hit(10);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            Heal(10);
    }

    #region Public Manipulators

    /// <summary>
    /// Apply damage to the character
    /// </summary>
    /// <param name="damage"> amount of damages </param>
    public void Hit(int damage)
    {
        if ( damage < 0 )
        {
            Debug.LogError($"Damages ({damage}) < 0");
            return;
        }

        m_Hp -= damage;

        HealthChangedEvent?.Invoke(m_Hp);

        if (m_Hp <= 0)
            m_Controller.Die();
    }

    /// <summary>
    /// Apply healing to the character
    /// </summary>
    /// <param name="heal"></param>
    public void Heal(int heal)
    {
        if (heal < 0)
        {
            Debug.LogError($"Healing ({heal}) < 0");
            return;
        }

        if (m_Hp + heal > InitialHp)
            m_Hp = InitialHp;
        else
            m_Hp += heal;

        HealthChangedEvent?.Invoke(m_Hp);
    }   

    #endregion
}
