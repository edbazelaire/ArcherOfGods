using Data.GameManagement;
using Inventory;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.Common.Displayers
{
    public class RewardsDisplayer : MonoBehaviour
    {
        #region Members

        GameObject m_RewardsDisplayContainer;
        HorizontalLayoutGroup m_RewardsDisplayRow;
        GameObject m_TemplateReward;

        #endregion

        public void Initialize(SRewardsData rewardsData, int maxElemPerRow = 4)
        {
            m_RewardsDisplayContainer = this.gameObject;
            m_RewardsDisplayRow = Finder.FindComponent<HorizontalLayoutGroup>(m_RewardsDisplayContainer);
            m_TemplateReward = AssetLoader.LoadTemplateItem("Reward");

            // clean items
            UIHelper.CleanContent(m_RewardsDisplayRow.gameObject);

            SetUpRewards(rewardsData.Rewards, maxElemPerRow);
        }

        public void SetUpRewards(List<SReward> rewards, int maxElemPerRow = 4)
        {
            Transform row = m_RewardsDisplayRow.transform;

            int j = 0;
            int rowCount = 0;
            for (int i = 0; i < rewards.Count; i++)
            {
                bool isNewRow = i != 0 && (j >= maxElemPerRow || (rowCount % 2 == 1 && j >= maxElemPerRow - 1));

                if (isNewRow)
                {
                    rowCount++;
                    j = 0;
                    row = Instantiate(m_RewardsDisplayRow, m_RewardsDisplayContainer.transform).GetComponent<Transform>();
                }

                var template = Instantiate(m_TemplateReward, row).GetComponent<TemplateReward>();
                template.Initialize(rewards[i]);
            }
        }
    }
}