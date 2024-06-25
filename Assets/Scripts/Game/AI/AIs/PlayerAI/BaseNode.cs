using System.Collections;
using Tools;
using UnityEngine;

namespace AI
{
    public class BaseNode : Node
    {
        #region Members

        protected Controller m_Controller;
        protected ProjectileTrigger m_ProjectileTrigger;
        protected ImmediatThreatTrigger m_ImmediatThreatTrigger;

        #endregion


        #region Init & End

        public BaseNode(Controller controller)
        {
            m_Controller = controller;
            m_ProjectileTrigger = Finder.FindComponent<ProjectileTrigger>(controller.gameObject);
            m_ImmediatThreatTrigger = Finder.FindComponent<ImmediatThreatTrigger>(controller.gameObject);
        }

        #endregion
    }
}