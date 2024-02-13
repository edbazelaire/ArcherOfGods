using UnityEngine;

namespace Save
{
    public class CloudSaveManager : MonoBehaviour
    {
        #region Members

        InventoryCloudData          m_InventoryCloudData;
        ChestsCloudData             m_ChestsCloudData;

        public InventoryCloudData   InventoryCloudData      => m_InventoryCloudData;
        public ChestsCloudData      ChestsCloudData         => m_ChestsCloudData;

        /// <summary> check that all cloud data have been loaded </summary>
        public bool LoadingCompleted => 
            m_InventoryCloudData != null && m_InventoryCloudData.LoadingCompleted
            && m_ChestsCloudData != null && m_ChestsCloudData.LoadingCompleted;

        #endregion

        public void LoadSave()
        {
            m_InventoryCloudData = new InventoryCloudData();
            m_ChestsCloudData = new ChestsCloudData();
        }

        public void SaveAll()
        {
            m_InventoryCloudData.Save();
        }
    }
}