using Data;
using Game.Managers;
using Game.Spells;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Character
{
    public class SpellHandler : MonoBehaviour
    {
        public List<ESpells> SpellsList;

        Controller m_Controller;
        Transform m_Spawn;

        private void Start()
        {
            m_Controller = GetComponent<Controller>();
            m_Spawn = transform.Find("SpellSpawn");
        }

        void Update()
        {
            SelectAction();
        }


        #region Private Manipulators

        void SelectAction()
        {
            if (m_Controller.IsPlayer)
            {
                CheckInputs();
                return;
            }

            ActionIA();
            return;
        }

        /// <summary>
        /// Check if movement inputs have beed pressed
        /// </summary>
        void CheckInputs()
        {
        }

        void ActionIA()
        {
            return;
        }

        #endregion

        
        #region Public Manipulators

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spell"></param>
        public void Cast(ESpells spell)
        {
            SpellLoader.Instance.Spells[spell].Cast(m_Controller, GameManager.Instance.Arena.transform, m_Spawn.position, m_Spawn.rotation);
        }

        #endregion
    }
}