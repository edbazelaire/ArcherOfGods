using Enums;
using MyBox;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data.AI
{
    public enum ENodeType
    {
        Selector,
        Sequence,

        Move,
        Attack,
    }


    [Serializable]
    public struct SNode
    {
        public ENodeType    NodeType;
        public List<SNode>  SubNodes;
    }

    [CreateAssetMenu(fileName = "BehaviorTreeData", menuName = "Game/AI/BehaviorTree")]
    public class BehaviorTreeData : ScriptableObject
    {
        public List<SNode> DefaultTree;

        public List<SNode> GetTree(ECharacter character)
        { 
            return DefaultTree; 
        }
    }
}