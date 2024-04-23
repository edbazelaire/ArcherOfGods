using System;
using System.Collections.Generic;


namespace Inventory
{
    public struct SReward
    {
        public const string METADATA_KEY_SPELL_TYPE = "SpellType";

        public Type RewardType;
        public string RewardName;
        public int Qty;
        public Dictionary<string, object> Metadata;

        public SReward(Type rewardType, string name, int count, Dictionary<string, object> metadata = default)
        {
            RewardType  = rewardType;
            RewardName  = name;
            Qty         = count;
            Metadata    = metadata;
        }

        public void AddQty(int qty)
        {
            Qty += qty;
        }
    }
}