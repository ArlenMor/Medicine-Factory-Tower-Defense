using _Project.Code.Scripts.Audio;
using _Project.Code.Scripts.EnemySystem;
using DG.Tweening;
using UnityEngine;

namespace _Project.Code.Scripts.BattleField
{
    public class TurretShooter : MonoBehaviour
    {
        [SerializeField] private Transform _gunTransform;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private LayerMask _enemyLayer;

        private float _attackRadius;
        private float _fireRate;
        private float _damage;
        private float _bulletSpeed;

        private float _fireCooldown;
        private Enemy _currentTarget;

        public void Initialize(float attackRadius, float fireRate, float damage, float bulletSpeed)
        {
            _attackRadius = attackRadius;
            _fireRate = fireRate;
            _damage = damage;
            _bulletSpeed = bulletSpeed;
        }

        public void Tick(float deltaTime)
        {
            FindTarget();

            if (_currentTarget == null) return;

            RotateGunTowards(_currentTarget.transform.position);

            _fireCooldown -= deltaTime;
            if (_fireCooldown <= 0f)
            {
                Shoot();
                _fireCooldown = _fireRate;
            }
        }

        private void FindTarget()
        {
            if (_currentTarget != null && !_currentTarget.IsDead
                && IsInRange(_currentTarget.transform.position))
                return;

            _currentTarget = null;
            float closestDist = float.MaxValue;

            var hits = Physics2D.OverlapCircleAll(transform.position, _attackRadius, _enemyLayer);
            foreach (var hit in hits)
            {
                if (!hit.TryGetComponent(out Enemy enemy) || enemy.IsDead)
                    continue;

                float dist = (enemy.transform.position - transform.position).sqrMagnitude;
                if (dist < closestDist)
                {
                    closestDist = dist;
                    _currentTarget = enemy;
                }
            }
        }

        private bool IsInRange(Vector3 position)
        {
            return (position - transform.position).sqrMagnitude <= _attackRadius * _attackRadius;
        }

        private void RotateGunTowards(Vector3 targetPosition)
        {
            Vector3 dir = targetPosition - _gunTransform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            _gunTransform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }

        private void Shoot()
        {
            if (BulletPool.Instance == null) return;

            var bullet = BulletPool.Instance.Get(_firePoint.position, _firePoint.rotation);
            bullet.Initialize(_currentTarget.transform, _bulletSpeed, _damage);

            AudioManager.Instance.PlayTurretShoot();

            _gunTransform.DOKill();
            _gunTransform.DOPunchPosition(-_gunTransform.up * 0.08f, 0.15f, 1, 0.5f)
                         .SetUpdate(UpdateType.Normal, false);
        }
    }
}
