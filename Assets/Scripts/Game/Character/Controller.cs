using Data;
using Game.Character;
using Game.Managers;
using System.Collections;
using System.Collections.Generic;
using Tools;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Controller : MonoBehaviour
{
    public ECharacter       Character       { get; set; }
    public int              Team            { get; set; }
    public bool             IsCurrentPlayer { get; set; }
    public bool             IsPlayer        { get; set; }

    Movement                m_Movement;
    Life                    m_Life;
    HealthBar               m_HealthBar;
    SpellHandler            m_SpellHandler;

    public Movement         Movement        => m_Movement;
    public Life             Life            => m_Life;
    public SpellHandler     SpellHandler    => m_SpellHandler;


    #region Setup 

    /// <summary>
    /// Initialize the controller
    /// </summary>
    public void Initialize(ECharacter character, int team, bool isPlayer, bool isCurrentPlayer, HealthBar healthBar)
    {
        Character       = character;
        Team            = team;
        IsPlayer        = isPlayer;
        IsPlayer        = isCurrentPlayer;
        m_Life          = GetComponent<Life>();
        m_Movement      = GetComponent<Movement>();
        m_SpellHandler  = GetComponent<SpellHandler>();

        Checker.NotNull(m_Life);
        Checker.NotNull(m_Movement);    

        SetHealthBar(healthBar);

        if (IsCurrentPlayer) 
            SetupSpellUI();
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

    /// <summary>
    /// 
    /// </summary>
    public void SetupSpellUI()
    {
        foreach (SpellData spell in CharacterLoader.GetCharacterData(Character).SpellData)
        {
            GameUIManager.Instance.CreateSpellTemplate(spell.Name);
        }
    }   

    #endregion


    #region Public Manipulators

    /// <summary>
    /// Kill the character
    /// </summary>
    public void Die()
    {
        Destroy(gameObject);
    }

    #endregion

}
