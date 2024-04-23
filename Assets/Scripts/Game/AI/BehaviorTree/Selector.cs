using Data.AI;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace AI
{
    public class Selector : Node
    {
        public Selector() : base() { }
        public Selector(List<Node> children) : base(children) { }

        public override NodeState Evaluate()
        {
            foreach (Node node in m_Children)
            {
                switch (node.Evaluate())
                {
                    case NodeState.FAILURE:
                        continue;
                    case NodeState.SUCCESS:
                        m_State = NodeState.SUCCESS;
                        return m_State;
                    case NodeState.RUNNING:
                        m_State = NodeState.RUNNING;
                        return m_State;
                    default:
                        continue;
                }
            }

            m_State = NodeState.FAILURE;
            return m_State;
        }

    }

}
