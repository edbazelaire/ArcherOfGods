using Tools.Debugs.Monitoring;
using UnityEngine;

namespace Tools.Debugs
{
    public class PerformanceMonitor : MObject
    {
        #region Members

        private PerfMonitor m_FpsMonitor;
        private PerfMonitor m_MemoryMonitor;

        private float deltaTime = 0.0f;

        #endregion


        #region Update

        protected override void Start()
        {
            base.Start();

            m_FpsMonitor = Finder.FindComponent<PerfMonitor>(gameObject, "FpsMonitor");
            m_MemoryMonitor = Finder.FindComponent<PerfMonitor>(gameObject, "MemoryMonitor");

            CoroutineManager.DelayMethod(() => m_FpsMonitor.Initialize("FPS", 60, 120, 0, 300));
            CoroutineManager.DelayMethod(() => m_MemoryMonitor.Initialize("Mem.Usg", 0, 0, 0, 1500));
            CoroutineManager.DelayMethod(() => m_Initialized = true);

        }

        void Update()
        {
            if (! m_Initialized)
                return;

            // Update the frame rate
            float fps = Mathf.Round(1.0f / Time.deltaTime);
            m_FpsMonitor.AddValue(fps);

            // Update the memory usage
            long totalMemory = System.GC.GetTotalMemory(false);
            m_MemoryMonitor.AddValue(totalMemory / 1024f / 1024f); // Convert bytes to MB
            m_MemoryMonitor.Text.text = "Mem.Usg \n" + FormatBytes(totalMemory);
        }

        #endregion


        #region Helpers

        string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return string.Format("{0:0.##} {1}", len, sizes[order]);
        }

        #endregion
    }
}