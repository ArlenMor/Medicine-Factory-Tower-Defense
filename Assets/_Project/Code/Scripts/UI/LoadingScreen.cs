using System;
using DG.Tweening;
using UnityEngine;

namespace _Project.Code.Scripts.UI
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeDuration = 0.4f;

        private void Awake()
        {
            gameObject.SetActive(true);
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
        }

        public void Hide(Action onComplete = null)
        {
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup
                .DOFade(0f, _fadeDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    onComplete?.Invoke();
                });
        }
    }
}
