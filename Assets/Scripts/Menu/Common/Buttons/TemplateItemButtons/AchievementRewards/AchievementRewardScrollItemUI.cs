using UnityEngine;
using TMPro;
using Menu.Common.Buttons;
using Enums;
using Tools;
using UnityEngine.UI;
using Save;

public class AchievementRewardScrollItemUI : MObject
{
    #region Members

    // Data
    EAchievementReward  m_AchievementReward;
    string              m_AchievementRewardName;
    AchievementRewardUI m_AchievementRewardUI;

    // GameObjects & Components
    GameObject          m_RewardContainer;
    TMP_Text            m_Title;

    // Public Accessors
    public AchievementRewardUI AchievementRewardUI => m_AchievementRewardUI;
    public Button Button => m_AchievementRewardUI.Button;

    #endregion


    #region Init & End

    protected override void FindComponents()
    {
        base.FindComponents();

        m_RewardContainer   = Finder.Find(gameObject, "RewardContainer");
        m_Title             = Finder.FindComponent<TMP_Text>(gameObject, "Title");
    }

    public void Initialize(string arName, EAchievementReward achievementReward)
    {
        m_AchievementRewardName = arName;
        m_AchievementReward = achievementReward;

        base.Initialize();
    }

    protected override void SetUpUI()
    {
        base.SetUpUI();


        SetUpTitle();
        SetUpIcon();
    }

    #endregion


    #region GUI Manipulators

    void SetUpTitle()
    {
        if (m_AchievementReward == EAchievementReward.Title)
        {
            m_Title.gameObject.SetActive(false);
            return;
        }

        if (m_AchievementReward == EAchievementReward.Badge)
        {
            if (ProfileCloudData.TryGetBadgeFromString(m_AchievementRewardName, out EBadge badge, out ELeague league) && league != ELeague.None)
            {
                m_Title.text = TextHandler.Split(badge.ToString()) + " (" + league.ToString() + ")";
                return;
            }
        }

        m_Title.text = TextHandler.Split(m_AchievementRewardName);
    }


    void SetUpIcon()
    {
        UIHelper.CleanContent(m_RewardContainer);

        // load & instantiate template UI of reward
        m_AchievementRewardUI = Instantiate(AssetLoader.LoadAchievementRewardTemplate(m_AchievementReward), m_RewardContainer.transform);

        // init template with value
        m_AchievementRewardUI.Initialize(m_AchievementRewardName, m_AchievementReward);
    }

    #endregion

}
