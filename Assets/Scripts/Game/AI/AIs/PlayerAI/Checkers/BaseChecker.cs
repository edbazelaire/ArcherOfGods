using System.Collections;
using Tools;
using UnityEngine;

namespace AI
{
    public class BaseChecker : BaseNode
    {
        #region Init & End

        public BaseChecker(Controller controller) : base(controller) { }

        #endregion


        #region Evaluate

        public override NodeState Evaluate()
        {
            m_State = NodeState.FAILURE;
            return m_State;
        }

        protected virtual bool RandomActivation()
        {
            return m_Controller.BehaviorTree.Randomness > Random.Range(0f, 1f);
        }

        #endregion
    }
}