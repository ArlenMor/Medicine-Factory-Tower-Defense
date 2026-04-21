using _Project.Code.Scripts.EnemySystem;
using UnityEngine;

namespace _Project.Code.Scripts.BattleField
{
    public class TurretBullet : MonoBehaviour
    {
        private float _speed;
        private float _damage;
        private Transform _target;
        private Vector3 _lastTargetPosition;
        private bool _active;

        public void Initialize(Transform target, float speed, float damage)
        {
            _target = target;
            _speed = speed;
            _damage = damage;
            _lastTargetPosition = target.position;
            _active = true;
        }

        private void Update()
        {
            if (!_active) return;

            if (_target != null && _target.gameObject.activeInHierarchy)
                _lastTargetPosition = _target.position;

            Vector3 direction = (_lastTargetPosition - transform.position);
            float distanceThisFrame = _speed * Time.deltaTime;

            if (direction.magnitude <= distanceThisFrame)
            {
                HitTarget();
                return;
            }

            transform.position += direction.normalized * distanceThisFrame;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }

        private void HitTarget()
        {
            if (_target != null && _target.TryGetComponent(out Enemy enemy) && !enemy.IsDead)
                enemy.TakeDamage(_damage);

            _active = false;
            BulletPool.Instance.Return(this);
        }
    }
}
