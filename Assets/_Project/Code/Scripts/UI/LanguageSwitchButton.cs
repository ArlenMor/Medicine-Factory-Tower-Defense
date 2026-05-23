using _Project.Code.Scripts.Localization;
using _Project.Code.Scripts.ServiceLocator;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.Scripts.UI
{
    public class LanguageSwitchButton : MonoBehaviour
    {
        [SerializeField] private Button _button;

        private void Awake()
        {
            if (_button == null)
                _button = GetComponent<Button>();

            _button.onClick.AddListener(ToggleLocale);
        }

        private void ToggleLocale()
        {
            if (!S.TryGet<LocalizationService>(out var service)) return;

            var next = service.CurrentLocale == Language.Ru ? Language.En : Language.Ru;
            service.SetLocale(next);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(ToggleLocale);
        }
    }
}
