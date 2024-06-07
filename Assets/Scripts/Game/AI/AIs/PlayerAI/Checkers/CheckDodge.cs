using AI;
using Enums;
using Game.AI;
using Tools;
using UnityEngine;

public class CheckDodge : Node
{
    #region Members

    // =============================================================================
    // Component & GameObjects
    private Controller m_Controller;

    // =============================================================================
    // Data
   
    Vector2 m_Position => m_Controller.transform.position;

    #endregion


    #region Init & End
    
    public CheckDodge(Controller controller)
    {
        m_Controller = controller;
    }

    #endregion


    #region Movement Evaluation

    public override NodeState Evaluate()
    {
        m_State = NodeState.SUCCESS;

        // checks if is on a spell preview
        if (CheckSpellPreviews())
            return m_State;

        // checks that is not in the trajectory of projectile
        if (CheckProjectiles())
            return m_State;

        m_State = NodeState.FAILURE;
        return m_State;
    }


    /// <summary>
    /// Check if character is standing on a Spell Preview
    /// </summary>
    /// <returns></returns>
    bool CheckSpellPreviews()
    {
        // Check for collisions within a circle with variableRadius radius
        Collider2D[] colliders = CollisionChecker.GetControllerCollisions(m_Controller, ELayer.SpellSpawn);

        // Iterate through all colliders found
        foreach (Collider2D collider in colliders)
        {
            ErrorHandler.Log("CheckSpellPreviews() : SpellSpawn DETECTED", ELogTag.AICheckers);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check if character is on the trajectory of a Projectile
    /// </summary>
    /// <returns></returns>
    bool CheckProjectiles()
    {
        // Define the range and width of the detection area in front of the character
        float detectionRange = 3f; // How far ahead to check for projectiles
        float detectionWidth = 2f; // The width of the detection area to consider (for vertical variation in projectile paths)

        // Calculate the forward direction relative to the character (assuming left is forward)
        Vector2 forward = new Vector3(-1, 0, 0);

        // Calculate the start position of the detection area (can adjust the y-value as needed to align with character height)
        Vector2 detectionOrigin = m_Position + (forward * detectionWidth / 2);

        // Perform a Physics2D BoxCast in the forward direction to detect incoming projectiles
        RaycastHit2D hit = Physics2D.BoxCast(detectionOrigin, new Vector2(detectionWidth, detectionWidth), 0f, forward, detectionRange, LayerMask.GetMask("Spell"));

        // If a hit is detected and it's a projectile, we know to dodge
        if (hit.collider != null)
        {
            // Optionally, you can check the specific type of projectile, speed, etc.
            ErrorHandler.Log("CheckSpellPreviews() : Projectile DETECTED", ELogTag.AICheckers);
            return true;
        }

        return false;
    }

    #endregion


}
