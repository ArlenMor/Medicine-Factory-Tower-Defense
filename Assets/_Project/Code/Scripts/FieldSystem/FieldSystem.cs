using UnityEngine;

namespace _Project.Code.Scripts.BattleField
{
    public class FieldSystem : MonoBehaviour, IFieldSystem
    {
        [SerializeField] private FieldConfig _config;
        [SerializeField] private FieldHighlight _highlight;

        private FieldCell[,] _cells;
        public int Width => _config.Width;
        public int Height => _config.Height;
        public float CellSize => _config.CellSize;

        public void Initialize(FieldConfig config)
        {
            _config = config;
            _cells = new FieldCell[config.Width, config.Height];

            Vector3 origin = transform.position;
            Vector3 offset = new Vector3(
                (config.Width - 1) * config.CellSize * 0.5f,
                (config.Height - 1) * config.CellSize * 0.5f,
                0f);

            for (int x = 0; x < config.Width; x++)
            {
                for (int y = 0; y < config.Height; y++)
                {
                    Vector3 worldPos = origin - offset + new Vector3(
                        x * config.CellSize,
                        y * config.CellSize,
                        0f);

                    _cells[x, y] = new FieldCell(new Vector2Int(x, y), worldPos);
                }
            }

            _highlight.Initialize(this);
        }

        public void ShowHighlight() => _highlight?.SetActive(true);
        public void HideHighlight() => _highlight?.SetActive(false);

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < _config.Width && y >= 0 && y < _config.Height;
        }

        public bool IsOccupied(int x, int y)
        {
            return IsInBounds(x, y) && _cells[x, y].IsOccupied;
        }

        public FieldCell GetCell(int x, int y)
        {
            return IsInBounds(x, y) ? _cells[x, y] : null;
        }

        public bool TryPlace(int x, int y, IFieldPlaceable placeable)
        {
            if (!IsInBounds(x, y)) return false;

            var cell = _cells[x, y];
            if (!cell.TryPlace(placeable)) return false;

            placeable.transform.position = cell.WorldPosition;
            return true;
        }

        public bool CanPlaceMulti(int originX, int originY, int width, int height)
        {
            for (int dx = 0; dx < width; dx++)
            {
                for (int dy = 0; dy < height; dy++)
                {
                    int cx = originX + dx;
                    int cy = originY + dy;
                    if (!IsInBounds(cx, cy) || _cells[cx, cy].IsOccupied)
                        return false;
                }
            }
            return true;
        }

        public bool TryPlaceMulti(int originX, int originY, int width, int height, IFieldPlaceable placeable, Vector2 pivotOffset)
        {
            if (!CanPlaceMulti(originX, originY, width, height)) return false;

            for (int dx = 0; dx < width; dx++)
            {
                for (int dy = 0; dy < height; dy++)
                {
                    _cells[originX + dx, originY + dy].TryPlace(placeable);
                }
            }

            placeable.transform.position = GetMultiCellWorldPosition(originX, originY, width, height, pivotOffset);
            return true;
        }

        public Vector3 GetMultiCellWorldPosition(int originX, int originY, int width, int height, Vector2 pivotOffset)
        {
            Vector3 bottomLeft = _cells[originX, originY].WorldPosition;
            Vector3 center = bottomLeft + new Vector3(
                (width - 1) * _config.CellSize * 0.5f,
                (height - 1) * _config.CellSize * 0.5f,
                0f);
            Vector3 pivot = center - new Vector3(
                pivotOffset.x * _config.CellSize,
                pivotOffset.y * _config.CellSize,
                0f);
            return pivot;
        }

        public void Remove(int x, int y)
        {
            if (IsInBounds(x, y))
                _cells[x, y].Clear();
        }

        public void RemoveMulti(int originX, int originY, int width, int height)
        {
            for (int dx = 0; dx < width; dx++)
            {
                for (int dy = 0; dy < height; dy++)
                {
                    int cx = originX + dx;
                    int cy = originY + dy;
                    if (IsInBounds(cx, cy))
                        _cells[cx, cy].Clear();
                }
            }
        }

