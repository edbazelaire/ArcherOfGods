using AI;
using Enums;
using Game.AI;
using Game.Spells;
using Tools;
using UnityEngine;

public class CheckInZone : Node
{
    #region Members

    // =============================================================================
    // Component & GameObjects
    private Controller m_Controller;
   
    #endregion


    #region Init & End
    
    public CheckInZone(Controller controller)
    {
        m_Controller = controller;
    }

    #endregion


    #region Evaluation

    public override NodeState Evaluate()
    {
        m_State = CheckIsInZone() ? NodeState.SUCCESS : NodeState.FAILURE;

        if (m_State == NodeState.SUCCESS)
            ErrorHandler.Log("CheckIsInZone() : SUCCESS", ELogTag.AICheckers);

        return m_State;
    }

    /// <summary>
    /// Check if character is standing on a Spell Preview
    /// </summary>
    /// <returns></returns>
    bool CheckIsInZone()
    {
        // Check for overlapped colliders with the capsule collider
        Collider2D[] colliders = CollisionChecker.GetControllerCollisions(m_Controller, ELayer.Spell);

        // Now you can iterate through overlappedColliders to handle each collider
        foreach (Collider2D collider in colliders)
        {
            Spell spell = collider.gameObject.GetComponent<Spell>();
            if (spell == null) 
            {
                ErrorHandler.Error("Found collider on layer Spell but unable to find a Spell component");
                continue;
            }

            // ignore allies spells
            if (spell.Controller.Team == m_Controller.Team)
                continue;

            // only check zones
            if (spell.SpellData.SpellType != ESpellType.Zone)
                continue;

            return true;
        }

        return false;
    }

    #endregion
}
