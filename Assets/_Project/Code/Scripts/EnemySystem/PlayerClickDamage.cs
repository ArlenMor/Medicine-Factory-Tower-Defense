using System.Collections.Generic;
using _Project.Code.Scripts.Audio;
using _Project.Code.Scripts.InputResolverService;
using _Project.Code.Scripts.ServiceLocator;
using _Project.Code.Scripts.Stats;
using UnityEngine;

namespace _Project.Code.Scripts.EnemySystem
{
    public class PlayerClickDamage : MonoBehaviour
    {
        private const float ClickDamage = 7f;
        private const float ClickCooldown = 0.22f;

        [SerializeField] private List<ParticleSystem> _hitEffectPrefabs;
        [SerializeField] private float _inputBufferWindow = 0.03f;

        private IInputResolver _inputResolver;
        private float _lastClickTime = float.NegativeInfinity;
        private InputEventData? _bufferedClick;

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

            float timeSinceLastClick = Time.time - _lastClickTime;

            if (timeSinceLastClick < ClickCooldown)
            {
                float remaining = ClickCooldown - timeSinceLastClick;
                if (remaining <= _inputBufferWindow)
                    _bufferedClick = data;
                return;
            }

            ExecuteClick(data);
        }

        private void Update()
        {
            if (_bufferedClick.HasValue && Time.time - _lastClickTime >= ClickCooldown)
            {
                ExecuteClick(_bufferedClick.Value);
                _bufferedClick = null;
            }
        }

        private void ExecuteClick(InputEventData data)
        {
            Vector2 worldPoint = data.WorldHit2D.HasValue
                ? data.WorldHit2D.Value.point
                : data.WorldHit.HasValue
                    ? (Vector2)data.WorldHit.Value.point
                    : (Vector2)(data.HitObject ? data.HitObject.transform.position : Vector3.zero);

            Collider2D col = Physics2D.OverlapPoint(worldPoint, LayerMask.GetMask("Enemy"));
            if (col == null || !col.TryGetComponent(out Enemy enemy)) return;

            enemy.TakeDamage(ClickDamage);
            _lastClickTime = Time.time;
            AudioManager.Instance.PlayClickHit();
            if (S.TryGet<GameplayLogger>(out var pLog))
                pLog.LogPlayerClick();
            SpawnHitEffect(data);
        }

        private void SpawnHitEffect(InputEventData data)
        {
            if (_hitEffectPrefabs == null || _hitEffectPrefabs.Count == 0) return;

            Vector3 hitPosition = data.WorldHit.HasValue
                ? data.WorldHit.Value.point
                : data.WorldHit2D.HasValue
                    ? (Vector3)data.WorldHit2D.Value.point
                    : data.HitObject.transform.position;

            ParticleSystem fx = Instantiate(_hitEffectPrefabs[Random.Range(0, _hitEffectPrefabs.Count)], hitPosition, Quaternion.identity, transform);
            fx.Play();
            Destroy(fx.gameObject, fx.main.duration + fx.main.startLifetime.constantMax);
        }
    }
}
