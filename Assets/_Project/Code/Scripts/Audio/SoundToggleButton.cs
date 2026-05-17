using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _Project.Code.Scripts.Audio
{
    public class SoundVolumeButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private RectTransform _buttonRect;
        [SerializeField] private GameObject _volumePanel;
        [SerializeField] private GameObject _iconOn;
        [SerializeField] private GameObject _iconOff;
        [SerializeField] private float _hideDistance = 150f;


        private void Awake()
        {
            _button.onClick.AddListener(OnClick);
        }

        private void Start()
        {
            AudioManager.Instance.OnVolumeChanged += OnVolumeChanged;
            RefreshIcons();
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnClick);
            if (AudioManager.Instance != null)
                AudioManager.Instance.OnVolumeChanged -= OnVolumeChanged;
        }

        private void Update()
        {
            if (_volumePanel == null || !_volumePanel.activeSelf) return;

            Vector2 buttonScreenPos = RectTransformUtility.WorldToScreenPoint(null, _buttonRect.position);
            float distance = Vector2.Distance(Mouse.current.position.ReadValue(), buttonScreenPos);

            if (distance > _hideDistance)
                _volumePanel.SetActive(false);
        }

        private void OnClick()
        {
            if (_volumePanel != null)
                _volumePanel.SetActive(true);
        }

        private void OnVolumeChanged(float volume)
        {
            RefreshIcons();
        }

        private void RefreshIcons()
        {
            bool hasSound = AudioListener.volume > 0f;
            if (_iconOn != null) _iconOn.SetActive(hasSound);
            if (_iconOff != null) _iconOff.SetActive(!hasSound);
        }
    }
}
