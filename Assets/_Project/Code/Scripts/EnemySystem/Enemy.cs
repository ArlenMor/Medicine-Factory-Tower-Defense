using System;
using System.Collections.Generic;
using _Project.Code.Scripts.Audio;
using _Project.Code.Scripts.BattleField;
using _Project.Code.Scripts.Data;
using _Project.Code.Scripts.UI;
using UnityEngine;

namespace _Project.Code.Scripts.EnemySystem
{
    /// <summary>
    /// Враг: двигается к CenterTarget, атакует баррикады на пути.
    /// Требует Collider на префабе для обнаружения кликов (raycast).
    /// Баррикады обнаруживаются через OnTriggerEnter — нужен Rigidbody (Kinematic) на враге
    /// и Collider (Is Trigger) на баррикаде.
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private EnemyAnimator _animator;
        [SerializeField] private HealthBar _healthBar;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        [SerializeField] private float _flashDuration = 0.15f;

        public event Action<Enemy> OnDied;

        public EnemyType Type { get; private set; }

        private float _currentHp;
        private float _speed;
        private float _centerDamage;
        private float _barricadeDamage;
        private float _attackInterval;
        private float _attackTimer;

        private Vector3 _attackPosition;
        private Vector3 _lungeTarget;
        private BrainView _centerTarget;
        private IFieldPlaceable _targetPlaceable;
        private readonly List<IFieldPlaceable> _overlappingPlaceables = new();
        private bool _reachedCenter;

        private MaterialPropertyBlock _mpb;
        private float _flashTimer;
        private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");

        public bool IsDead => _currentHp <= 0f;

        public void Initialize(EnemyStats stats, BrainView centerTarget)
        {
            Type = stats.Type;
            _currentHp = stats.HP;
            _speed = stats.Speed;
            _centerDamage = stats.CenterDamage;
            _barricadeDamage = stats.BarricadeDamage;
            _attackInterval = stats.AttackInterval;
            _centerTarget = centerTarget;
            _attackTimer = 0f;
            _reachedCenter = false;
            _targetPlaceable = null;

            _attackPosition = ComputeAttackPosition(centerTarget, out _lungeTarget);
            SetRotationTowards(_attackPosition);
            _healthBar.Initialize(stats.HP);

            _mpb ??= new MaterialPropertyBlock();
        }

        private Vector3 ComputeAttackPosition(BrainView centerTarget, out Vector3 lungeTarget)
        {
            var collider = centerTarget.GetComponent<Collider>();
            if (collider != null)
            {
                Vector3 closestPoint = collider.ClosestPoint(transform.position);
                Vector3 dir = (transform.position - closestPoint).normalized;
                lungeTarget = closestPoint;
                return closestPoint + dir * centerTarget.AttackRange;
            }

            if (centerTarget.Collider != null)
            {
                Vector2 closestPoint = centerTarget.Collider.ClosestPoint(transform.position);
                Vector2 dir = ((Vector2)transform.position - closestPoint).normalized;
                lungeTarget = closestPoint;
                return (Vector3)(closestPoint + dir * centerTarget.AttackRange);
            }

            Vector3 fallbackDir = (transform.position - centerTarget.transform.position).normalized;
            lungeTarget = centerTarget.transform.position;
            return centerTarget.transform.position + fallbackDir * centerTarget.AttackRange;
        }

        private void SetRotationTowards(Vector3 attackPosition)
        {
            Vector3 dir = attackPosition - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }

        public void Tick(float deltaTime)
        {
            if (IsDead) return;

            if (_flashTimer > 0f)
            {
                _flashTimer -= deltaTime;
                float amount = Mathf.Clamp01(_flashTimer / _flashDuration);
                _mpb.SetFloat(FlashAmountId, amount);
                _spriteRenderer.SetPropertyBlock(_mpb);
            }

            if (_animator.IsLunging)
            {
                _animator.TickLunge(deltaTime);
                return;
            }

            if (_targetPlaceable != null && !_targetPlaceable.IsDead)
            {
                AttackPlaceable(deltaTime);
                _animator.TickIdle(deltaTime);
                return;
            }

            _targetPlaceable = TryPickOverlappingTarget();

            if (_targetPlaceable != null)
            {
                _attackTimer = 0f;
                AttackPlaceable(deltaTime);
                _animator.TickIdle(deltaTime);
                return;
            }

            if (_reachedCenter)
            {
                AttackCenter(deltaTime);
                _animator.TickIdle(deltaTime);
                return;
            }

            Move(deltaTime);
        }

        private void Move(float deltaTime)
        {
            Vector3 dir = _attackPosition - transform.position;
            float distance = dir.magnitude;

            if (distance < 0.2f)
            {
                _reachedCenter = true;
                _attackTimer = 0f;
                _animator.ResetScale();
                return;
            }

            transform.position += dir.normalized * _speed * deltaTime;
            _animator.TickWalk(deltaTime);
        }

        private void AttackCenter(float deltaTime)
        {
            _attackTimer -= deltaTime;
            if (_attackTimer <= 0f)
            {
                _centerTarget.TakeDamage(_centerDamage);
                _attackTimer = _attackInterval;
                //AudioManager.Instance.PlayEnemyAttack();
                _animator.StartLunge(_lungeTarget);
            }
        }

        private void AttackPlaceable(float deltaTime)
        {
            _attackTimer -= deltaTime;
            if (_attackTimer <= 0f)
            {
                _targetPlaceable.TakeDamage(_barricadeDamage);
                _attackTimer = _attackInterval;
                AudioManager.Instance.PlayEnemyAttack();
                _animator.StartLunge(_targetPlaceable.transform.position);
            }
        }

        public void TakeDamage(float damage)
        {
            if (IsDead) return;

            _currentHp -= damage;
            if (_healthBar != null) _healthBar.SetHealth(_currentHp);
            DamagePopup.Spawn(transform.position, damage, transform, GameData.Instance.GameConfig.DamagePopupConfig);
            if (_spriteRenderer != null)
            {
                _flashTimer = _flashDuration;
                _mpb.SetFloat(FlashAmountId, 1f);
                _spriteRenderer.SetPropertyBlock(_mpb);
            }
            if (_currentHp <= 0f)
            {
                _currentHp = 0f;
                OnDied?.Invoke(this);
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out IFieldPlaceable placeable) && !placeable.IsDead)
            {
                if (!_overlappingPlaceables.Contains(placeable))
                    _overlappingPlaceables.Add(placeable);

                if (_targetPlaceable == null)
                {
                    _targetPlaceable = placeable;
                    _attackTimer = 0f;
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent(out IFieldPlaceable placeable))
                _overlappingPlaceables.Remove(placeable);
        }

        private IFieldPlaceable TryPickOverlappingTarget()
        {
            for (int i = _overlappingPlaceables.Count - 1; i >= 0; i--)
            {
                var p = _overlappingPlaceables[i];
                if (p == null || p.IsDead)
                {
                    _overlappingPlaceables.RemoveAt(i);
                    continue;
                }
                return p;
            }
            return null;
        }
    }
}
