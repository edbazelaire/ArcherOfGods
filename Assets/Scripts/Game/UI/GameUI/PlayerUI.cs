using Enums;
using Game.Managers;
using System.Collections.Generic;
using TMPro;
using Tools;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class PlayerUI : MonoBehaviour
    {
        const string        c_HealthBar         = "HealthBar";
        const string        c_EnergyBar         = "EnergyBar";
        const string        c_StateDisplayer    = "StateDisplayer";

        [SerializeField] GameObject   m_TemplateStateEffect;

        SpellItemUI         m_UltimateButton;
        PlayerBarUI         m_HealthBar;
        PlayerBarUI         m_ShieldBar;
        PlayerBarUI         m_EnergyBar;
        TMP_Text            m_PlayerName;
        GameObject          m_StateDisplayer;

        Dictionary<string, StateEffectUI> m_StateEffectsUI;

        public void Initialize(ulong clientId)
        {
            Debug.Log("Initialize PlayerUI");
            Debug.Log($"        + ClientID: {clientId}");
            Debug.Log($"        + LocalId: {NetworkManager.Singleton.LocalClientId}");

            var controller = GameManager.Instance.GetPlayer(clientId);

            // Player Name
            m_PlayerName = Finder.FindComponent<TMP_Text>(gameObject, "PlayerName");
            m_PlayerName.text = controller.PlayerName.ToString();

            if (NetworkManager.Singleton.LocalClientId == clientId)
                m_PlayerName.color = Color.green;

            // Character Icon
            var characterData = CharacterLoader.GetCharacterData(controller.Character);
            m_UltimateButton = Finder.FindComponent<SpellItemUI>(gameObject, "UltimateButton");
            m_UltimateButton.Initialize(characterData.Ultimate, controller.CharacterLevel);

            // HealthBar
            m_HealthBar = Finder.FindComponent<PlayerBarUI>(gameObject, c_HealthBar);
            m_HealthBar.Initialize(controller.Life.Hp.Value, controller.Life.MaxHp.Value);
            controller.Life.MaxHp.OnValueChanged    += m_HealthBar.OnMaxValueChanged;
            controller.Life.Hp.OnValueChanged       += m_HealthBar.OnValueChanged;

            // HealthBar
            m_ShieldBar = Finder.FindComponent<PlayerBarUI>(gameObject, "ShieldBar");
            m_ShieldBar.Initialize(controller.StateHandler.RemainingShield.Value, controller.Life.MaxHp.Value);
            controller.Life.MaxHp.OnValueChanged                    += m_ShieldBar.OnMaxValueChanged;
            controller.StateHandler.RemainingShield.OnValueChanged  += m_ShieldBar.OnValueChanged;

            // Energy Bar
            m_EnergyBar = Finder.FindComponent<PlayerBarUI>(gameObject, c_EnergyBar);
            m_EnergyBar.Initialize(controller.EnergyHandler.Energy.Value, controller.EnergyHandler.MaxEnergy.Value);
            controller.EnergyHandler.MaxEnergy.OnValueChanged   += m_EnergyBar.OnMaxValueChanged;
            controller.EnergyHandler.Energy.OnValueChanged      += m_EnergyBar.OnValueChanged;

            // State Displayer
            m_StateDisplayer = Finder.Find(gameObject, c_StateDisplayer);
            m_StateEffectsUI = new Dictionary<string, StateEffectUI>();
            UIHelper.CleanContent(m_StateDisplayer);
            controller.StateHandler.OnStateEvent += OnStateEvent;
        }

        #region State Displayer

        void OnStateEvent(EListEvent listEvent, string state, int stack, float duration)
        {
            // check that is not one of the state that are not displayed
            if (state == EStateEffect.Jump.ToString())
                return;

            switch (listEvent)
            {
                case EListEvent.Add:
                    AddState(state, stack, duration);
                    break;
                case EListEvent.Remove:
                    RemoveState(state);
                    break;
                default:
                    Debug.Log("Unhandled case");
                    break;
            }
        }

        void AddState(string state, int stack, float duration)
        {
            // if not in existing state, create it and add it to the list
            if (! m_StateEffectsUI.ContainsKey(state))
            {
                GameObject stateEffectUI = Instantiate(m_TemplateStateEffect, m_StateDisplayer.transform);
                m_StateEffectsUI.Add(state, stateEffectUI.GetComponent<StateEffectUI>());
            }
          
            // initialize the state (or refresh it)
            m_StateEffectsUI[state].Initialize(state, stack, duration);
        }

        void RemoveState(string state)
        {
            // if not in existing state, create it and add it to the list
            if (! m_StateEffectsUI.ContainsKey(state))
            {
                ErrorHandler.Error($"Unable to find remvoed state {state} in list");
                return;
            }

            // destroy state and remove from list
            Destroy(m_StateEffectsUI[state].gameObject);
            m_StateEffectsUI.Remove(state);
        }

        #endregion
    }
}