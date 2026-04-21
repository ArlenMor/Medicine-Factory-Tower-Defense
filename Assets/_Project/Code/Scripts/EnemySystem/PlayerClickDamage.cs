using _Project.Code.Scripts.Audio;
using _Project.Code.Scripts.InputResolverService;
using UnityEngine;

namespace _Project.Code.Scripts.EnemySystem
{
    public class PlayerClickDamage : MonoBehaviour
    {
        private const float ClickDamage = 7f;
        private const float ClickCooldown = 0.22f;

        [SerializeField] private ParticleSystem _hitEffectPrefab;

        private IInputResolver _inputResolver;
        private float _lastClickTime = float.NegativeInfinity;

        public void ManualAwake(IInputResolver inputResolver)
        {
            _inputResolver = inputResolver;
            _inputResolver.OnPointerDown += HandlePointerDown;
        }

        private void OnDestroy()
        {
            if (_inputResolver != null)
                _inputResolver.OnPointerDown -= HandlePointerDown;
        }

        private void HandlePointerDown(InputEventData data)
        {
            if (data.Button != MouseButton.Left) return;
            if (!data.IsWorldHit) return;
            if (Time.time - _lastClickTime < ClickCooldown) return;

            if (data.HitObject != null && data.HitObject.TryGetComponent(out Enemy enemy))
            {
                enemy.TakeDamage(ClickDamage);
                _lastClickTime = Time.time;
                AudioManager.Instance.PlayClickHit();
                SpawnHitEffect(data);
            }
        }

        private void SpawnHitEffect(InputEventData data)
        {
            if (_hitEffectPrefab == null) return;

            Vector3 hitPosition = data.WorldHit.HasValue
                ? data.WorldHit.Value.point
                : data.WorldHit2D.HasValue
                    ? (Vector3)data.WorldHit2D.Value.point
                    : data.HitObject.transform.position;

            ParticleSystem fx = Instantiate(_hitEffectPrefab, hitPosition, Quaternion.identity, transform);
            fx.Play();
            Destroy(fx.gameObject, fx.main.duration + fx.main.startLifetime.constantMax);
        }
    }
}
