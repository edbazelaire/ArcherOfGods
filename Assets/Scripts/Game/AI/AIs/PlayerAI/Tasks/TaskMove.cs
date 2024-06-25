using AI;
using Enums;
using Game.AI;
using Game.Character;
using System.Collections.Generic;
using Tools;
using UnityEngine;

public class TaskMove : Node
{
    #region Members

    // =============================================================================
    // Component & GameObjects
    protected Controller m_Controller;
    protected ImmediatThreatTrigger m_ImmediatThreatTrigger;
    protected Movement m_Movement => m_Controller.Movement;

    // =============================================================================
    // Data
    // -- Serializable data (todo)
    protected float m_CheckObstaclesSize = 0.5f;
    protected float m_CheckZoneSize = 1f;

    // -- continue data
    protected List<int> m_AllowedMovements    = new List<int> { -1, 1 };
    protected int m_CurrentMoveX              = 1;
    protected Vector2 m_Position => m_Controller.transform.position;

    #endregion


    #region Init & End
    
    public TaskMove(Controller controller)
    {
        m_Controller = controller;
        m_ImmediatThreatTrigger = Finder.FindComponent<ImmediatThreatTrigger>(controller.gameObject);
    }

    #endregion


    #region Movement Evaluation

    public override NodeState Evaluate()
    {
        // select a movement direction
        SelectMovement();

        if (m_State == NodeState.FAILURE)
            return m_State;

        // apply movement ((-1) because of team effect (remove ?))
        m_Movement.SetMovement((-1) * m_CurrentMoveX);

        return m_State;
    }

    protected virtual void SelectMovement()
    {
        m_State = NodeState.FAILURE;

        if (!m_Movement.CanMove)
            return;

        // reset allowed movements
        m_AllowedMovements = new List<int> { -1, 1 };

        CheckObstacles();
        CheckZones();
        CheckProjectiles();

        if (m_AllowedMovements.Count == 0)
            m_CurrentMoveX = 0;

        else if (!m_AllowedMovements.Contains(m_CurrentMoveX))
            m_CurrentMoveX = m_AllowedMovements[0];

        m_State = NodeState.RUNNING;
    }

    #endregion


    #region Checkers

    /// <summary>
    /// Check if there is an obstacle on the ground that prevents movement on the left or the right
    /// </summary>
    /// <param name="m_AllowedMovements"></param>
    protected virtual void CheckObstacles()
    {
        if (m_AllowedMovements.Count == 0)
            return;

        // duplicate array to be able to remove while going threw
        var allowedMovement = m_AllowedMovements.ToArray();

        // for each remaining allowed movements, check if there is obstacles in that direction
        foreach (int moveX in allowedMovement)
        {
            Collider2D[] colliders = CollisionChecker.GetCollidersInDistance(m_Controller.transform.position.x, moveX * m_CheckObstaclesSize, CollisionChecker.OBSTACLES_LAYERS);
            if (colliders.Length > 0)
                m_AllowedMovements.Remove(moveX);
        }
    }

    /// <summary>
    /// Check if there is a zone spell on the ground that prevents movement on the left or the right
    /// </summary>
    /// <param name="m_AllowedMovements"></param>
    protected virtual void CheckZones()
    {
        if (m_AllowedMovements.Count == 0)
            return;

        // duplicate array to be able to remove while going threw
        var allowedMovement = m_AllowedMovements.ToArray();

        // for each remaining allowed movements, check if there is an Enemy ZoneSpell in that direction
        foreach (int moveX in allowedMovement)
        {
            // get all colliders on Layer "Spell"
            Collider2D[] colliders = CollisionChecker.GetCollidersInDistance(m_Controller.transform.position.x, moveX * m_CheckZoneSize, ELayer.Spell);
            
            // filter spells of type Zone of Enemies
            var zones = CollisionChecker.FilterSpells(colliders, ESpellType.Zone, (m_Controller.Team + 1) % 2);
            if (zones.Count == 0)
                continue;

            // if any : un-allow movement
            m_AllowedMovements.Remove(moveX);
        }
    }

    protected virtual void CheckProjectiles()
    {
        if (m_AllowedMovements.Count == 0)
            return;
    }

    #endregion
}
