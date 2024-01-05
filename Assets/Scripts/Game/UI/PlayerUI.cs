using System.Collections;
using TMPro;
using Tools;
using Unity.Netcode;
using UnityEngine;

namespace Game.UI
{
    public class PlayerUI : MonoBehaviour
    {
        const string c_HealthBar = "HealthBar";
        const string c_EnergyBar = "EnergyBar";

        PlayerBarUI m_HealthBar;
        PlayerBarUI m_EnergyBar;
        TMP_Text m_PlayerName;

        public void Initialize(ulong clientId)
        {
            Debug.Log("Initialize PlayerUI");
            Debug.Log($"        + ClientID: {clientId}");
            Debug.Log($"        + LocalId: {NetworkManager.Singleton.LocalClientId}");

            var controller = GameManager.Instance.GetPlayer(clientId);

            // Player Name
            m_PlayerName = Finder.FindComponent<TMP_Text>(gameObject, "PlayerName");
            m_PlayerName.text = controller.Character.ToString();

            if (NetworkManager.Singleton.LocalClientId == clientId)
                m_PlayerName.color = Color.green;

            // HealthBar
            m_HealthBar = Finder.FindComponent<PlayerBarUI>(gameObject, c_HealthBar);
            m_HealthBar.Initialize(controller.Life.Hp.Value, controller.Life.MaxHp.Value);
            controller.Life.MaxHp.OnValueChanged    += m_HealthBar.OnMaxValueChanged;
            controller.Life.Hp.OnValueChanged       += m_HealthBar.OnValueChanged;

            // Energy Bar
            m_EnergyBar = Finder.FindComponent<PlayerBarUI>(gameObject, c_EnergyBar);
            m_EnergyBar.Initialize(controller.EnergyHandler.Energy.Value, controller.EnergyHandler.MaxEnergy.Value);
            controller.EnergyHandler.MaxEnergy.OnValueChanged   += m_EnergyBar.OnMaxValueChanged;
            controller.EnergyHandler.Energy.OnValueChanged      += m_EnergyBar.OnValueChanged;
        }
    }
}