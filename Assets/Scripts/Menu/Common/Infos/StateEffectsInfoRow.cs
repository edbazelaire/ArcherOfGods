using Data;
using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace Menu.Common.Infos
{
    public class StateEffectsInfoRow : InfoRowUI
    {
        #region Members

        GameObject    m_ValueContainer;

        List<TemplateStateEffectIconUI> m_StateEffectIcons;

        #endregion


        #region Init & End

        void Awake()
        {
            var infosContainer  = Finder.Find(gameObject, "InfosContainer");
            m_ValueContainer    = Finder.Find(infosContainer, "ValueContainer");
        }


        public void Initialize(List<SStateEffectData> stateEffectDatas, int spellLevel)
        {
            m_StateEffectIcons = new();
            UIHelper.CleanContent(m_ValueContainer);
            foreach (var stateEffectData in stateEffectDatas)
            {
                var icon = Instantiate(AssetLoader.LoadTemplateItem("StateEffectIcon"), m_ValueContainer.transform).GetComponent<TemplateStateEffectIconUI>();
                icon.Initialize(stateEffectData, spellLevel);

                m_StateEffectIcons.Add(icon);
            }
        }

        #endregion
    }
}