        public Vector3 GridToWorld(int x, int y)
        {
            return IsInBounds(x, y) ? _cells[x, y].WorldPosition : Vector3.zero;
        }

        public bool WorldToGrid(Vector3 worldPos, out int x, out int y)
        {
            Vector3 origin = transform.position;
            Vector3 offset = new Vector3(
                (_config.Width - 1) * _config.CellSize * 0.5f,
                (_config.Height - 1) * _config.CellSize * 0.5f,
                0f);

            Vector3 local = worldPos - origin + offset;
            x = Mathf.RoundToInt(local.x / _config.CellSize);
            y = Mathf.RoundToInt(local.y / _config.CellSize);

            return IsInBounds(x, y);
        }

        private void OnDrawGizmos()
        {
            DrawEditModeGizmos();
            DrawRuntimeGizmos();
        }

        private void DrawEditModeGizmos()
        {
            if (_config == null) return;
            if (_cells != null) return;

            Vector3 origin = transform.position;
            Vector3 offset = new Vector3(
                (_config.Width - 1) * _config.CellSize * 0.5f,
                (_config.Height - 1) * _config.CellSize * 0.5f,
                0f);

            // Grid cells
            Gizmos.color = new Color(0.3f, 0.8f, 0.3f, 0.3f);
            for (int x = 0; x < _config.Width; x++)
            {
                for (int y = 0; y < _config.Height; y++)
                {
                    Vector3 pos = origin - offset + new Vector3(
                        x * _config.CellSize,
                        y * _config.CellSize,
                        0f);

                    Gizmos.DrawWireCube(pos, Vector3.one * _config.CellSize * 0.95f);
                }
            }

            // Outer border
            Gizmos.color = new Color(1f, 1f, 0f, 0.6f);
            Vector3 totalSize = new Vector3(
                _config.Width * _config.CellSize,
                _config.Height * _config.CellSize,
                0f);
            Gizmos.DrawWireCube(origin, totalSize);
        }

        private void DrawRuntimeGizmos()
        {
            if (_cells == null || _config == null) return;

            for (int x = 0; x < _config.Width; x++)
            {
                for (int y = 0; y < _config.Height; y++)
                {
                    var cell = _cells[x, y];
                    bool occupied = cell.IsOccupied;

                    // Filled cell background
                    Gizmos.color = occupied
                        ? new Color(0.9f, 0.2f, 0.2f, 0.15f)
                        : new Color(0.3f, 0.8f, 0.3f, 0.1f);
                    Gizmos.DrawCube(cell.WorldPosition, Vector3.one * _config.CellSize * 0.9f);

                    // Cell border
                    Gizmos.color = occupied
                        ? new Color(0.9f, 0.2f, 0.2f, 0.6f)
                        : new Color(0.3f, 0.8f, 0.3f, 0.4f);
                    Gizmos.DrawWireCube(cell.WorldPosition, Vector3.one * _config.CellSize * 0.95f);

                    // Occupied marker cross
                    if (occupied)
                    {
                        Gizmos.color = new Color(1f, 0.1f, 0.1f, 0.7f);
                        float half = _config.CellSize * 0.35f;
                        Vector3 p = cell.WorldPosition;
                        Gizmos.DrawLine(p + new Vector3(-half, -half, 0), p + new Vector3(half, half, 0));
                        Gizmos.DrawLine(p + new Vector3(-half, half, 0), p + new Vector3(half, -half, 0));
                    }
                }
            }

            // Outer border
            Gizmos.color = new Color(1f, 1f, 0f, 0.8f);
            Vector3 totalSize = new Vector3(
                _config.Width * _config.CellSize,
                _config.Height * _config.CellSize,
                0f);
            Gizmos.DrawWireCube(transform.position, totalSize);
        }
    }
}