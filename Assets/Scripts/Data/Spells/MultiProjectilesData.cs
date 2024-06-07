using Enums;
using Game;
using Game.Spells;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Tools;
using UnityEngine;
using Assets;
using static UnityEngine.GraphicsBuffer;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using Data.GameManagement;

namespace Data
{
    [CreateAssetMenu(fileName = "MultiProjectiles", menuName = "Game/Spells/MultiProjectiles")]
    public class MultiProjectilesData : ProjectileData
    {
        #region Members

        public override ESpellType SpellType => ESpellType.Projectile;

        [Header("Projectile")]
        [Description("Type of path that the spell is taking")]
        public ProjectileData ProjectileData;

        [Header("Multiple Projectiles Data")]
        [Description("Type of multiple projectile launch")]
        public EMultiProjectileType MultiProjectileType;
        [Description("Number of projectiles launched")]
        public int NProjectiles = 1;
        [Description("Size of the projectile zone")]
        [SerializeField] protected float m_ProjectileZoneSize = 0f;
        [Description("Delay between each projectile cast")]
        public float DelayBetweenLaunches = 0f;
        [Description("Number of waves")]
        public int NWaves = 1;
        [Description("Delay between each waves")]
        public float DelayBetweenWaves = 0f;

        // ============================================================================================
        // Private Members
        bool m_IsCancelled;

        // ============================================================================================
        // Dependent Members
        /// <summary> Is the spell blocking movement and cast until the end of the multicast ? </summary>
        protected bool m_IsBlocking => Trajectory == ESpellTrajectory.Curve || Trajectory == ESpellTrajectory.Straight;

        public float ProjectileZoneSize => m_ProjectileZoneSize * Settings.SpellSizeFactor;

        #endregion


        #region Casting & Spawning

        public override void Cast(ulong clientId, Vector3 target, Vector3 position = default, Quaternion rotation = default, bool recalculateTarget = true)
        {
            // recalculate target depending on spell type
            if (recalculateTarget)
                CalculateTarget(ref target, clientId);

            if (NProjectiles < 1)
            {
                ErrorHandler.Error("Bad config for spell " + name + " : NProjectiles (" + NProjectiles + ")  < 1");
                return;
            }

            Main.Instance.StartCoroutine(CastMultipleProjectiles(clientId, target, position, rotation));
        }

        public IEnumerator CastMultipleProjectiles(ulong clientId, Vector3 target, Vector3 position = default, Quaternion rotation = default)
        {
            // block movement and cast until the end
            Controller controller = GameManager.Instance.GetPlayer(clientId);
            if (m_IsBlocking)
            {
                controller.SpellHandler.ForceBlockCast(true);
                controller.Movement.ForceBlockMovement(true);
            }

            // save spellTarget to avoid 
            var targetType = SpellTarget;

            // change spell target to None (to avoid projectile re-calculating target)
            SpellTarget = ESpellTarget.None;

            for (int i = 0; i < NWaves; i++)
            {
                yield return CastWave(clientId, target, position, rotation);

                if (i == NWaves - 1 || m_IsCancelled)
                    break;

                controller.AnimationHandler.CastAnimation(Animation, DelayBetweenWaves);

                var delay = DelayBetweenWaves;
                while (delay > 0)
                {
                    if (m_IsCancelled || controller.SpellHandler.HasStateBlockingCast())
                    {
                        m_IsCancelled = true;
                        break;
                    }

                    delay -= Time.deltaTime;
                    yield return null;
                }

                controller.AnimationHandler.CancelCastAnimation();

                if (m_IsCancelled)
                {
                    m_IsCancelled = true;
                    break;
                }
            }

            // reset spell target before leaving
            SpellTarget = targetType;

            // remove blockers
            if (m_IsBlocking)
            {
                controller.SpellHandler.ForceBlockCast(false);
                controller.Movement.ForceBlockMovement(false);
            }
        }

        public IEnumerator CastWave(ulong clientId, Vector3 target, Vector3 position = default, Quaternion rotation = default)
        {
            // block movement and cast until the end
            Controller controller = GameManager.Instance.GetPlayer(clientId);

            // change spell target to None (to avoid projectile re-calculating target)
            SpellTarget = ESpellTarget.None;

            for (int i = 0; i < NProjectiles; i++)
            {
                // if specific projectile data are provided : use theme
                if (ProjectileData != null)
                    ProjectileData.Cast(clientId, CalculateMultiProjectileTarget(target, i, controller.Team), position, rotation, false);
                // otherwise use config of the file
                else
                    base.Cast(clientId, CalculateMultiProjectileTarget(target, i, controller.Team), position, rotation, false);
                    
                var delay = DelayBetweenLaunches;

                while (delay > 0)
                {
                    if (controller.SpellHandler.HasStateBlockingCast())
                    {
                        m_IsCancelled = true;
                        yield break;
                    }

                    delay -= Time.deltaTime;
                    yield return null;
                }
            }
        }

        #endregion


        #region Postion & Target

        public override void CalculateTarget(ref Vector3 target, ulong clientId)
        {
            base.CalculateTarget(ref target, clientId);

            target.x -= ProjectileZoneSize / 2;

            ClampTargetX(ref target, clientId);
        }

        /// <summary>
        /// Calculate position of the projectile number "i" depending on his type
        /// </summary>
        /// <param name="target">   base target of the projectile   </param>
        /// <param name="i">        projectile number               </param>
        /// <returns></returns>
        protected Vector3 CalculateMultiProjectileTarget(Vector3 target, int i, int team)
        {
            // if projectile size < 0 : use all the size of 
            var zoneSize = ProjectileZoneSize >= 0f ? ProjectileZoneSize : ArenaManager.Instance.TargettableAreaSize;

            switch (MultiProjectileType)
            {
                case EMultiProjectileType.None:
                    break;

                case (EMultiProjectileType.Line):
                    target.x += i * (team == 0 ? 1 : -1) * zoneSize / (NProjectiles - 1);
                    break;

                case (EMultiProjectileType.Random):
                    target.x += UnityEngine.Random.Range(-zoneSize / 2, zoneSize / 2);
                    break;

                default:
                    ErrorHandler.Warning("Unhandled MultiProjectileType : " + MultiProjectileType);
                    break;
            }

            (float min, float max) = ArenaManager.GetAreaBounds(team, SpellTarget == ESpellTarget.None || IsEnemyTarget);
            target.x = Mathf.Clamp(target.x, min, max);

            return target;
        }

        #endregion


        #region Level

        protected override void SetLevel(int level)
        {
            if (ProjectileData != null)
                ProjectileData = (ProjectileData)ProjectileData.Clone(level);

            base.SetLevel(level);
        }

        #endregion


        #region Info Display

        public override Dictionary<string, object> GetInfos()
        {
            var infoDict = base.GetInfos();
            if (ProjectileData != null)
            {
                foreach (var item in ProjectileData.GetInfos())
                    infoDict[item.Key] = item.Value;
            }

            infoDict["Projectiles"] = NProjectiles;

            if (NWaves > 1)
            {
                infoDict["Waves"] = NWaves;
            }

            return infoDict;
        }

        #endregion
    }
}