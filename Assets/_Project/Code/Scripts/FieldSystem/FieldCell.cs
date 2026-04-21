using UnityEngine;

namespace _Project.Code.Scripts.BattleField
{
    public class FieldCell
    {
        public Vector2Int GridPosition { get; }
        public Vector3 WorldPosition { get; }
        public bool IsOccupied => Placeable != null && !Placeable.IsDead;
        public IFieldPlaceable Placeable { get; private set; }

        public FieldCell(Vector2Int gridPosition, Vector3 worldPosition)
        {
            GridPosition = gridPosition;
            WorldPosition = worldPosition;
        }

        public bool TryPlace(IFieldPlaceable placeable)
        {
            if (IsOccupied) return false;

            Placeable = placeable;
            placeable.OnPlaceableDestroyed += HandlePlaceableDestroyed;
            return true;
        }

        public void Clear()
        {
            if (Placeable != null)
            {
                Placeable.OnPlaceableDestroyed -= HandlePlaceableDestroyed;
                Placeable = null;
            }
        }

        private void HandlePlaceableDestroyed(IFieldPlaceable placeable)
        {
            if (Placeable == placeable)
                Clear();
        }
    }
}
