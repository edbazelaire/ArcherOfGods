using Assets.Scripts.Managers.Sound;
using Menu.MainMenu;
using Save;
using System.Collections;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.PopUps.PopUps.MessagePopUps
{
    public class PseudoPopUp : MessagePopUp
    {
        #region Members

        TMP_InputField  m_InputField;
        TMP_Text        m_ErrorMessage;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_InputField        = Finder.FindComponent<TMP_InputField>(m_WindowContent, "InputField");
            m_ErrorMessage      = Finder.FindComponent<TMP_Text>( m_WindowContent, "ErrorMessage");
        }

        protected override void OnPrefabLoaded()
        {
            base.OnPrefabLoaded();

            m_ErrorMessage.text = "";
        }

        #endregion


        #region Buttons

        protected override void OnUIButton(string bname)
        {
            switch (bname)
            {
                case "Background":
                    break;

                case "ValidateButton":
                    OnValidateButton();
                    break;

                default:
                    base.OnUIButton(bname);
                    break;
            }
        }

        #endregion


        #region Listeners

        protected override void OnValidateButton()
        {
            if (ProfileCloudData.IsGamerTagValid(m_InputField.text, out string reason))
            {
                ProfileCloudData.SetGamerTag(m_InputField.text);
                base.OnValidateButton();
                Exit();
                return;
            }

            // play error sound
            SoundFXManager.PlayOnce(SoundFXManager.ErrorSoundFX);
            
            // TODO : Add animation Vibration and set to red
            // ...

            // display why is not valid
            m_ErrorMessage.text = reason;
        }

        #endregion
    }
}