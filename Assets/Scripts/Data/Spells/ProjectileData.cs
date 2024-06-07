﻿using Enums;
using Game;
using Game.Spells;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Tools;
using UnityEngine;
using Data.GameManagement;

namespace Data
{
    [CreateAssetMenu(fileName = "Projectile", menuName = "Game/Spells/Projectile")]
    public class ProjectileData : SpellData
    {
        #region Members
        public override ESpellType SpellType => ESpellType.Projectile;

        [Header("Movement Data")]
        [Description("Type of path that the spell is taking")]
        public ESpellTrajectory Trajectory;
        [Description("Speed of the spell")]
        [SerializeField] float            m_Speed       = 0f;

        // ================================================================================================
        // Dependent Members
        /// <summary> Movement speed of the spell </summary>
        public float Speed => Settings.SpellSpeedFactor * m_Speed;
        public bool IsTrajectoryFromAbove => Trajectory == ESpellTrajectory.Hight || Trajectory == ESpellTrajectory.Diagonal;
        #endregion


        #region Inherited Spawning Members

        public override void Cast(ulong clientId, Vector3 target, Vector3 position = default, Quaternion rotation = default, bool recalculateTarget = true)
        {
            if (recalculateTarget)
                CalculateTarget(ref target, clientId);

            RecalculatePosition(ref position, target, clientId);
            base.Cast(clientId, target, position, rotation, false);
        }

        public override void SpellPreview(Controller controller, Transform parent = default, Vector3 offset = default)
        {
            offset = GetSpawnOffset(controller);
            switch (Trajectory)
            {
                case ESpellTrajectory.Curve:
                case ESpellTrajectory.Hight:
                    parent = ArenaManager.Instance.Arena.transform;
                    break;

                case ESpellTrajectory.Diagonal:
                case ESpellTrajectory.Straight:
                    break;

                default:
                    Debug.LogError($"Trajectory {Trajectory} not implemented");
                    break;
            }

            base.SpellPreview(controller, parent, offset);
        }

        /// <summary>
        /// Spawn the prefabs that are displayed when the spell is casted
        /// </summary>
        /// <returns></returns>
        /// 
        public override List<GameObject> SpawnOnCastPrefabs(Transform ownerTransform, Vector3 target)
        {
            List<GameObject> gameObjects = new List<GameObject>();
            // spawn on cast particles
            foreach (var prefab in OnCastPrefabs)
            {
                GameObject go = GameObject.Instantiate(prefab);

                float delay = Delay;
                if (m_Speed > 0)
                    delay += Math.Abs(target.x - ownerTransform.position.x) / Speed;

                var size = Size;
                if (OnHit.Count > 0 && OnHit[0].SpellType == ESpellType.Aoe)
                    size = OnHit[0].Size;

                Finder.FindComponent<OnCastAoe>(go).Initialize(target, size, delay);
                gameObjects.Add(go);
            }

            return gameObjects;
        }

        #endregion


        #region Postion & Target

        protected virtual void RecalculatePosition(ref Vector3 position, Vector3 target, ulong clientId)
        {
            Controller controller = GameManager.Instance.GetPlayer(clientId);

            if (IsTrajectoryFromAbove)
                position.y = 4;

            // bounds of the proc area
            (float Min, float Max) bounds = (0f, 0f);

            switch (Trajectory)
            {
                case ESpellTrajectory.Straight:
                case ESpellTrajectory.Diagonal:
                case ESpellTrajectory.Curve:
                    position += GetSpawnOffset(controller);
                    bounds = ArenaManager.GetAreaBounds(controller.Team, false);
                    break;

                case ESpellTrajectory.Hight:
                    position.x = target.x;
                    break;

                default:
                    ErrorHandler.Error($"Trajectory {Trajectory} not implemented");
                    break;
            }

            if (bounds != (0f, 0f))
            {
                var offset = 0.1f + Size / 2;
                position.x = Mathf.Clamp(position.x, bounds.Min + offset, bounds.Max - offset);
            }
        }

        /// <summary>
        /// Calculate offset of the spawn position depending on the trajectory
        /// </summary>
        /// <param name="trajectory"></param>
        /// <returns></returns>
        protected virtual Vector3 GetSpawnOffset(Controller controller)
        {
            int rotationFactor = controller.transform.rotation.y > 0 ? 1 : -1;

            switch (Trajectory)
            {
                case ESpellTrajectory.Straight:
                    return new Vector3(0, 0, 0);

                case ESpellTrajectory.Curve:
                case ESpellTrajectory.Diagonal:
                    return new Vector3(rotationFactor * 0.1f, 0.25f, 0);

                case ESpellTrajectory.Hight:
                    return new Vector3(rotationFactor * controller.SpellHandler.SpellSpawn.transform.position.x, 1f, 0); ;

                default:
                    ErrorHandler.Error($"Trajectory {Trajectory} not implemented");
                    return new Vector3(0, 0, 0);
            }
        }

        #endregion


        #region Info Display

        public override Dictionary<string, object> GetInfos()
        {
            var infoDict = base.GetInfos();

            if (m_Speed > 0)
                infoDict.Add("Speed", m_Speed);

            return infoDict;
        }

        #endregion
    }
}