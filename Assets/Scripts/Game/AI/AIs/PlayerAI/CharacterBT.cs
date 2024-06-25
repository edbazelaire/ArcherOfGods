using System.Collections.Generic;
using AI;
using Game.AI;
using Tools;

public class CharacterBT : BehaviorTree
{
    protected override Node SetupTree()
    {
        // ===============================================================================================
        // TODO : Create tree from data
        //BehaviorTreeData btData = AssetLoader.Load<BehaviorTreeData>("BehaviorTreeData", AssetLoader.c_AIDataPath);
        //List<SNode> tree = btData.GetTree(m_Controller.Character);
        // ===============================================================================================

        if (m_Controller == null)
        {
            ErrorHandler.Error("No controller found for this CharacterBT");
            return new Node();
        }

        Node root = new Selector(new List<Node>
        {
            new Sequence(new List<Node> {
                new CheckRandom(m_Controller),
                new Selector(new List<Node>
                {
                    new TaskAttack(m_Controller),
                    new TaskCounter(m_Controller),
                    new TaskJump(m_Controller),
                    new TaskMove(m_Controller),
                    new TaskAutoAttack(m_Controller),
                }, random: true),
             }),

            // Check Immadiat Threats (Zones & Projectiles)
            new Sequence(new List<Node> {
                new CheckImmediatThreat(m_Controller),
                new Selector(new List<Node>
                {
                    new TaskCounter(m_Controller),
                    new TaskJump(m_Controller),
                }),
            }),

            // Check if character is currently in a ZoneSpell
            new Sequence(new List<Node> {
                new CheckInZone(m_Controller),
                new TaskExitZone(m_Controller),
            }),

            // Check if character has imperative to dodge
            new Sequence(new List<Node> { 
                new CheckDodge(m_Controller),
                new TaskDodge(m_Controller),
            }),

            new TaskAttack(m_Controller),
            new TaskAutoAttack(m_Controller),
        });

        return root;
    }
}
