using System.Collections.Generic;
using UnityEngine;

namespace _Project.Code.Scripts.BattleField
{
    public class BulletPool : MonoBehaviour
    {
        public static BulletPool Instance { get; private set; }

        [SerializeField] private TurretBullet _bulletPrefab;
        [SerializeField] private int _initialSize = 20;

        private readonly Queue<TurretBullet> _pool = new();

        private void Awake()
        {
            Instance = this;
            Prewarm();
        }

        private void Prewarm()
        {
            for (int i = 0; i < _initialSize; i++)
            {
                var bullet = Instantiate(_bulletPrefab, transform);
                bullet.gameObject.SetActive(false);
                _pool.Enqueue(bullet);
            }
        }

        public TurretBullet Get(Vector3 position, Quaternion rotation)
        {
            TurretBullet bullet;

            if (_pool.Count > 0)
            {
                bullet = _pool.Dequeue();
            }
            else
            {
                bullet = Instantiate(_bulletPrefab, transform);
            }

            bullet.transform.SetPositionAndRotation(position, rotation);
            bullet.gameObject.SetActive(true);
            return bullet;
        }

        public void Return(TurretBullet bullet)
        {
            bullet.gameObject.SetActive(false);
            _pool.Enqueue(bullet);
        }
    }
}
