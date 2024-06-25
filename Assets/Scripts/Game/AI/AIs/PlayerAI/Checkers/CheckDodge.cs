using AI;
using Tools;

public class CheckDodge : BaseChecker
{
    #region Members

    #endregion


    #region Init & End
    
    public CheckDodge(Controller controller) : base(controller) { }

    #endregion


    #region Evaluation

    public override NodeState Evaluate()
    {
        base.Evaluate();

        if (m_State != NodeState.FAILURE)
            return m_State;

        // checks if is on a spell preview
        if (m_ImmediatThreatTrigger.CheckTriggerSpellSpawn())
        {
            m_State = NodeState.SUCCESS;
            return m_State;
        }

        // checks that is not in the trajectory of projectile
        if (m_ProjectileTrigger.IsTriggered)
        {
            m_State = NodeState.SUCCESS;
            return m_State;
        }

        return m_State;
    }

    #endregion


}
