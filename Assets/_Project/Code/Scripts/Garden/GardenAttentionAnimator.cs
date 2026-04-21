using DG.Tweening;
using UnityEngine;

namespace _Project.Code.Scripts.Garden
{
    public class GardenAttentionAnimator : MonoBehaviour
    {
        [SerializeField] private float _defaultScale = 0.6f;
        [SerializeField] private float _pulseScale = 0.8f;
        [SerializeField] private float _pulseDuration = 0.55f;
        [SerializeField] private float _pauseBetweenPulses = 0.4f;

        private Sequence _loopSequence;
        private bool _stopRequested;

        private void Awake()
        {
            transform.localScale = Vector3.one * _defaultScale;
        }

        public void PlayAttention()
        {
            _loopSequence?.Kill();
            _stopRequested = false;
            transform.localScale = Vector3.one * _defaultScale;

            _loopSequence = DOTween.Sequence()
                .Append(transform.DOScale(_pulseScale, _pulseDuration).SetEase(Ease.InOutSine))
                .Append(transform.DOScale(_defaultScale, _pulseDuration).SetEase(Ease.InOutSine))
                .AppendInterval(_pauseBetweenPulses)
                .SetLoops(-1)
                .OnStepComplete(() =>
                {
                    if (!_stopRequested) return;
                    _loopSequence?.Kill();
                    _loopSequence = null;
                    transform.localScale = Vector3.one * _defaultScale;
                })
                .SetLink(gameObject);
        }

        public void StopAttention()
        {
            if (_loopSequence != null && _loopSequence.IsPlaying())
            {
                _stopRequested = true;
            }
            else
            {
                _loopSequence?.Kill();
                _loopSequence = null;
                if (this != null)
                    transform.localScale = Vector3.one * _defaultScale;
            }
        }

        private void OnDestroy()
        {
            _loopSequence?.Kill();
        }
    }
}
