using AI;
using Tools;

public class TaskDodge : TaskMove
{
    #region Init & End
    
    public TaskDodge(Controller controller) : base(controller) { }

    #endregion


    #region Movement Selection

    protected override void SelectMovement()
    {
        base.SelectMovement();

        if (m_AllowedMovements.Count == 0)
        {
            m_State = NodeState.FAILURE;
            return;
        }

        // get closest safe XPos on left and right
        (float safeSpotLeft, float safeSpotRight) = GetNextSafeSpots();

        // TODO : get to closest safe spot

        ErrorHandler.Log("TaskDodge()");
        m_State = NodeState.RUNNING;
    }

    /// <summary>
    /// Get next safe spots left and right 
    /// </summary>
    /// <returns></returns>
    (float, float) GetNextSafeSpots()
    {
        // TODO 
        return (0f, 0f);
    }
    
    #endregion

}
