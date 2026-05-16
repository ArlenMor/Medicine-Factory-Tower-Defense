using TMPro;
using UnityEngine;

namespace _Project.Code.Scripts.UI
{
    /// <summary>
    /// Переиспользуемая полоска здоровья на основе SpriteRenderer.
    /// Добавляется как компонент на объект, создаёт визуал автоматически при Initialize.
    /// </summary>
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Vector2 _size = new Vector2(1f, 0.15f);
        [SerializeField] private Vector3 _offset = new Vector3(0f, 0.8f, 0f);
        [SerializeField] private Color _backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        [SerializeField] private Color _fillColor = new Color(0.2f, 0.8f, 0.2f, 1f);
        [SerializeField] private int _sortingOrder = 100;
        [SerializeField] private float _fontSize = 3f;
        [SerializeField] private bool _flipText;
        [SerializeField] [Range(0f, 0.5f)] private float _cornerRadius = 0.3f;

        private Transform _fillTransform;
        private TextMeshPro _hpText;
        private float _maxHp;
        private float _currentHp;
        private Sprite _pixel;

        public void Initialize(float maxHp)
        {
            _maxHp = maxHp;
            CreateBar();
            SetHealth(maxHp);
        }

        public void SetHealth(float currentHp)
        {
            if (_fillTransform == null) return;

            _currentHp = Mathf.Max(0f, currentHp);
            float ratio = Mathf.Clamp01(_currentHp / _maxHp);
            _fillTransform.localScale = new Vector3(ratio, 1f, 1f);
            _fillTransform.localPosition = _offset + new Vector3((_size.x * ratio - _size.x) * 0.5f, 0f, 0f);
            _hpText.text = $"{Mathf.CeilToInt(_currentHp)}/{Mathf.CeilToInt(_maxHp)}";
        }

        public void SetTextFlipped(bool flipped)
        {
            _flipText = flipped;
            ApplyTextFlip();
        }

        private void CreateBar()
        {
            _pixel = CreateRoundedSprite();

            var bgGo = new GameObject("HealthBar_BG");
            bgGo.transform.SetParent(transform);
            bgGo.transform.localPosition = _offset;
            bgGo.transform.localRotation = Quaternion.identity;
            bgGo.transform.localScale = Vector3.one;
            var bgRenderer = bgGo.AddComponent<SpriteRenderer>();
            bgRenderer.sprite = _pixel;
            bgRenderer.color = _backgroundColor;
            bgRenderer.sortingOrder = _sortingOrder;

            var fillGo = new GameObject("HealthBar_Fill");
            fillGo.transform.SetParent(transform);
            fillGo.transform.localPosition = _offset;
            fillGo.transform.localRotation = Quaternion.identity;
            fillGo.transform.localScale = Vector3.one;
            var fillRenderer = fillGo.AddComponent<SpriteRenderer>();
            fillRenderer.sprite = _pixel;
            fillRenderer.color = _fillColor;
            fillRenderer.sortingOrder = _sortingOrder + 1;
            _fillTransform = fillGo.transform;

            var textGo = new GameObject("HealthBar_Text");
            textGo.transform.SetParent(transform);
            textGo.transform.localPosition = _offset;
            textGo.transform.localRotation = Quaternion.identity;
            textGo.transform.localScale = Vector3.one;
            _hpText = textGo.AddComponent<TextMeshPro>();
            _hpText.fontSize = _fontSize;
            _hpText.alignment = TextAlignmentOptions.Center;
            _hpText.sortingOrder = _sortingOrder + 2;
            _hpText.rectTransform.sizeDelta = _size;
            ApplyTextFlip();
        }

        private void ApplyTextFlip()
        {
            if (_hpText == null) return;

            var scale = _hpText.transform.localScale;
            float x = Mathf.Abs(scale.x);
            float y = Mathf.Abs(scale.y);
            _hpText.transform.localScale = new Vector3(_flipText ? -x : x, _flipText ? -y : y, scale.z);
        }

        private Sprite CreateRoundedSprite()
        {
            const int texH = 64;
            int texW = Mathf.Max(1, Mathf.RoundToInt(_size.x / _size.y * texH));
            float r = _cornerRadius * _size.y;
            float pixelSize = _size.y / texH;

            var tex = new Texture2D(texW, texH, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            var pixels = new Color32[texW * texH];
            for (int py = 0; py < texH; py++)
            {
                for (int px = 0; px < texW; px++)
                {
                    float wx = (px + 0.5f) / texW * _size.x;
                    float wy = (py + 0.5f) / texH * _size.y;
                    float sdf = RoundedRectSDF(wx, wy, _size.x, _size.y, r);
                    float alpha = Mathf.Clamp01(0.5f - sdf / pixelSize);
                    pixels[py * texW + px] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();

            float ppu = texH / _size.y;
            return Sprite.Create(tex, new Rect(0, 0, texW, texH), new Vector2(0.5f, 0.5f), ppu);
        }

        private static float RoundedRectSDF(float wx, float wy, float W, float H, float r)
        {
            float qx = Mathf.Abs(wx - W * 0.5f) - (W * 0.5f - r);
            float qy = Mathf.Abs(wy - H * 0.5f) - (H * 0.5f - r);
            float outer = Mathf.Sqrt(Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) + Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f));
            return outer + Mathf.Min(Mathf.Max(qx, qy), 0f) - r;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Vector3 worldPos = transform.position + _offset;

            // Фон
            Gizmos.color = _backgroundColor;
            Gizmos.DrawCube(worldPos, new Vector3(_size.x, _size.y, 0f));

            // Заливка (полный HP)
            Gizmos.color = _fillColor;
            Gizmos.DrawCube(worldPos, new Vector3(_size.x, _size.y * 0.8f, 0f));

            // Рамка
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(worldPos, new Vector3(_size.x, _size.y, 0f));
        }
#endif
    }
}
