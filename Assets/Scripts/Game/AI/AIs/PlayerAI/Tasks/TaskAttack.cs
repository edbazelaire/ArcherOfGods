using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI;
using Data;
using Enums;
using Game.Character;
using Game.Loaders;
using Tools;
using UnityEngine;

public class TaskAttack : Node
{
    #region Members

    const float ATTACK_COOLDOWN = 0.2f;

    private Controller m_Controller;
    private SpellHandler m_SpellHandler => m_Controller.SpellHandler;

    private bool m_CanAttack = true;

    #endregion


    #region Init & End

    public TaskAttack(Controller controller)
    {
        m_Controller = controller;
    }

    #endregion


    public override NodeState Evaluate()
    {
        if (! m_CanAttack)
        {
            m_State = NodeState.FAILURE;
            return m_State;
        }

        ErrorHandler.Log("===================================================================", ELogTag.AITaskAttack);
        ErrorHandler.Log("TaskAttack.Evaluate()", ELogTag.AITaskAttack);
        if (m_Controller.SpellHandler.IsCasting.Value)
        {
            ErrorHandler.Log("     + IsCasting     : true",     ELogTag.AITaskAttack);
            ErrorHandler.Log("     + State         : RUNNING",  ELogTag.AITaskAttack);

            m_State = NodeState.RUNNING;
            return m_State;
        }

        // check that no state is blocking the cast
        if (m_Controller.SpellHandler.HasStateBlockingCast())
        {
            ErrorHandler.Log("     + HasStateBlockingCast  : true",     ELogTag.AITaskAttack);
            ErrorHandler.Log("     + State                 : FAILURE",  ELogTag.AITaskAttack);

            m_State = NodeState.FAILURE;
            return m_State;
        }

        // check if any spell can be casted
        ESpell spell = CheckSpellToSelect();
        if (spell != ESpell.Count)
        {
            m_Controller.SpellHandler.TryStartCastSpell(spell);
                
            ErrorHandler.Log("     + Casting Spell     : " + spell, ELogTag.AITaskAttack);
            ErrorHandler.Log("     + State             : SUCCESS",  ELogTag.AITaskAttack);

            ErrorHandler.Log("TaskAttack() : Casting Spell - " + spell, ELogTag.AIFinalDecision);

            m_State = NodeState.SUCCESS;
            m_Controller.StartCoroutine(TimerAttackCoroutine());
            return m_State;
        }

        // action failed, no spell can be used
        ErrorHandler.Log("     + no spell avaliable", ELogTag.AI);
        ErrorHandler.Log("     + State                 : FAILURE", ELogTag.AITaskAttack);

        m_State = NodeState.FAILURE;
        return m_State;
    }


    #region Spell Selection

    /// <summary>
    /// Check all spells to decide which is more adequate to the situation
    /// </summary>
    /// <returns></returns>
    ESpell CheckSpellToSelect()
    {
        // init spell
        ESpell spell = ESpell.Count;

        // check : ULTIMATE
        CheckUltimate(ref spell);

        // check : HEAL
        CheckHealingSpells(ref spell);

        // check : IronSkin
        CheckBuffs(ref spell);
      
        // check : IronSkin
        CheckDamageSpells(ref spell);

        // check : AutoAttack
        CheckAutoAttack(ref spell);

        return spell;
    }

    /// <summary>
    /// Check if should use an Ultimate
    /// </summary>
    /// <param name="spell"></param>
    void CheckUltimate(ref ESpell spell)
    {
        // skip if a spell was already selected
        if (spell != ESpell.Count)
            return;

        ErrorHandler.Log("CheckUltimate()", ELogTag.AITaskAttack);

        // check : ULTIMATE
        if (m_SpellHandler.CanCast(m_SpellHandler.Ultimate))
            spell = m_SpellHandler.Ultimate;
    }

    /// <summary>
    /// Check if should use a Heal spell
    /// </summary>
    /// <param name="spell"></param>
    void CheckHealingSpells(ref ESpell spell)
    {
        // skip if a spell was already selected
        if (spell != ESpell.Count)
            return;

        ErrorHandler.Log("CheckHealingSpells()", ELogTag.AITaskAttack);

        if (m_Controller.Life.Hp.Value == m_Controller.Life.MaxHp.Value)
            return;

        var spellsList = FilterSpellsByProperty(ESpellProperty.Heal);
        foreach (ESpell tempSpell in spellsList)
        {
            if (!m_SpellHandler.CanCast(tempSpell))
                continue;

            spell = tempSpell;
            return;
        }
    }

