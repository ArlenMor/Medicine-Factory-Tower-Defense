using UnityEngine;

namespace _Project.Code.Scripts.InputResolverService
{
    internal class InputResolverDebug : MonoBehaviour
    {
        [SerializeField] private InputResolver _resolver;
        [SerializeField] private Color _rayColorCanvas = Color.yellow;
        [SerializeField] private Color _rayColorWorld = Color.green;
        [SerializeField] private Color _rayColorNone = Color.red;
        [SerializeField] private float _gizmoRayDuration = 0.5f;

        private Ray _lastRay;
        private float _lastRayDistance;
        private Color _lastRayColor;
        private float _lastRayTime;

        private void OnEnable()
        {
            if (_resolver == null) return;
            _resolver.OnPointerDown += OnDown;
            _resolver.OnPointerUp += OnUp;
        }

        private void OnDisable()
        {
            if (_resolver == null) return;
            _resolver.OnPointerDown -= OnDown;
            _resolver.OnPointerUp -= OnUp;
        }

        private void OnDown(InputEventData data) => LogEvent(data, "Down");
        private void OnUp(InputEventData data) => LogEvent(data, "Up");

        private void LogEvent(InputEventData data, string state)
        {
            string target = data.Target.ToString();
            string hitName = data.HitObject ? data.HitObject.name : "nothing";

            Debug.Log($"[InputResolver] {data.Button} {state} | Target: {target} | Hit: {hitName} | Pos: {data.ScreenPosition}");

            DrawDebugRay(data);
        }

        private void DrawDebugRay(InputEventData data)
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(data.ScreenPosition);

            Color color = data.Target switch
            {
                InputTarget.Canvas => _rayColorCanvas,
                InputTarget.World => _rayColorWorld,
                _ => _rayColorNone
            };

            float distance = data.WorldHit.HasValue ? data.WorldHit.Value.distance : 100f;

            Debug.DrawRay(ray.origin, ray.direction * distance, color, _gizmoRayDuration);

            _lastRay = ray;
            _lastRayDistance = distance;
            _lastRayColor = color;
            _lastRayTime = Time.time;
        }

        private void OnDrawGizmos()
        {
            if (Time.time - _lastRayTime > _gizmoRayDuration) return;

            Gizmos.color = _lastRayColor;
            Gizmos.DrawRay(_lastRay.origin, _lastRay.direction * _lastRayDistance);

            if (_lastRayDistance < 100f)
            {
                Gizmos.DrawWireSphere(_lastRay.GetPoint(_lastRayDistance), 0.15f);
            }
        }
    }
}
