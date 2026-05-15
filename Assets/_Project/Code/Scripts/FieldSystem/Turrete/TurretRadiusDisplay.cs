using _Project.Code.Scripts.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Code.Scripts.BattleField
{
    public class TurretRadiusDisplay : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _radiusRenderer;
        [Tooltip("Коллайдер тела башни для определения наведения мыши.")]
        [SerializeField] private Collider2D _hoverCollider;

        private float _attackRadius;
        private float _fadeDuration;
        private float _maxAlpha;
        private Camera _camera;

        private bool _isMouseOver;
        private float _fadeTimer;
        private bool _visible;
        private bool _locked;

        public void Initialize(float attackRadius, float fadeDuration, float maxAlpha)
        {
            _attackRadius = attackRadius;
            _fadeDuration = fadeDuration;
            _maxAlpha = maxAlpha;
            _camera = GameData.Instance.Camera;

            if (_radiusRenderer == null) return;

            float diameter = _attackRadius * 2f;
            Vector2 spriteSize = _radiusRenderer.sprite.bounds.size;
            _radiusRenderer.transform.localScale = new Vector3(
                diameter / spriteSize.x,
                diameter / spriteSize.y,
                1f);

            SetAlpha(0f);
        }

        private void Update()
        {
            UpdateMouseOver();
            UpdateFade();
        }

        public void Show()
        {
            _visible = true;
            _fadeTimer = _fadeDuration;
            SetAlpha(_maxAlpha);
        }

        public void Lock(bool locked)
        {
            _locked = locked;
            if (_locked)
                SetAlpha(_maxAlpha);
        }

        private void UpdateMouseOver()
        {
            if (_locked) return;
            if (Mouse.current == null) return;

            Camera cam = _camera ? _camera : Camera.main;
            Vector2 mouseWorld = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            bool wasOver = _isMouseOver;
            _isMouseOver = _hoverCollider != null
                ? _hoverCollider.OverlapPoint(mouseWorld)
                : (mouseWorld - (Vector2)transform.position).sqrMagnitude <= _attackRadius * _attackRadius;

            if (_isMouseOver && !wasOver)
                SetAlpha(_maxAlpha);
            else if (!_isMouseOver && wasOver)
                Show();
        }

        private void UpdateFade()
        {
            if (_radiusRenderer == null) return;

            if (_locked || _isMouseOver)
            {
                SetAlpha(_maxAlpha);
                return;
            }

            if (!_visible) return;

            _fadeTimer -= Time.deltaTime;
            if (_fadeTimer <= 0f)
            {
                _visible = false;
                SetAlpha(0f);
            }
            else
            {
                SetAlpha(_maxAlpha * (_fadeTimer / _fadeDuration));
            }
        }

        private void SetAlpha(float alpha)
        {
            if (_radiusRenderer == null) return;
            Color c = _radiusRenderer.color;
            c.a = alpha;
            _radiusRenderer.color = c;
        }
    }
}
