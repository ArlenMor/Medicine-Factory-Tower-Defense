using TMPro;
using UnityEngine;

namespace _Project.Code.Scripts.UI
{
    public class LevelTimer : MonoBehaviour
    {
        [SerializeField] private TMP_Text _timerText;

        public float Elapsed { get; private set; }
        public bool Running { get; private set; }

        public void StartTimer()
        {
            Elapsed = 0f;
            Running = true;
        }

        public void StopTimer()
        {
            Running = false;
        }

        private void Update()
        {
            if (!Running) return;
            Elapsed += Time.deltaTime;
            int minutes = (int)(Elapsed / 60f);
            int seconds = (int)(Elapsed % 60f);
            _timerText.text = $"{minutes:D2}:{seconds:D2}";
        }
    }
}
