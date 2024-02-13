using Data;
using Enums;
using System.Collections.Generic;
using Tools;

namespace Game.Managers
{
    public static class ItemLoader
    {
        #region Members

        static ChestRewardData[] m_ChestRewardData;

        #endregion


        #region Init & End

        #endregion
        
        static void Initialize()
        {
            m_ChestRewardData = AssetLoader.LoadAll<ChestRewardData>(AssetLoader.c_ChestsDataPath);
        }

        public static ChestRewardData GetChestRewardData(EChestType chestType)
        {
            if (m_ChestRewardData == null || m_ChestRewardData.Length == 0)
                Initialize();

            foreach (var item in m_ChestRewardData)
                if (item.ChestType == chestType)
                    return item;

            ErrorHandler.Error("Unable to find ChestRewardData matching chest type : " + chestType);
            return null;
        }


    }
}