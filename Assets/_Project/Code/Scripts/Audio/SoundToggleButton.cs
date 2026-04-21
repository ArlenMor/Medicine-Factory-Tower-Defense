using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.Scripts.Audio
{
    public class SoundToggleButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _iconOn;
        [SerializeField] private GameObject _iconOff;

        private void Awake()
        {
            _button.onClick.AddListener(OnClick);
        }

        private void Start()
        {
            Refresh();
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            AudioManager.Instance.ToggleMute();
            Refresh();
        }

        private void Refresh()
        {
            bool muted = AudioManager.Instance.IsMuted;
            if (_iconOn != null) _iconOn.SetActive(!muted);
            if (_iconOff != null) _iconOff.SetActive(muted);
        }
    }
}
