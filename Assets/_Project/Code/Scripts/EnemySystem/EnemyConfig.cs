using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Code.Scripts.EnemySystem
{
    [Serializable]
    public struct EnemyStats
    {
        public EnemyType Type;
        public float HP;
        public float Speed;
        public float CenterDamage;
        public float BarricadeDamage;
        public float AttackInterval;
        public GameObject Prefab;
    }

    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "GameConfig/EnemyConfig")]
    public class EnemyConfig : ScriptableObject
    {
        public List<EnemyStats> Enemies;

        private Dictionary<EnemyType, EnemyStats> _lookup;

        public EnemyStats GetStats(EnemyType type)
        {
            if (_lookup == null)
            {
                _lookup = new Dictionary<EnemyType, EnemyStats>();
                foreach (var e in Enemies)
                    _lookup[e.Type] = e;
            }

            return _lookup[type];
        }

        /// <summary>
        /// Заполняет конфиг значениями из GDD.
        /// ПКМ по ассету → Fill Default Stats.
        /// </summary>
        [ContextMenu("Fill Default Stats")]
        private void FillDefaultStats()
        {
            Enemies = new List<EnemyStats>
            {
                new()
                {
                    Type = EnemyType.Scout,
                    HP = 18f,
                    Speed = 1.9f,
                    CenterDamage = 1f,
                    BarricadeDamage = 2f,
                    AttackInterval = 0.7f
                },
                new()
                {
                    Type = EnemyType.Gnawer,
                    HP = 42f,
                    Speed = 1.25f,
                    CenterDamage = 2f,
                    BarricadeDamage = 5f,
                    AttackInterval = 0.9f
                },
                new()
                {
                    Type = EnemyType.Tank,
                    HP = 105f,
                    Speed = 0.78f,
                    CenterDamage = 5f,
                    BarricadeDamage = 11f,
                    AttackInterval = 1.15f
                }
            };

            _lookup = null;
        }
    }
}
