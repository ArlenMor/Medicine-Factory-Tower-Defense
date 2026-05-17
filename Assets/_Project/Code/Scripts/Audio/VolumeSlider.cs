using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.Scripts.Audio
{
    public class VolumeSlider : MonoBehaviour
    {
        [SerializeField] private Slider _slider;

        private void Start()
        {
            _slider.value = AudioListener.volume;
            _slider.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDestroy()
        {
            _slider.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(float value)
        {
            AudioManager.Instance.SetVolume(value);
        }
    }
}
