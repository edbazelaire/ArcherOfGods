using Inventory;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.Common
{
    public class TemplateReward : MonoBehaviour
    {
        #region Members

        Image m_Icon;
        TMP_Text m_Qty;

        #endregion


        #region Init & End

        public void Initialize(SReward reward)
        {
            m_Icon = Finder.FindComponent<Image>(gameObject, "Icon");
            m_Qty = Finder.FindComponent<TMP_Text>(gameObject, "Qty");

            m_Icon.sprite = AssetLoader.LoadIcon(reward.RewardName, reward.RewardType);
            if (reward.Qty <= 1)
                m_Qty.gameObject.SetActive(false);
            else 
                m_Qty.text = "x " + reward.Qty.ToString();
        }

        #endregion
    }
}