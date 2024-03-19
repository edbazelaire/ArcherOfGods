using Enums;
using Game;
using Inventory;
using Network;
using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using Tools;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;


public class EndGameUI : MonoBehaviour
{
    #region Members

    const string GOLDS_FORMAT = "+ {0}";
    const string c_TitleText = "TitleText";
    const string c_LeaveButton = "LeaveButton";

    TMP_Text    m_TitleText;
    GameObject  m_RewardsContent;
    GameObject  m_GoldsRewardDisplay;
    TMP_Text    m_GoldsQty;
    Image       m_ChestRewardIcon;
    Button      m_LeaveButton;

    #endregion


    // Use this for initialization
    public void SetUpGameOver(bool win)
    {
        InitializeUI();

        m_TitleText.text = win ? "Victory" : "Defeat";
        m_TitleText.color = win ? Color.green : Color.red;

        SRewardCalculator reward = win ? Rewarder.WinGameReward : Rewarder.LossGameReward;

        // ----------------------------------------------------------------------------
        // GOLDS   
        int golds = reward.GetGolds();
        if (golds <= 0)
            m_GoldsRewardDisplay.SetActive(false);
        else
        {
            m_GoldsRewardDisplay.SetActive(true);
            m_GoldsQty.text = string.Format(GOLDS_FORMAT, golds);
            InventoryManager.AddGolds(golds);
        }

        // ----------------------------------------------------------------------------
        // CHESTS
        // init chests rewards to empty list
        List<EChestType> chests = new();

        // check if any index is available to store the chest (otherwise : no chest reward)
        if (InventoryManager.GetFirstAvailableIndex(out int index))
            chests = reward.GetChests();
        
        if (chests.Count == 0)
        {
            m_ChestRewardIcon.gameObject.SetActive(false);
        } else
        {
            m_ChestRewardIcon.gameObject.SetActive(true);
            if (chests.Count > 1)
                ErrorHandler.Warning("Multiple chests provided to EndGameUI : case not handled");

            m_ChestRewardIcon.sprite = AssetLoader.LoadChestIcon(chests[0]);
            InventoryManager.AddChest(chests[0]);
        }

        m_LeaveButton.onClick.AddListener(Leave);
    }

    void InitializeUI()
    {
        m_TitleText             = Finder.FindComponent<TMP_Text>(gameObject, c_TitleText);
        m_LeaveButton           = Finder.FindComponent<Button>(gameObject, c_LeaveButton);
        m_RewardsContent        = Finder.Find(gameObject, "RewardsContent");
        m_GoldsRewardDisplay    = Finder.Find(m_RewardsContent, "GoldsRewardDisplay");
        m_GoldsQty              = Finder.FindComponent<TMP_Text>(m_GoldsRewardDisplay, "Qty");
        m_ChestRewardIcon       = Finder.FindComponent<Image>(m_RewardsContent, "ChestRewardIcon");
    }

    void Leave()
    {
        // reset network manager
        NetworkManager.Singleton.Shutdown();

        // reset GameManager
        GameManager.Instance.Shutdown();

        // load MainMenu
        SceneLoader.Instance.LoadScene("MainMenu");
    }
}
