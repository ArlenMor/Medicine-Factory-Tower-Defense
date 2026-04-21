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
            _fillTransform.localScale = new Vector3(_size.x * ratio, _size.y, 1f);
            _fillTransform.localPosition = _offset + new Vector3((_size.x * ratio - _size.x) * 0.5f, 0f, 0f);
            _hpText.text = $"{Mathf.CeilToInt(_currentHp)}/{Mathf.CeilToInt(_maxHp)}";
        }

        private void CreateBar()
        {
            _pixel = CreatePixelSprite();

            var bgGo = new GameObject("HealthBar_BG");
            bgGo.transform.SetParent(transform);
            bgGo.transform.localPosition = _offset;
            bgGo.transform.localRotation = Quaternion.identity;
            bgGo.transform.localScale = new Vector3(_size.x, _size.y, 1f);
            var bgRenderer = bgGo.AddComponent<SpriteRenderer>();
            bgRenderer.sprite = _pixel;
            bgRenderer.color = _backgroundColor;
            bgRenderer.sortingOrder = _sortingOrder;

            var fillGo = new GameObject("HealthBar_Fill");
            fillGo.transform.SetParent(transform);
            fillGo.transform.localPosition = _offset;
            fillGo.transform.localRotation = Quaternion.identity;
            fillGo.transform.localScale = new Vector3(_size.x, _size.y, 1f);
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
        }

        private static Sprite CreatePixelSprite()
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
    }
}
