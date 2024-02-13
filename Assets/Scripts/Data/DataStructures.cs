using Enums;
using MyBox;
using System;
using System.Collections.Generic;
using Tools;
using UnityEngine;


namespace Data
{
    /// <summary>
    /// Structure allowing to define a state effect in the SpellData inspector
    /// </summary>
    [Serializable]
    public struct SStateEffectData
    {
        public EStateEffect     StateEffect;
        public int              Stacks;
        public float            Duration;
        public float            SpeedBonus;

        public readonly bool OverrideDuration    => Duration > 0;
        public readonly bool OverrideSpeedBonus  => SpeedBonus != 0;

        public SStateEffectData(EStateEffect stateEffect, int stacks = 1, float duration = -1f, float speedBonus = 0f)
        {
            StateEffect = stateEffect;
            Stacks      = stacks;
            Duration    = duration;
            SpeedBonus  = speedBonus;
        }
    }

    [Serializable]
    public struct SPrefabSpawn
    {
        public GameObject       Prefab;
        public ESpawnLocation   SpawnLocation;
        public bool             IsFollowing;

        public SPrefabSpawn(GameObject prefab, ESpawnLocation spawnLocation, bool isFollowing)
        {
            Prefab          = prefab;
            SpawnLocation   = spawnLocation;
            IsFollowing     = isFollowing;
        }

        public readonly GameObject Spawn(Controller controller, float duration = 1f)
        {
            Transform parent = null;
            Vector3 pos = Vector3.zero;

            switch (SpawnLocation)
            {
                case ESpawnLocation.Caster:
                    parent = IsFollowing ? controller.transform : null;
                    pos = controller.transform.position;
                    break;

                case ESpawnLocation.CasterFeets:
                    parent = IsFollowing ? controller.transform : null;
                    pos = controller.transform.position;
                    pos.y = 0;
                    break;

                case ESpawnLocation.CasterSpellSpawn:
                    parent = IsFollowing ? controller.SpellHandler.SpellSpawn : null;
                    break;

                case ESpawnLocation.Target:
                    pos = controller.SpellHandler.TargetPos;
                    break;

                case ESpawnLocation.TargetFeets:
                    pos = controller.SpellHandler.TargetPos;
                    pos.y = 0;
                    break;

                default:
                    Debug.LogError("SPrefabSpawn::Spawn() - Unknown spawn location " + SpawnLocation);
                    return null;
            }

            //AdjustDuration(Prefab, duration);
            GameObject go = GameObject.Instantiate(Prefab, pos, Quaternion.identity, parent);

            return go;
        }

        readonly void AdjustDuration(GameObject go, float duration)
        {
            if (go == null)
                return;

            List<ParticleSystem> particleSystems = Finder.FindComponents<ParticleSystem>(go);

            foreach (ParticleSystem ps in particleSystems)
            {
                ps.Stop();

                var main = ps.main;
                main.duration = duration;
                
                ps.Play();
            }
        }
    }
}