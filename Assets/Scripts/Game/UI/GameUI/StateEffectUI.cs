using Enums;
using Game.Managers;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class StateEffectUI : MonoBehaviour
    {
        const string    c_Icon              = "Icon";
        const string    c_StacksContainer   = "StacksContainer";
        const string    c_Stacks            = "Stacks";
        const string    c_TimerFill         = "TimerFill";

        Image           m_Icon;
        GameObject      m_StacksContainer;
        TMP_Text        m_Stacks;
        Image           m_TimerFill;

        float           m_Duration;
        float           m_Timer;

        void Awake()
        {
            m_Icon              = Finder.FindComponent<Image>(gameObject, c_Icon);
            m_StacksContainer   = Finder.Find(gameObject, c_StacksContainer);
            m_Stacks            = Finder.FindComponent<TMP_Text>(gameObject, c_Stacks);
            m_TimerFill         = Finder.FindComponent<Image>(gameObject, c_TimerFill);
        }

        // Update is called once per frame
        void Update()
        {
            if (m_Timer <= 0)
                return;

            m_Timer -= Time.deltaTime;
            m_TimerFill.fillAmount = Mathf.Clamp01(m_Timer / m_Duration);
        }

        public void Initialize(EStateEffect stateEffect, int stacks, float duration)
        {
            // Setup icon (if found)
            Sprite icon = AssetLoader.LoadStateEffectIcon(stateEffect);
            if (icon != null)
                m_Icon.sprite   = icon;
            
            // Setup stacks (if any)
            if (stacks <= 1)
                m_StacksContainer.SetActive(false);
            else
            {
                m_StacksContainer.SetActive(true);
                m_Stacks.text = stacks.ToString();
            }

            m_Duration      = duration;
            m_Timer         = duration;             // reset timer
        }
    }
}