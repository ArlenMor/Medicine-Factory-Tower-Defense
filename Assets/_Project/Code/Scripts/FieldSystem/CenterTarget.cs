using System;
using _Project.Code.Scripts.Audio;
using _Project.Code.Scripts.Data;
using _Project.Code.Scripts.UI;
using UnityEngine;

namespace _Project.Code.Scripts.EnemySystem
{
    public class BrainView : MonoBehaviour, IPlayerDamageEventProvider
    {
        public event Action<float> OnDamageReceived;
        public event Action OnDied;

        public bool IsDead => _currentHp <= 0f;

        [SerializeField] private float _maxHp = 200f;
        [SerializeField] private float _attackRange = 0.5f;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private CenterTargetHealth _healthBar;
        [SerializeField] private ParticleSystem _hitEffectPrefab;
        [SerializeField] private Transform _parentFx;

        private float _currentHp;
        private ParticleSystem _activeHitEffect;

        public float AttackRange => _attackRange;
        public Collider2D Collider => _collider;

        private void Awake()
        {
            _currentHp = _maxHp;
            if (_healthBar != null) _healthBar.Initialize(_maxHp);
        }

        public void Reset()
        {
            _currentHp = _maxHp;
            if (_healthBar != null) _healthBar.SetHealth(_currentHp);
        }

        public void TakeDamage(float damage)
        {
            Debug.Log($"{nameof(BrainView)} took {damage} damage");
            _currentHp -= damage;
            if (_currentHp <= 0f) _currentHp = 0f;
            if (_healthBar != null) _healthBar.SetHealth(_currentHp);
            DamagePopup.Spawn(transform.position, damage, transform, GameData.Instance.GameConfig.DamagePopupConfig);
            SpawnHitEffect();
            AudioManager.Instance.PlayBrainHit();
            OnDamageReceived?.Invoke(damage);
            if (_currentHp <= 0f) OnDied?.Invoke();
        }

        private void SpawnHitEffect()
        {
            if (_hitEffectPrefab == null) return;
            if (_activeHitEffect != null) return;

            _activeHitEffect = Instantiate(_hitEffectPrefab, _parentFx.transform.position, Quaternion.identity, _parentFx);
            _activeHitEffect.Play();
            Destroy(_activeHitEffect.gameObject, _activeHitEffect.main.duration + _activeHitEffect.main.startLifetime.constantMax);
        }
    }

    public interface IPlayerDamageEventProvider
    {
        public event Action<float> OnDamageReceived;
        public event Action OnDied;
    }
}
