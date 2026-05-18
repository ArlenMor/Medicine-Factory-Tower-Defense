using _Project.Code.Scripts.Data;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _Project.Code.Scripts.UI
{
    public class StoredResourceView: MonoBehaviour
    {
        [SerializeField] private ResourceType resourceType;
        [SerializeField] private TMP_Text _resourceAmount;

        [Header("Bump Animation")]
        [SerializeField] private float _bumpPunch = 0.3f;
        [SerializeField] private float _bumpDuration = 0.35f;

        private FlyingIconService _flyingIconService;

        public void Initialize(FlyingIconService flyingIconService = null)
        {
            GameData.Instance.OnResourcesChanged += OnResourcesChanged;
            OnResourcesChanged();

            _flyingIconService = flyingIconService;
            if (_flyingIconService != null)
                _flyingIconService.OnIconArrived += OnFlyIconArrived;
        }

        private void OnResourcesChanged()
        {
            _resourceAmount.text = GameData.Instance.Resources[resourceType].ToString();
        }

        private void OnFlyIconArrived(ResourceType arrivedType)
        {
            if (arrivedType != resourceType) return;
            transform.DOKill();
            transform.localScale = Vector3.one;
            transform.DOPunchScale(Vector3.one * _bumpPunch, _bumpDuration, 5, 0.5f)
                     .SetAutoKill(true);
        }

        private void OnDestroy()
        {
            GameData.Instance.OnResourcesChanged -= OnResourcesChanged;
            if (_flyingIconService != null)
                _flyingIconService.OnIconArrived -= OnFlyIconArrived;
        }
    }
}