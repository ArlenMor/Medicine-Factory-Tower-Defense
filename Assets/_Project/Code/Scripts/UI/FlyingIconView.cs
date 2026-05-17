using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.Scripts.UI
{
    public class FlyingIconView : MonoBehaviour
    {
        [SerializeField] private Image _icon;

        public RectTransform RectTransform { get; private set; }

        private void Awake()
        {
            RectTransform = (RectTransform)transform;
        }

        public void Initialize(Sprite sprite)
        {
            _icon.sprite = sprite;
        }
    }
}
