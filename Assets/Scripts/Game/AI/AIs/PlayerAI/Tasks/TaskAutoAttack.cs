using AI;
using Game.Character;

namespace Game.AI
{
    public class TaskAutoAttack : Node
    {
        #region Members

        protected Controller m_Controller;

        protected Movement m_Movement => m_Controller.Movement;

        #endregion


        #region Init & End

        public TaskAutoAttack(Controller controller)
        {
            m_Controller = controller;
        }

        #endregion


        #region Evaluate

        public override NodeState Evaluate()
        {
            m_Movement.SetMovement(0);
            return NodeState.SUCCESS;
        }

        #endregion

    }
}