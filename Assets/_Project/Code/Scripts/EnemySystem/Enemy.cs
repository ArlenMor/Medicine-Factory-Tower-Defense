using System;
using _Project.Code.Scripts.Audio;
using _Project.Code.Scripts.BattleField;
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

        public event Action<Enemy> OnDied;

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
        private bool _reachedCenter;

        public bool IsDead => _currentHp <= 0f;

        public void Initialize(EnemyStats stats, BrainView centerTarget)
        {
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

            _targetPlaceable = null;

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
                AudioManager.Instance.PlayEnemyAttack();
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
            if (_currentHp <= 0f)
            {
                _currentHp = 0f;
                OnDied?.Invoke(this);
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_targetPlaceable == null && other.TryGetComponent(out IFieldPlaceable placeable) && !placeable.IsDead)
            {
                _targetPlaceable = placeable;
                _attackTimer = 0f;
            }
        }
    }
}
