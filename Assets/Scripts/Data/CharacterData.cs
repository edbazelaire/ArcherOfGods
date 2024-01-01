using Enums;
using Game;
using Game.Managers;
using System.Collections.Generic;
using System.Xml.Linq;
using Tools;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Character", menuName = "Game/Character")]
    public class CharacterData : ScriptableObject
    {
        public ECharacter       Name;
        public Sprite           Image;
        public GameObject       Prefab;
        public List<ESpells>    Spells;

        // TODO : remove if not used (= player instantiated by NetworkManager)
        //public GameObject Instantiate(int id, int team, bool isPlayer, Transform spawn, Vector3 position = default, Quaternion rotation = default)
        //{
        //    return;
        //}
    }
}