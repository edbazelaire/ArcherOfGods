using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Life : MonoBehaviour
{
    public int InitialHp;
    public Action<int> OnHealthChanged;

    int m_Hp;
    public int Hp { get { return m_Hp; } }  

    // Start is called before the first frame update
    void Start()
    {
        m_Hp = InitialHp;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            Hit(10);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            Heal(10);
    }

    #region Private Manipulators

    /// <summary>
    /// Kill the character
    /// </summary>
    void Die()
    {
        Destroy(gameObject);
    }   

    /// <summary>
    /// Apply damage to the character
    /// </summary>
    /// <param name="damage"> amount of damages </param>
    void Hit(int damage)
    {
        if ( damage < 0 )
        {
            Debug.LogError($"Damages ({damage}) < 0");
            return;
        }

        m_Hp -= damage;

        OnHealthChanged?.Invoke(m_Hp);

        if (m_Hp <= 0)
            Die();
    }   

    /// <summary>
    /// Apply healing to the character
    /// </summary>
    /// <param name="heal"></param>
    void Heal(int heal)
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

        OnHealthChanged?.Invoke(m_Hp);
    }   

    #endregion
}
