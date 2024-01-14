using Data;
using Enums;
using Game.Managers;
using Game.Spells;
using Tools;
using Unity.Netcode;
using UnityEngine;

namespace Game.Character
{
    public class CounterHandler : NetworkBehaviour
    {
        #region Members

        NetworkVariable<bool>   m_CounterActivated  = new NetworkVariable<bool>(false);
        NetworkVariable<float>  m_CounterTimer      = new NetworkVariable<float>(0f);

        Controller m_Controller;
        SCounterData m_CounterData;
        int m_HitCtr = 0;
        GameObject m_CounterGraphics;

        public NetworkVariable<bool> CounterActivated => m_CounterActivated;
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


        #region Client RPCs

        [ClientRpc]
        void SetCounterGraphicsClientRPC(bool activate)
        {
            if (activate)
                m_CounterGraphics = Instantiate(m_CounterData.CounterGraphics, m_Controller.transform);
            else
            {
                Destroy(m_CounterGraphics);
                m_CounterGraphics = null;
            }
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
            CounterActivated.Value = false;
            m_Controller.Movement.CancelMovement(false);

            // reset color on the client side
            if (m_CounterData.ColorSwap != default)
                m_Controller.AnimationHandler.ChangeColorClientRPC(Color.white);

            // destory counter graphics on the client side
            if (m_CounterGraphics != null)
                SetCounterGraphicsClientRPC(false);
        }

        #endregion


        #region Public Manipulators

        public void SetCounter(SCounterData counterData)
        {
            if (!IsServer)
                return;

            m_CounterData = counterData;
            m_CounterTimer.Value = m_CounterData.Duration;
            m_Controller.Movement.CancelMovement(true);

            if (m_CounterData.ColorSwap != default)
            {
                m_Controller.AnimationHandler.ChangeColorClientRPC(m_CounterData.ColorSwap);
            }

            if (m_CounterData.CounterGraphics != null)
            {
                SetCounterGraphicsClientRPC(true);
            }
        }

        public void ProcCounter(Spell enemySpell)
        {
            if (!IsServer)
                return;

            var targetPosition = enemySpell.Controller.transform.position;
            SpellData spellData;

            switch (m_CounterData.Type)
            {
                // cast the counter spell on the enemy
                case ECounterType.Proc:
                    spellData = SpellLoader.GetSpellData(m_CounterData.OnCounterProc);
                    spellData.Cast(OwnerClientId, targetPosition, transform.position);
                    break;

                // block the spell : do nothing
                case ECounterType.Block:
                    // todo : block animation
                    break;

                // Recast the spell to the enemy
                case ECounterType.Reflect:
                    enemySpell.SpellData.Cast(OwnerClientId, targetPosition, transform.position);
                    break;

                default :
                    Debug.LogError("Unhandled counter type : " + m_CounterData.Type);
                    break;
            }

            enemySpell.DestroySpell();

            m_HitCtr++;
            if (m_HitCtr >= m_CounterData.MaxHit && m_CounterData.MaxHit > 0)
                EndCounter();
        }

        #endregion
    }
}