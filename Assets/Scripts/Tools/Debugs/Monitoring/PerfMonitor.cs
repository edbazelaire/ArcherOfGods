using Assets;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Tools.Debugs.Monitoring
{
    public class PerfMonitor : MObject
    {
        #region Members

        private float m_LastValue;
        private List<float> m_Values = new List<float>();
 
        LineRenderer    m_LineRenderer;
        TMP_Text        m_Text;
        TMP_Text        m_AvgText;
        TMP_Text        m_MinText;
        TMP_Text        m_MaxText;
        TMP_Text        m_CountBelowText;
        TMP_Text        m_CountAboveText;

        string m_ValueName;
        float m_BadThreshold;
        float m_GoodThreshold;
        float m_MinValue;
        float m_MaxValue;
        float m_GraphWidth;
        float m_GraphHeight;

        float m_MinRecordedValue;
        float m_MaxRecordedValue;
        float m_Avg;
        float m_Count;
        float m_CountBelow;
        float m_CountAbove;

        public List<float> Values => m_Values;
        public TMP_Text Text => m_Text;

        #endregion

        #region Init & End

        protected override void FindComponents()
        {
            base.FindComponents();

            m_LineRenderer = Finder.FindComponent<LineRenderer>(gameObject);
            m_Text = Finder.FindComponent<TMP_Text>(gameObject);
            m_AvgText = Finder.FindComponent<TMP_Text>(gameObject, "Avg");
            m_MinText = Finder.FindComponent<TMP_Text>(gameObject, "Min");
            m_MaxText = Finder.FindComponent<TMP_Text>(gameObject, "Max");
            m_CountBelowText = Finder.FindComponent<TMP_Text>(gameObject, "CountBelow");
            m_CountAboveText = Finder.FindComponent<TMP_Text>(gameObject, "CountAbove");

            var rectT = Finder.FindComponent<RectTransform>(m_LineRenderer.gameObject);
            m_GraphWidth = rectT.rect.width;
            m_GraphHeight = rectT.rect.height;
        }

        public void Initialize(string valueName, float badThreshold, float goodThreshold, float min = 0, float max = 1000)
        {
            m_ValueName = valueName;
            m_BadThreshold = badThreshold;
            m_GoodThreshold = goodThreshold;

            m_MinValue = min;
            m_MaxValue = max;

            base.Initialize();

            ActivateGraphs(false);

            if (m_BadThreshold == m_GoodThreshold && m_BadThreshold == 0)
            {
                m_CountBelowText.transform.parent.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Graph 

        public void ActivateGraphs(bool activate)
        {
            m_LineRenderer.gameObject.SetActive(activate);
        }

        public void AddValue(float value)
        {
            if (value <= 0)
                return;

            if (value < m_MinRecordedValue || m_LastValue == 0)
                m_MinRecordedValue = value;

            if (value > m_MaxRecordedValue || m_LastValue == 0)
                m_MaxRecordedValue = value;

            m_LastValue = value;

            m_Avg = (m_Avg * m_Count + value) / ++m_Count;

            if (m_GoodThreshold != m_BadThreshold || m_GoodThreshold != 0)
            {
                if (value <= m_BadThreshold)
                {
                    m_CountBelow++;
                }
                else if (value > m_GoodThreshold)
                {
                    m_CountAbove++;
                }
            }

            UpdateGraph(value);
            UpdateText();
        }

        public void UpdateGraph(float value)
        {
            if (! m_LineRenderer.gameObject.activeInHierarchy)
                return;

            m_Values.Add(value);

            // Limit the number of points
            if (m_Values.Count > m_GraphWidth)
            {
                m_Values.RemoveAt(0);
            }

            // Create points for the graph
            Vector3[] points = new Vector3[m_Values.Count];
            for (int i = 0; i < m_Values.Count; i++) 
            {
                float x = (float)i / (m_Values.Count - 1) * m_GraphWidth;
                float y = Normalize(m_Values[i]) * m_GraphHeight; // Scale y value for better visualization
                points[i] = new Vector3(x, y, 1000);
            }

            m_LineRenderer.positionCount = points.Length;
            m_LineRenderer.SetPositions(points);
        }

        public void UpdateText()
        {
            m_Text.text = m_ValueName + " : " + m_LastValue;

            m_AvgText.text = "Avg\n" + Mathf.Round(m_Avg);
            m_MinText.text = "Min\n" + m_MinRecordedValue;
            m_MaxText.text = "Max\n" + m_MaxRecordedValue;

            if (m_Count <= 0 || (m_BadThreshold == m_GoodThreshold && m_BadThreshold == 0))
                return;

            m_CountBelowText.text = "Below " + m_BadThreshold + "\n" + m_CountBelow + " (" + Mathf.Round(100 * m_CountBelow / m_Count) + "%)";
            m_CountAboveText.text = "Above " + m_GoodThreshold + "\n" + m_CountAbove + " (" + Mathf.Round(100 * m_CountAbove / m_Count) + "%)";
        }

        #endregion


        #region Helpers

        float Normalize(float value)
        {
            return (value - m_MinValue) / (m_MaxValue - m_MinValue);
        }

        #endregion
    }
}
