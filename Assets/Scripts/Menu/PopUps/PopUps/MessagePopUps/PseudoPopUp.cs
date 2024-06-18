using Assets.Scripts.Managers.Sound;
using Save;
using Save.RSDs;
using TMPro;
using Tools;

namespace Menu.PopUps.PopUps.MessagePopUps
{
    public class PseudoPopUp : MessagePopUp
    {
        #region Members

        TMP_InputField  m_InputField;
        TMP_InputField  m_TokenInputField;
        TMP_Text        m_ErrorMessage;

        #endregion


        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_InputField        = Finder.FindComponent<TMP_InputField>(m_WindowContent, "InputField");
            m_TokenInputField   = Finder.FindComponent<TMP_InputField>(m_WindowContent, "TokenInputField");
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
            // CHECK : Gamer tag
            if (! ProfileCloudData.IsGamerTagValid(m_InputField.text, out string reason))
            {
                // play error sound
                SoundFXManager.PlayOnce(SoundFXManager.ErrorSoundFX);

                // display why is not valid
                m_ErrorMessage.text = reason;
                return;
            }

            // CHECK : Token
            if (!TokensRSD.IsTokenValid(m_TokenInputField.text, out reason))
            {
                // play error sound
                SoundFXManager.PlayOnce(SoundFXManager.ErrorSoundFX);

                // display why is not valid
                m_ErrorMessage.text = reason;
                return;
            }

            ProfileCloudData.SetGamerTag(m_InputField.text);
            ProfileCloudData.SetToken(m_TokenInputField.text);

            base.OnValidateButton();
            Exit();
        }

        #endregion
    }
}