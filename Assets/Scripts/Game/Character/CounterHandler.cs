using Data;
using Enums;
using Game;
using Game.Character;
using Game.Managers;
using System.Collections;
using Tools;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Game.Character
{
    public class CounterHandler : NetworkBehaviour
    {
        #region Members

        NetworkVariable<int> m_CounterProc = new NetworkVariable<int>((int)ESpell.Count);
        NetworkVariable<float> m_CounterTimer = new NetworkVariable<float>(0f);

        Controller m_Controller;

        public NetworkVariable<int> CounterProc => m_CounterProc;
        public bool HasCounter => m_CounterTimer.Value > 0;

        #endregion


        #region Inherited Manipulators

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            m_Controller = Finder.FindComponent<Controller>(gameObject);
        }

        void Update()
        {
            if (!IsServer)
                return;

            UpdateCounterTimer();
        }

        #endregion


        #region Private Manipulators

        void UpdateCounterTimer()
        {
            if (m_CounterTimer.Value <= 0)
                return;
            
            m_CounterTimer.Value -= Time.deltaTime;
            if (m_CounterTimer.Value <= 0)
            {
                EndCounter();
            }
        }

        void EndCounter()
        {
            m_CounterTimer.Value = 0;
            m_CounterProc.Value = (int)ESpell.Count;
            m_Controller.Movement.CancelMovement(false);
        }

        #endregion



        #region Public Manipulators

        public void SetCounter(ESpell CounterProc, float duration)
        {
            if (!IsServer)
                return;

            m_CounterProc.Value = (int)CounterProc;
            m_CounterTimer.Value = duration;
            m_Controller.Movement.CancelMovement(true);
        }

        public void ProcCounter(Controller enemy)
        {
            if (!IsServer)
                return;
            
            var targetPosition = enemy.transform.position;

            SpellData spellData = SpellLoader.GetSpellData((ESpell)m_CounterProc.Value);
            spellData.Cast(OwnerClientId, targetPosition, transform.position);

            EndCounter();
        }

        #endregion
    }
}