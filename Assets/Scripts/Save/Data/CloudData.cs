using Assets;
using Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tools;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;

namespace Save
{
    public class CloudData
    {
        public static CloudData Instance => Main.CloudSaveManager.GetCloudData(typeof(CloudData)) as CloudData;

        protected virtual Dictionary<string, object> m_Data { get; set; }

        /// <summary> list of remaining values to load</summary>
        List<string> m_KeysToLoad;

        public Dictionary<string, object> Data => m_Data;
        public bool LoadingCompleted => m_KeysToLoad != null && m_KeysToLoad.Count == 0;

        public static Action<string> CloudDataLoadedEvent;

        #region Init & End 

        public CloudData() 
        {
            CloudDataLoadedEvent += OnCloudDataLoaded;
            m_KeysToLoad = m_Data.Keys.ToList();
            Load();
        }

        #endregion


        #region Loading & Saving

        public virtual void Load()
        {
            foreach (var key in m_Data.Keys)
            {
                // if value exists in the cloud get it, other wise keep default
                LoadValue(key);
            }
        }

        protected async virtual void LoadValue(string key)
        {
            var cloudData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });
            if (!cloudData.TryGetValue(key, out var item))
            {
                CloudDataLoadedEvent?.Invoke(key);
                return;
            }

            var value = Convert(item);
            if (value == null)
                return;

            SetData(key, value);
            CloudDataLoadedEvent?.Invoke(key);
        }

        public virtual void Save()
        {
            foreach (var item in m_Data)
            {
                SaveValue(item.Key);
            }
        }

        public virtual async void SaveValue(string key)
        {
            var playerData = new Dictionary<string, object> { { key, m_Data[key] } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(playerData);
        }

        #endregion


        #region Helpers

        public virtual void SetData(string key, object value)
        {
            m_Data[key] = value;
            SaveValue(key);
        }

        protected virtual object Convert(Item item)
        {
            var expectedType = m_Data[item.Key].GetType().Name;
            switch (expectedType)
            {
                case "String":
                    return item.Value.GetAs<string>();

                case "Int16":
                case "Int32":
                case "Int64":
                    return item.Value.GetAs<int>();

                case "Double":
                    return item.Value.GetAs<double>();

                case "Float":
                    return item.Value.GetAs<float>();

                case "ECharacter":
                    return item.Value.GetAs<ECharacter>();
                case "ESpell":
                    return item.Value.GetAs<ESpell>();

                default:
                    ErrorHandler.Error("Unhandled type : " + expectedType);
                    return null;
            }
        }

        public async Task<bool> KeyExists(string key)
        {
            var keys = await CloudSaveService.Instance.Data.Player.ListAllKeysAsync();
            for (int i = 0; i < keys.Count; i++)
            {
                if (key == keys[i].Key)
                    return true;
            }

            return false;
        }

        #endregion


        #region Listeners

        protected virtual void OnCloudDataLoaded(string key) 
        { 
            if (m_KeysToLoad.Contains(key))
                m_KeysToLoad.Remove(key);
        }

        #endregion
    }
}