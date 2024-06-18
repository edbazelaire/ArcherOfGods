using Assets;
using Enums;
using Network;
using System.Collections;
using TMPro;
using Tools;
using Tools.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.PopUps
{
    public class LobbyScreen : OverlayScreen
    {
        #region Members

        [SerializeField] string m_NumPlayersMessage = "Players found {0} / {1}";

        TMP_Text m_SubMessageText;
        Image m_Logo;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_SubMessageText = Finder.FindComponent<TMP_Text>(gameObject, "SubMessageText");
            m_Logo = Finder.FindComponent<Image>(gameObject, "Logo");
        }

        protected override void OnInitializationCompleted()
        {
            base.OnInitializationCompleted();

            ApplyLogoAnimation();
        }

        #endregion


        #region Inherited Manipulators

        protected override void Update()
        {
            base.Update();

            if (! m_Initialized)
                return;

            RefreshPlayerCount();
        }

        #endregion


        #region GUI Manipulators

        void RefreshPlayerCount()
        {
            m_SubMessageText.text = string.Format(m_NumPlayersMessage, LobbyHandler.Instance.NPlayers, LobbyHandler.Instance.MaxPlayers);
        }

        void ApplyLogoAnimation()
        {
            var rotateAnim = m_Logo.gameObject.AddComponent<RotateAnimation>();
            rotateAnim.Initialize(duration: -1, rotation: new Vector3(0, 0, -360));
        }

        #endregion


        #region Listeners

        protected override void OnCancelButton()
        {
            LobbyHandler.Instance.LeaveLobby();
            Main.SetState(EAppState.MainMenu);

            base.OnCancelButton();
        }

        #endregion
    }
}