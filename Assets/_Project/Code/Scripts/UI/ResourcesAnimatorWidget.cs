using _Project.Code.Scripts.Data;
using DG.Tweening;
using UnityEngine;

namespace _Project.Code.Scripts.UI
{
    public class ResourcesAnimatorWidget: MonoBehaviour
    {
        [SerializeField] private ResourceImageView _resourceImage;

        [SerializeField] private float _offset;
        [SerializeField] private float _duration;
        [SerializeField] private Ease _ease;
        
        private ResourceImageView _currentImage;
        private Tween _tween;
        
        public void PlayAnimation(ResourceType resourceType, int value)
        {
            if (_currentImage != null)
            {
                DestroyImage();
            }
            
            var sprite = GameData.Instance.GameConfig.ResourceIconConfig.GetIcon(resourceType);
            
            _currentImage = Instantiate(_resourceImage, transform);
            _currentImage.Initialize(sprite, value);
            
            var t1 = _currentImage.transform.DOLocalMoveY(-_offset, _duration)
                .SetEase(_ease);

            var t2 = _currentImage.CanvasGroup.DOFade(0f, _duration).SetEase(_ease);

            _tween = DOTween.Sequence()
                .Append(t1)
                .Join(t2)
                .AppendCallback(DestroyImage)
                .SetAutoKill();
        }

        private void DestroyImage()
        {
            _tween?.Kill();
            Destroy(_currentImage.gameObject);
            _currentImage = null;
        }

        public void Reset()
        {
            if (_currentImage != null)
                DestroyImage();
        }

        private void OnDestroy()
        {
            _tween?.Kill();
        }
    }
}