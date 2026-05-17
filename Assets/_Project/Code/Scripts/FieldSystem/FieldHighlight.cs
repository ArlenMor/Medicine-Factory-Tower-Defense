using UnityEngine;

namespace _Project.Code.Scripts.BattleField
{
    public class FieldHighlight : MonoBehaviour
    {
        private FieldSystem _fieldSystem;
        private SpriteRenderer[,] _cellRenderers;
        private Sprite _whiteSprite;
        [SerializeField] private GameObject _highlightRoot;

        [SerializeField] private Color FreeCellColor = new Color(0.3f, 0.8f, 0.3f, 0.15f);
        private static readonly Color OccupiedCellColor = new Color(0.9f, 0.2f, 0.2f, 0.2f);
        private static readonly Color FreeBorderColor = new Color(0.3f, 0.8f, 0.3f, 0.5f);
        private static readonly Color OccupiedBorderColor = new Color(0.9f, 0.2f, 0.2f, 0.6f);
        private static readonly Color OuterBorderColor = new Color(1f, 1f, 0f, 0.6f);

        public void Initialize(FieldSystem fieldSystem)
        {
            _fieldSystem = fieldSystem;
            CreateSprite();
            CreateCells();
            CreateOuterBorder();
            SetActive(false);
        }

        public void SetActive(bool active)
        {
            if (_highlightRoot == null) return;
            _highlightRoot.SetActive(active);
            if (active) RefreshColors();
        }

        private void RefreshColors()
        {
            for (int x = 0; x < _fieldSystem.Width; x++)
            {
                for (int y = 0; y < _fieldSystem.Height; y++)
                {
                    bool occupied = _fieldSystem.IsOccupied(x, y);
                    var fill = _cellRenderers[x, y];
                    fill.color = occupied ? OccupiedCellColor : FreeCellColor;

                    var border = fill.transform.GetChild(0).GetComponent<SpriteRenderer>();
                    border.color = occupied ? OccupiedBorderColor : FreeBorderColor;
                }
            }
        }

        private void CreateSprite()
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            _whiteSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
        }

        private void CreateCells()
        {
            if (_highlightRoot == null)
            {
                _highlightRoot = new GameObject("FieldHighlight");
                _highlightRoot.transform.SetParent(transform);
            }

            float cellSize = _fieldSystem.CellSize;
            _cellRenderers = new SpriteRenderer[_fieldSystem.Width, _fieldSystem.Height];

            for (int x = 0; x < _fieldSystem.Width; x++)
            {
                for (int y = 0; y < _fieldSystem.Height; y++)
                {
                    var cell = _fieldSystem.GetCell(x, y);

                    // Fill
                    var go = new GameObject($"Cell_{x}_{y}");
                    go.transform.SetParent(_highlightRoot.transform);
                    go.transform.position = cell.WorldPosition;
                    go.transform.localScale = Vector3.one * cellSize * 0.9f;

                    var fillSr = go.AddComponent<SpriteRenderer>();
                    fillSr.sprite = _whiteSprite;
                    fillSr.color = FreeCellColor;
                    fillSr.sortingOrder = 0;

                    // Border (slightly larger child)
                    var borderGo = new GameObject("Border");
                    borderGo.transform.SetParent(go.transform);
                    borderGo.transform.localPosition = Vector3.zero;
                    borderGo.transform.localScale = Vector3.one * 1.06f;

                    var borderSr = borderGo.AddComponent<SpriteRenderer>();
                    borderSr.sprite = _whiteSprite;
                    borderSr.color = FreeBorderColor;
                    borderSr.sortingOrder = -11;

                    _cellRenderers[x, y] = fillSr;
                }
            }
        }

        private void CreateOuterBorder()
        {
            float cellSize = _fieldSystem.CellSize;
            float totalW = _fieldSystem.Width * cellSize;
            float totalH = _fieldSystem.Height * cellSize;
            float thickness = cellSize * 0.04f;

            Vector3 center = _fieldSystem.transform.position;

            CreateBorderEdge("Border_Top", center + new Vector3(0, totalH * 0.5f, 0),
                new Vector3(totalW + thickness, thickness, 1f));
            CreateBorderEdge("Border_Bottom", center - new Vector3(0, totalH * 0.5f, 0),
                new Vector3(totalW + thickness, thickness, 1f));
            CreateBorderEdge("Border_Left", center - new Vector3(totalW * 0.5f, 0, 0),
                new Vector3(thickness, totalH + thickness, 1f));
            CreateBorderEdge("Border_Right", center + new Vector3(totalW * 0.5f, 0, 0),
                new Vector3(thickness, totalH + thickness, 1f));
        }

        private void CreateBorderEdge(string name, Vector3 position, Vector3 scale)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_highlightRoot.transform);
            go.transform.position = position;
            go.transform.localScale = scale;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _whiteSprite;
            sr.color = OuterBorderColor;
            sr.sortingOrder = -9;
        }
    }
}
