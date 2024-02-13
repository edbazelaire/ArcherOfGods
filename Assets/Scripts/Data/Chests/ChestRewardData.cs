using Enums;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    struct SpellRewardData
    {
        public ERarety Rarety;
        public int Strikes;
        public int Count;
    }

    [CreateAssetMenu(fileName = "ChestRewardData", menuName = "Game/ChestRewardData")]
    public class ChestRewardData : ScriptableObject
    {
        [Header("Identity")]
        public EChestType ChestType;
        public Sprite Image;
        public int UnlockTime;

        [Header("Collectables")]
        public int[] Golds = new int[2];


        #region Public Manipulators

        public Dictionary<EReward, object> GetRewards()
        {
            return new Dictionary<EReward, object>()
            {
                { EReward.Golds, Random.Range(Golds[0], Golds[1]) }
            };
        }

        #endregion
    }
}