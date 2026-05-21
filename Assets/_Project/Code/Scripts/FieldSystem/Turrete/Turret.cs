using System;
using _Project.Code.Scripts.Data;
using _Project.Code.Scripts.UI;
using DG.Tweening;
using UnityEngine;

namespace _Project.Code.Scripts.BattleField
{
    public class Turret : MonoBehaviour, IFieldPlaceable, IManualUpdate
    {
        public event Action<IFieldPlaceable> OnPlaceableDestroyed;

        [SerializeField] private TurretShooter _shooter;
        [SerializeField] private TurretRadiusDisplay _radiusDisplay;
        [SerializeField] private HealthBar _healthBar;

        private float _currentHp;
        private bool _placed;

        public bool IsDead => _currentHp <= 0f;

        public void Initialize(DefenseItemData data)
        {
            _currentHp = data.MaxHp;
            _shooter.Initialize(data.AttackRadius, data.FireRate, data.Damage, data.BulletSpeed);
            _radiusDisplay.Initialize(data.AttackRadius, data.RadiusFadeDuration, data.RadiusMaxAlpha);
            if (_healthBar != null) _healthBar.Initialize(data.MaxHp);
        }

        public void ManualUpdate(float deltaTime)
        {
            if (IsDead || !_placed) return;

            _shooter.Tick(deltaTime);
        }

        public void Place()
        {
            _placed = true;
            _radiusDisplay.Lock(false);
            _radiusDisplay.Show();
        }

        public void LockRadius(bool locked)
        {
            _radiusDisplay.Lock(locked);
        }

        public void TakeDamage(float damage)
        {
            if (IsDead) return;

            _currentHp -= damage;
            if (_healthBar != null) _healthBar.SetHealth(_currentHp);
            DamagePopup.Spawn(transform.position, damage, transform, GameData.Instance.GameConfig.DamagePopupConfig);
            if (_currentHp <= 0f)
            {
                _currentHp = 0f;
                OnPlaceableDestroyed?.Invoke(this);
                PlayDeathAnimation();
            }
        }
        private void PlayDeathAnimation()
        {
            if (_healthBar != null) _healthBar.gameObject.SetActive(false);
            transform.DOKill();
            float startY = transform.localPosition.y;
            DOTween.Sequence()
                .Append(transform.DOLocalMoveY(startY + 0.3f, 0.12f).SetEase(Ease.OutQuad))
                .Append(transform.DOLocalMoveY(startY - 1.5f, 0.35f).SetEase(Ease.InQuad))
                .OnComplete(() => gameObject.SetActive(false));
        }
    }
}
