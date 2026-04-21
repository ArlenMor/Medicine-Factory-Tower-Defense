using UnityEngine;
using _Project.Code.Scripts.InputResolverService;

namespace _Project.Code.Scripts.CameraController
{
    public class CameraEdgeScroll : MonoBehaviour
    {
        [SerializeField] private float _dragSpeed = 0.01f;
        [SerializeField] private Collider2D _confinerBounds;

        private IInputResolver _inputResolver;
        private bool _isDragging;
        private Vector2 _lastMousePos;

        public void Initialize(IInputResolver inputResolver)
        {
            _inputResolver = inputResolver;
            _inputResolver.OnPointerDown += OnPointerDown;
            _inputResolver.OnPointerHeld += OnPointerHeld;
            _inputResolver.OnPointerUp += OnPointerUp;
        }

        private void OnDestroy()
        {
            if (_inputResolver == null) return;
            _inputResolver.OnPointerDown -= OnPointerDown;
            _inputResolver.OnPointerHeld -= OnPointerHeld;
            _inputResolver.OnPointerUp -= OnPointerUp;
        }

        private void OnPointerDown(InputEventData data)
        {
            if (data.Button == MouseButton.Left && data.Target == InputTarget.None)
            {
                _isDragging = true;
                _lastMousePos = data.ScreenPosition;
            }
        }

        private void OnPointerHeld(InputEventData data)
        {
            if (!_isDragging || data.Button != MouseButton.Left) return;

            Vector2 delta = data.ScreenPosition - _lastMousePos;
            _lastMousePos = data.ScreenPosition;

            transform.position += new Vector3(0f, -delta.y, 0f) * _dragSpeed;
            ClampPosition();
        }

        private void OnPointerUp(InputEventData data)
        {
            if (data.Button == MouseButton.Left)
                _isDragging = false;
        }

        private void ClampPosition()
        {
            if (_confinerBounds == null) return;

            Vector2 pos2D = new Vector2(transform.position.x, transform.position.y);
            Vector2 closest = _confinerBounds.ClosestPoint(pos2D);

            if (pos2D != closest)
                transform.position = new Vector3(closest.x, closest.y, transform.position.z);
        }
    }
}