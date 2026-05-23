using _Project.Code.Scripts.Localization;
using _Project.Code.Scripts.ServiceLocator;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.Scripts.UI
{
    [RequireComponent(typeof(Image))]
    public class LocalizedImage : MonoBehaviour
    {
        [SerializeField] private string _key;

        private Image _image;
        private LocalizationService _locService;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void Start()
        {
            if (S.TryGet<LocalizationService>(out var service))
            {
                _locService = service;
                _locService.OnLocaleChanged += Refresh;
                Refresh();
            }
        }

        private void OnDestroy()
        {
            if (_locService != null)
                _locService.OnLocaleChanged -= Refresh;
        }

        public void SetKey(string key)
        {
            _key = key;
            if (enabled && gameObject.activeInHierarchy)
                Refresh();
        }

        public void Refresh()
        {
            if (_image == null || string.IsNullOrEmpty(_key)) return;
            var sprite = _locService.GetSprite(_key);
            if (sprite != null)
                _image.sprite = sprite;
        }
    }
}
