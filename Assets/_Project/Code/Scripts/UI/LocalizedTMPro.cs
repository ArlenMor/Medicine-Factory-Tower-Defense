using _Project.Code.Scripts.Localization;
using _Project.Code.Scripts.ServiceLocator;
using TMPro;
using UnityEngine;

namespace _Project.Code.Scripts.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedTMPro : MonoBehaviour
    {
        [SerializeField] private string _key;
        [SerializeField] private bool _localizeOnStart = true;

        private TMP_Text _text;
        private LocalizationService _locService;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
        }

        private void Start()
        {
            if (S.TryGet<LocalizationService>(out var service))
            {
                _locService = service;
                _locService.OnLocaleChanged += Refresh;
                if (_localizeOnStart)
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
            if (_text == null || string.IsNullOrEmpty(_key)) return;
            _text.text = _locService.GetString(_key);
        }
    }
}
