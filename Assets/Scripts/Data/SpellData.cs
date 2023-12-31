using Game;
using Game.Spells;
using System.ComponentModel;
using Tools;
using UnityEngine;
using Enums;
using Game.Character;
using Unity.Netcode;

namespace Data
{
    [CreateAssetMenu(fileName = "Spell", menuName = "Game/Spell")]
    public class SpellData : ScriptableObject
    {
        [Header("Identity")]
        [Description("Id of the Spell")]
        public      ESpells             Name;
        [Description("Icon of the Spell")]
        public      Sprite              Image;
        [Description("Border of the Spell Card")]
        public Sprite Border;

        [Header("Prefabs")]
        [Description("Prefab of the spell that will be instantiated when the spell is cast")]
        public      GameObject          Prefab;
        [Description("Preview of the spell target on the ground displayed before the cast of the spell")]
        public      GameObject          Preview;
        [Description("Particles displayed during the animation")]
        public      GameObject          AnimationParticles;

        [Header("Stats")]
        [Description("Type of spell")]
        public      ESpellType          Type;
        [Description("Type of path that the spell is taking")]
        public      ESpellTrajectory    Trajectory;
        [Description("Damage of the spell")]
        public      int                 Damage;
        [Description("Speed of the spell")]
        public      float               Speed;
        [Description("Max distance of the spell")]
        public      float               Distance = -1f;

        [Header("Animation & Cooldowns")]
        [Description("Time for the animation to take from start to begin (in seconds)")]
        public      float               AnimationTimer;
        [Description("Percentage of time during the animation when the spell will be casted")]
        public      float               CastAt = 1f;
        [Description("Cooldown to be able to re-use that ability")]
        public      float               Cooldown;


        #region Public Manipualtors

        /// <summary>
        /// Cast a the spell by instantiating the prefab, initializing it and spawning in the network
        /// </summary>
        /// <param name="owner">            caster of the spell                         </param>
        /// <param name="targetPosition">   position targetted                          </param>
        /// <param name="position">         position where to spawn the spell prefab    </param>
        /// <param name="rotation">         rotation of the prefab                      </param>
        /// <returns></returns>
        public GameObject Cast(ulong clientId, Vector3 targetPosition, Vector3 position = default, Quaternion rotation = default)
        {
            // instantiate the spell 
            var spellGO = GameObject.Instantiate(Prefab, position, rotation);

            // spawn in network
            Finder.FindComponent<NetworkObject>(spellGO).SpawnWithOwnership(clientId);

            // initialize the spell
            var spell = Finder.FindComponent<Spell>(spellGO);
            spell.InitializeClientRPC(targetPosition, Name);

            return spellGO;
        }

        /// <summary>
        /// Display the preview of the spell on the ground where the player is aiming
        /// </summary>
        public void SpellPreview(Transform targettableArea, Transform parent, Vector3 offset = default)
        {
            switch (Trajectory)
            {
                case ESpellTrajectory.Curve:
                case ESpellTrajectory.Hight:
                    parent = GameManager.Instance.Arena.transform;
                    break;

                case ESpellTrajectory.Straight:
                    break;

                default:
                    Debug.LogError($"Trajectory {Trajectory} not implemented");
                    break;
            }

            // instantiate the gameobject of the preview
            var preview = GameObject.Instantiate(Preview, parent);
            // apply the offset
            preview.transform.localPosition += offset;

            // get the component of the preview and initialize it
            var component = Finder.FindComponent<SpellPreview>(preview);
            component.Initialize(targettableArea, Distance);
        }

        #endregion
    }
}