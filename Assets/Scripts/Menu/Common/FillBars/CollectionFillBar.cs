﻿using Data.GameManagement;
using Inventory;
using Save;
using System;
using System.Collections;
using TMPro;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.Common
{
    public class CollectionFillBar : MObject
    {
        #region Members

        const string COLLECTION_VALUE_FORMAT = "{0} / {1}";

        [SerializeField] float  m_CollectionAnimationDuration = 1.0f;
        [SerializeField] Color  m_FullColor;
        [SerializeField] float  m_GlowSpeed;
        [SerializeField] float  m_GlowPause;

        Image                   m_CollectionFill;
        RectTransform           m_CollectionFillRectT;
        TMP_Text                m_CollectionValue;
        Color                   m_BaseColor;
        GameObject              m_Glow;

        SCollectableCloudData?  m_CollectableCloudData = null;
        int                     m_CurrentCollection;
        int                     m_MaxCollection;
        float                   m_GlowPauseTimer;

        Coroutine               m_Animation;

        public bool IsAnimated => m_Animation != null;

        #endregion


        #region Init & End

        public void Initialize(SCollectableCloudData collectableCloudData)
        {
            base.Initialize();

            RefreshCloudData(collectableCloudData);
            RegisterListeners();
        } 

        public void Initialize(int currentValue, int maxCollection)
        {
            base.Initialize();

            m_CurrentCollection = currentValue;
            m_MaxCollection = maxCollection;

            RefreshUI();
        }

        protected override void FindComponents()
        {
            m_CollectionFill = Finder.FindComponent<Image>(gameObject, "Fill");
            m_CollectionFillRectT = Finder.FindComponent<RectTransform>(m_CollectionFill.gameObject);
            m_CollectionValue = Finder.FindComponent<TMP_Text>(gameObject, "Value");
            m_BaseColor = m_CollectionFill.color;
            m_Glow = Finder.Find(gameObject, "Glow");

            m_Glow.SetActive(false);
        }

        #endregion  


        #region Update

        private void Update()
        {
            UpdateGlowAnimation();
        }

        void UpdateGlowAnimation()
        {
            // NOT ACTIVE : skip
            if (m_Glow == null || ! m_Glow.activeInHierarchy)
                return;

            // IN PAUSE : update timer and skip
            if (m_GlowPauseTimer > 0)
            {
                m_GlowPauseTimer -= Time.deltaTime;
                return;
            }    

            // REACH END : setup timer, reset position and leave
            if (m_Glow.transform.localPosition.x >= m_CollectionFill.transform.localPosition.x + m_CollectionFillRectT.rect.width)
            {
                m_GlowPauseTimer = m_GlowPause;
                m_Glow.transform.localPosition = new Vector3(m_CollectionFill.transform.localPosition.x - m_CollectionFillRectT.rect.width, m_Glow.transform.localPosition.y, 1f);
                return;
            }

            // DEFAULT : move glow from position to end 
            m_Glow.transform.localPosition += new Vector3(Time.deltaTime * m_GlowSpeed, 0, 0);
        }

        #endregion


        #region GUI Manipulators
        void RefreshUI()
        {
            if (m_MaxCollection == 0)
            {
                m_CollectionFill.fillAmount = 0;
                m_CollectionValue.text = "Maxed";
                return;
            }

            m_CollectionFill.fillAmount = Mathf.Clamp((float)m_CurrentCollection / (float)m_MaxCollection, 0, 1);
            if (m_CurrentCollection >= m_MaxCollection)
            {
                m_CollectionFill.color = m_FullColor;
                m_Glow.SetActive(true);
            }
            else
            {
                m_CollectionFill.color = m_BaseColor;
                m_Glow.SetActive(false);
            }

            m_CollectionValue.text = string.Format(COLLECTION_VALUE_FORMAT, m_CurrentCollection, m_MaxCollection);
        }

        #endregion


        #region Collection Manipulators

        public void RefreshCloudData(SCollectableCloudData? cloudData = null)
        {
            if (cloudData.HasValue) 
                m_CollectableCloudData = cloudData.Value;

            if (!m_CollectableCloudData.HasValue)
                return;

            UpdateCollection(m_CollectableCloudData.Value.Qty, CollectablesManagementData.GetLevelData(m_CollectableCloudData.Value.GetCollectable(), m_CollectableCloudData.Value.Level).RequiredQty);
        }

        public void UpdateCollection(int newValue, int? maxCollection = null)
        {
            if (maxCollection != null)
                m_MaxCollection = maxCollection.Value;

            m_CurrentCollection = newValue;
            RefreshUI();
        }

        public void Add(int amount)
        {
            UpdateCollection(m_CurrentCollection + amount);
        }

        public void AddCollectionAnimation(int amount)
        {
            m_Animation = StartCoroutine(CollectionAnimationCoroutine(amount));
        }

        public IEnumerator CollectionAnimationCoroutine(int amount)
        {
            int goal = m_CurrentCollection + amount;
            float newValue = m_CurrentCollection;
            while (m_CurrentCollection < goal) 
            {
                newValue += (amount * Time.deltaTime / m_CollectionAnimationDuration);
                UpdateCollection(Math.Min((int)Math.Round(newValue), goal));
                yield return null;
            }

            m_Animation = null;
        }

        #endregion


        #region Listeners

        protected override void RegisterListeners()
        {
            base.RegisterListeners();

            InventoryCloudData.CollectableDataChangedEvent += OnCollectableCloudDataChanged;
        }

        protected override void UnRegisterListeners()
        {
            InventoryCloudData.CollectableDataChangedEvent -= OnCollectableCloudDataChanged;
        }

        void OnCollectableCloudDataChanged(SCollectableCloudData cloudData)
        {
            if (m_CollectableCloudData == null)
                return;
            
            if (! m_CollectableCloudData.Value.GetCollectable().Equals(cloudData.GetCollectable()))
                return;

            RefreshCloudData(cloudData);
        }

        #endregion
    }
}