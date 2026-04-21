using UnityEngine;

namespace _Project.Code.Scripts.BattleField
{
    [CreateAssetMenu(fileName = "FieldConfig", menuName = "GameConfig/FieldConfig")]
    public class FieldConfig : ScriptableObject
    {
        [Tooltip("Количество клеток по горизонтали")]
        public int Width = 5;

        [Tooltip("Количество клеток по вертикали")]
        public int Height = 3;

        [Tooltip("Размер одной клетки в мировых единицах")]
        public float CellSize = 1f;
    }
}
