using System;
using Unity.Netcode;
using UnityEngine;

public class Life : NetworkBehaviour
{
    // DEBUG
    float debugTimer;

    // initial health points
    public int InitialHp;
    // thrown when the character dies
    public Action DiedEvent;

    Controller m_Controller;

    NetworkVariable<int> m_Hp = new (0);

    public NetworkVariable<int> Hp { get { return m_Hp; } }  
    public bool IsAlive { get { return m_Hp.Value > 0; } }

    /// <summary>
    /// 
    /// </summary>
    public override void OnNetworkSpawn()
    {
        m_Hp.Value = InitialHp;
        m_Controller = GetComponent<Controller>();
    }

    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        //DisplayLife(2f);
    }

    #region Public Manipulators

    /// <summary>
    /// Apply damage to the character
    /// </summary>
    /// <param name="damage"> amount of damages </param>
    public void Hit(int damage)
    {
        if (!IsServer)
            return;

        if (damage < 0)
        {
            Debug.LogError($"Damages ({damage}) < 0");
            return;
        }

        Debug.LogWarning($"Client ({OwnerClientId})");
        Debug.LogWarning($"     + Damages ({damage})");

        m_Hp.Value -= damage;
    }

    /// <summary>
    /// Apply healing to the character
    /// </summary>
    /// <param name="heal"></param>
    public void Heal(int heal)
    {
        if (!IsServer)
            return;

        if (heal < 0)
        {
            Debug.LogError($"Healing ({heal}) < 0");
            return;
        }

        if (m_Hp.Value + heal > InitialHp)
            m_Hp.Value = InitialHp;
        else
            m_Hp.Value += heal;
    }

    #endregion


    #region Debug

    void DisplayLife(float timer = 2f)
    {
        if (debugTimer > 0f)
        {
            debugTimer -= Time.deltaTime;
            return;
        }

        print("Client: " + OwnerClientId);
        print("     + Life: " + m_Hp.Value);

        debugTimer = timer;
    }

    #endregion
}
