using TMPro;
using UnityEngine;

namespace _Project.Code.Scripts.UI
{
    /// <summary>
    /// Всплывающая цифра урона. Создаётся через DamagePopup.Spawn(),
    /// поднимается вверх, исчезает и уничтожает себя.
    /// </summary>
    public class DamagePopup : MonoBehaviour
    {
        private TextMeshPro _text;
        private float _lifetime;
        private float _elapsed;
        private float _speed;
        private Color _color;
        private Vector3 _direction;

        private static int _directionIndex;
        private static readonly float[] DirectionAngles = { 0f, -20f, 20f };

        public static void Spawn(Vector3 position, float damage, Transform parent = null, DamagePopupConfig config = null)
        {
            config ??= DamagePopupConfig.Default;

            float angle = DirectionAngles[_directionIndex % DirectionAngles.Length];
            _directionIndex++;

            var go = new GameObject("DamagePopup");
            go.transform.position = position + config.Offset;
            if (parent != null) go.transform.SetParent(parent);
            var popup = go.AddComponent<DamagePopup>();
            popup.Init(damage, config, angle);
        }

        private void Init(float damage, DamagePopupConfig config, float angleDeg)
        {
            _lifetime = config.Lifetime;
            _speed = config.FloatSpeed;
            _color = config.Color;

            float rad = (90f + angleDeg) * Mathf.Deg2Rad;
            _direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
            
            _text = gameObject.AddComponent<TextMeshPro>();
            _text.text = Mathf.CeilToInt(damage).ToString();
            _text.fontSize = config.FontSize;
            _text.color = _color;
            _text.alignment = TextAlignmentOptions.Center;
            _text.sortingOrder = config.SortingOrder;
            var textMaterial = new Material(config.FontMaterial);
            textMaterial.SetColor("_Color", config.Color);
            _text.fontSharedMaterial = textMaterial;
            _text.rectTransform.sizeDelta = new Vector2(2f, 1f);
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            transform.position += _direction * _speed * Time.deltaTime;

            float alpha = Mathf.Lerp(1f, 0f, _elapsed / _lifetime);
            _text.color = new Color(_color.r, _color.g, _color.b, alpha);

            if (_elapsed >= _lifetime)
                Destroy(gameObject);
        }
    }
}
