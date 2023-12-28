using Game;
using Game.Managers;
using Game.Spells;
using Tools;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Spell", menuName = "Game/Spell")]
    public class SpellData : ScriptableObject
    {
        public ESpells Name;
        public Sprite Image;
        public GameObject Prefab;

        public GameObject Cast(Controller owner, Transform parent, Vector3 position = default, Quaternion rotation = default)
        {
            var spellGO = GameObject.Instantiate(Prefab, position, rotation, parent);
            var spell = spellGO.GetComponent<Arrow>();
            
            if (!Checker.NotNull(spell))
                return null;
            
            spell.Initialize(owner);

            return spellGO;
        }
    }
}