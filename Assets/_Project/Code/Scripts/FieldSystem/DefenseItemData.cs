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

        public int SafeWidth => Mathf.Max(1, Width);
        public int SafeHeight => Mathf.Max(1, Height);
    }
}
