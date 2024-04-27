using Data;
using Enums;
using Tools;

namespace Game.Loaders
{
    public static class ItemLoader
    {
        #region Members

        static ChestRewardData[] m_ChestRewardData;

        public static ChestRewardData[] ChestRewardData => m_ChestRewardData;

        #endregion


        #region Init & End

        public static void Initialize()
        {
            m_ChestRewardData = AssetLoader.LoadAll<ChestRewardData>(AssetLoader.c_ChestsDataPath);
        }

        #endregion


        public static ChestRewardData GetChestRewardData(EChest chestType)
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