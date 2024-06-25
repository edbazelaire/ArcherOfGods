using System.Collections;
using TMPro;
using Tools;
using UnityEngine;

namespace Assets.Scripts.Game.UI.GameUI
{
    public class ShieldBarUI : PlayerBarUI
    {
        protected override void FindComponents()
        {
            base.FindComponents();
            m_Text = Finder.FindComponent<TMP_Text>(gameObject, "ShieldText", throwError: false);
        }

        protected override string GetText()
        {
            return m_CurrentValue > 0 ? "+ " + m_CurrentValue : "";
        }
    }
}