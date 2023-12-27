using System.Collections;
using System.Collections.Generic;
using Tools;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Managers
{
    public enum ECharacter
    {
        GreenArcher,

        Count
    }

    public class CharacterLoader: MonoBehaviour
    {
        #region Members

        public List<GameObject> Characters;

        static CharacterLoader s_Instance;

        #endregion


        #region Initialization

        void Initialize()
        {
            if (Characters.Count != (int)ECharacter.Count)
            {
                ErrorHandler.FatalError("CharacterLoader : Characters list is not complete");
                return;
            }

            DontDestroyOnLoad(this);
        }

        #endregion


        #region Public Manipulators

        public GameObject InstantiateChar(ECharacter character, Transform spawn)
        {
            return GameObject.Instantiate(Characters[(int)character], spawn);
        }

        #endregion


        #region Dependent Members

        public static CharacterLoader Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = FindObjectOfType<CharacterLoader>();
                    s_Instance.Initialize();
                }
                return s_Instance;
            }
        }

        #endregion
    }

}