    /// <summary>
    /// Check if should use a Buff spell
    /// </summary>
    /// <param name="spell"></param>
    void CheckBuffs(ref ESpell spell)
    {
        // skip if a spell was already selected
        if (spell != ESpell.Count)
            return;

        ErrorHandler.Log("CheckBuffs()", ELogTag.AITaskAttack);

        var spellsList = FilterSpellsByType(ESpellType.Buff);
        foreach (ESpell tempSpell in spellsList)
        {
            if (!m_SpellHandler.CanCast(tempSpell))
                continue;

            spell = tempSpell;
            return;
        }
    }

    /// <summary>
    /// Check if should use an Aoe spell
    /// </summary>
    /// <param name="spell"></param>
    void CheckDamageSpells(ref ESpell spell)
    {
        // skip if a spell was already selected
        if (spell != ESpell.Count)
            return;

        ErrorHandler.Log("CheckDamageSpells()", ELogTag.AITaskAttack);

        var spellsList = FilterSpellsByProperty(ESpellProperty.Damages);

        foreach (ESpell tempSpell in spellsList)
        {
            // skip auto & utlimate (handled elsewhere)
            if (tempSpell == m_SpellHandler.AutoAttack || tempSpell == m_SpellHandler.Ultimate)
                continue;

            if (! m_SpellHandler.CanCast(tempSpell))
                continue;

            spell = tempSpell;
            return;
        }
    }
    

    /// <summary>
    /// Check if should use AutoAttack
    /// </summary>
    /// <param name="spell"></param>
    void CheckAutoAttack(ref ESpell spell)
    {
        // skip if a spell was already selected
        if (spell != ESpell.Count)
            return;

        ErrorHandler.Log("CheckAutoAttack()", ELogTag.AI);

        if (m_SpellHandler.CanCast(m_SpellHandler.AutoAttack))
            spell = m_SpellHandler.AutoAttack;
    }

    #endregion



    #region Helpers

    /// <summary>
    /// List spells in descending order on a specific property
    /// </summary>
    /// <param name="property"></param>
    /// <returns></returns>
    List<ESpell> FilterSpellsByProperty(ESpellProperty property)
    {
        List<(ESpell Spell, float Value)> spells = new();

        for (int i = 0; i < m_SpellHandler.Spells.Count; i++)
        {
            ESpell spell = m_SpellHandler.Spells[i];
            SpellData spellData = SpellLoader.GetSpellData(spell, m_SpellHandler.SpellLevels[i]);

            // TODO : BETTER
            var spellInfos = spellData.GetInfos();
            // try get value
            if (! spellInfos.ContainsKey(property.ToString()) || ! float.TryParse(spellInfos[property.ToString()].ToString(), out float value))
                continue;
            // TODO : BETTER

            if (value > 0)
            {
                int j = 0;
                for (j = 0; j < spells.Count; j++)
                {
                    if (spells[j].Value < value)
                        break;
                }

                spells.Insert(j, (spell, value));
            }
        }

        return spells.Select(t => t.Spell).ToList();
    }

    /// <summary>
    /// List spells in descending order on a specific property
    /// </summary>
    /// <param name="property"></param>
    /// <returns></returns>
    List<ESpell> FilterSpellsByType(ESpellType spellType)
    {
        List<ESpell> spells = new();

        for (int i = 0; i < m_SpellHandler.Spells.Count; i++)
        {
            ESpell spell = m_SpellHandler.Spells[i];
            SpellData spellData = SpellLoader.GetSpellData(spell);

            if (spellData.SpellType == spellType)
            {
                spells.Add(spell);
            }
        }

        return spells;
    }

    /// <summary>
    /// Set a fake cooldown to slow attacks
    /// </summary>
    /// <returns></returns>
    IEnumerator TimerAttackCoroutine()
    {
        m_CanAttack = false;

        float timer = ATTACK_COOLDOWN;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        m_CanAttack = true;

    }

    #endregion
}
