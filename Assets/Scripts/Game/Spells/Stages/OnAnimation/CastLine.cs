using Game.Character;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Game.Spells
{
    public class CastLine : MonoBehaviour
    {
        #region Members

        const string c_Graphics = "Graphics";
        const float c_ColorOpacity = 0.5f;

        SpellHandler m_SpellHandler;
        float m_AnimationTimer;
        float m_Distance;

        Color m_Color = Color.red;
        SpriteRenderer m_Graphics;

        #endregion


        #region Inherited Manipulators

        private void Update()
        {
            UpdateColor();
        }

        #endregion


        #region Initialization

        public void Initialize(SpellHandler spellHandler, float animationTimer, float distance)
        {
            m_SpellHandler = spellHandler;
            m_Color.a = c_ColorOpacity;
            m_AnimationTimer = animationTimer;
            m_Distance = distance;

            m_Graphics = Finder.FindComponent<SpriteRenderer>(gameObject, c_Graphics);
            m_Graphics.color = m_Color;
            m_Graphics.size = new Vector2(m_Distance, m_Graphics.size.y);

            // TODO : replace with AnimationTimer.OnValueChanged
            //spellHandler.CastEndedEvent += OnCastEnded;
        }

        #endregion


        #region Private Manipulators

        void End()
        {
            Destroy(gameObject);
        }

        void OnCastEnded()
        {
            End();
        }

        void UpdateColor()
        {
            m_Color.a = c_ColorOpacity + (1 - c_ColorOpacity) * (1 - m_SpellHandler.AnimationTimer / m_AnimationTimer);
            m_Graphics.color = m_Color;
        }

        #endregion
    }
}