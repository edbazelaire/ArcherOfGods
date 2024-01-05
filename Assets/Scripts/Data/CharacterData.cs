using Enums;
using Game;
using Game.Managers;
using System.Collections.Generic;
using System.Xml.Linq;
using Tools;
using Unity.Netcode;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Character", menuName = "Game/Character")]
    public class CharacterData : ScriptableObject
    {
        public ECharacter       Name;
        public Sprite           Image;
        public GameObject       CharacterPreview;
        public List<ESpell>     Spells;

        public GameObject InstantiateCharacterPreview(GameObject parent)
        {
            var go = GameObject.Instantiate(CharacterPreview, parent.transform);
            return go;
        }
    }
}