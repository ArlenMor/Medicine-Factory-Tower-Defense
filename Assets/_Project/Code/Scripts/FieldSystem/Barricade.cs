using System;
using _Project.Code.Scripts.Data;
using _Project.Code.Scripts.UI;
using DG.Tweening;
using UnityEngine;

namespace _Project.Code.Scripts.BattleField
{
    public class Barricade : MonoBehaviour, IFieldPlaceable
    {
        public event Action<Barricade> OnDestroyed;
        public event Action<IFieldPlaceable> OnPlaceableDestroyed;

        [SerializeField] private float _maxHp = 80f;
        [SerializeField] private HealthBar _healthBar;

        private float _currentHp;

        public bool IsDead => _currentHp <= 0f;

        private void Awake()
        {
            _currentHp = _maxHp;
            if (_healthBar != null) _healthBar.Initialize(_maxHp);
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
                OnDestroyed?.Invoke(this);
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
