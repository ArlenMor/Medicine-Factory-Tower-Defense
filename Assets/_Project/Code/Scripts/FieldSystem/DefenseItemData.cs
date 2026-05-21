using System;
using UnityEngine;

namespace _Project.Code.Scripts.BattleField
{
    [Serializable]
    public struct DefenseItemData
    {
        public DefenseType Type;
        public GameObject Prefab;
        public Sprite Icon;
        public int CreditCost;

        [Tooltip("Размер объекта в клетках по X")]
        public int Width;

        [Tooltip("Размер объекта в клетках по Y")]
        public int Height;

        [Tooltip("Смещение пивота в клетках от левого-нижнего угла объекта")]
        public Vector2 PivotOffset;

        [Header("Combat")]
        public float MaxHp;

        [Header("Turret Only")]
        public float AttackRadius;
        public float FireRate;
        public float Damage;
        public float BulletSpeed;
        public float RadiusFadeDuration;
        public float RadiusMaxAlpha;

        public int SafeWidth => Mathf.Max(1, Width);
        public int SafeHeight => Mathf.Max(1, Height);
    }
}
