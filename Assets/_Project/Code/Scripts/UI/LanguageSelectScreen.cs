using System;
using DG.Tweening;
using _Project.Code.Scripts.Localization;
using _Project.Code.Scripts.ServiceLocator;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.Scripts.UI
{
    public class LanguageSelectScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _englishButton;
        [SerializeField] private Button _russianButton;
        [SerializeField] private float _fadeDuration = 0.4f;

        public event Action OnLanguageSelected;

        public void ManualAwake()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
            _englishButton.onClick.AddListener(() => SelectLanguage(Language.En));
            _russianButton.onClick.AddListener(() => SelectLanguage(Language.Ru));
        }

        public void Show()
        {
            gameObject.SetActive(true);
            _canvasGroup.DOFade(1f, _fadeDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => _canvasGroup.blocksRaycasts = true);
        }

        private void SelectLanguage(Language lang)
        {
            S.Get<LocalizationService>().SetLocale(lang);
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.DOFade(0f, _fadeDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    OnLanguageSelected?.Invoke();
                });
        }

        private void OnDestroy()
        {
            _englishButton.onClick.RemoveAllListeners();
            _russianButton.onClick.RemoveAllListeners();
        }
    }
}
