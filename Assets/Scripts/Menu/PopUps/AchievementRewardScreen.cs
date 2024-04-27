using Data;
using Enums;
using Menu.Common.Buttons;
using Menu.PopUps;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Tools;
using Tools.Animations;
using Unity.VisualScripting;
using UnityEngine;

namespace Scripts.Menu.PopUps
{
    public class AchievementRewardScreen : OverlayScreen
    {
        #region Members

        // Data
        List<SAchievementRewardData> m_AchivementRewardDataList;

        // GameObjects & Components
        GameObject      m_RewardContainer;
        TMP_Text        m_RewardName;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_RewardContainer   = Finder.Find(gameObject, "RewardContainer");
            m_RewardName        = Finder.FindComponent<TMP_Text>(gameObject, "RewardName");
        }

        public void Initialize(List<SAchievementRewardData> data)
        {
            m_AchivementRewardDataList = data;

            base.Initialize();
        }

        protected override void OnInitializationCompleted()
        {
            base.OnInitializationCompleted();

            StartCoroutine(StartDisplay());
        }

        #endregion


        #region GUI Manipulators

        /// <summary>
        ///  Display all rewards of the list
        /// </summary>
        /// <returns></returns>
        IEnumerator StartDisplay()
        {
            foreach (SAchievementRewardData data in m_AchivementRewardDataList)
            {
                yield return DisplayReward(data);
                yield return new WaitUntil(() => Input.touchCount > 0 || Input.GetMouseButtonDown(0));
            }

            Exit();
        }

        /// <summary>
        /// Display a provided reward
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        IEnumerator DisplayReward(SAchievementRewardData data)
        {
            // clean previous content
            UIHelper.CleanContent(m_RewardContainer);

            // set & hide reward name container
            m_RewardName.text = TextHandler.Split(data.Value);
            m_RewardName.gameObject.SetActive(false);

            // load & instantiate template UI of reward
            var template = Instantiate(AssetLoader.LoadAchievementRewardTemplate(data.AchievementReward), m_RewardContainer.transform);
            
            // init template with value
            template.Initialize(data.Value, data.AchievementReward);

            // add animation
            yield return ApplyStartAnimation(template);
        }

        #endregion


        #region Animations

        IEnumerator ApplyStartAnimation(AchievementRewardUI template)
        {
            // add fadeIn enter animation
            var fadeIn = template.AddComponent<Fade>();
            fadeIn.Initialize(duration: 0.2f, startOpacity: 0, startScale: 0.8f);

            yield return new WaitUntil(() => fadeIn.IsOver);

            // add raycast animation the background
            AnimationHandler.AddRaycast(m_RewardContainer, color: new Color(1, 0.95f, 0.6f, 0.6f));

            // display name of the reward (with animation)
            m_RewardName.gameObject.SetActive(true);
            var fadeInTitle = m_RewardName.AddComponent<Fade>();
            fadeInTitle.Initialize(duration: 0.2f, startOpacity: 0, startScale: 0.8f);
        }

        #endregion
    }
}