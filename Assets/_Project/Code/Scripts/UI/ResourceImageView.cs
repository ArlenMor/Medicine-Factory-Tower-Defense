using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.Scripts.UI
{
    public class ResourceImageView: MonoBehaviour
    {
        [SerializeField] private Image _resourceIcon;
        [SerializeField] private TMP_Text _valueText;
        [SerializeField] private CanvasGroup _canvasGroup;
        
        public CanvasGroup CanvasGroup => _canvasGroup;
        
        public void Initialize(Sprite sprite, int value)
        {
            _resourceIcon.sprite = sprite;
            _valueText.text = $"+{value.ToString()}";
        }
    }
}