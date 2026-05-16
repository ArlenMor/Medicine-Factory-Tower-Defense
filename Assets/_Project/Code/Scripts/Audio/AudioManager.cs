using UnityEngine;

namespace _Project.Code.Scripts.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Music")]
        [SerializeField] private AudioClip _mainTheme;

        [Header("Craft")]
        [SerializeField] private AudioClip _craftWorking;
        [SerializeField] private AudioClip _craftComplete;

        [Header("Resource Gathering")]
        [SerializeField] private AudioClip[] _resourceGather;

        [Header("Planting")]
        [SerializeField] private AudioClip[] _plantPlanting;

        [Header("Upgrade")]
        [SerializeField] private AudioClip _upgrade;

        [Header("Building")]
        [SerializeField] private AudioClip _building;

        [Header("Game Result")]
        [SerializeField] private AudioClip _victory;
        [SerializeField] private AudioClip _defeat;

        [Header("Player Click Hit")]
        [SerializeField] private AudioClip[] _clickHit;
        [SerializeField] [Range(0f, 1f)] private float _clickHitVolume = 0.35f;
        [SerializeField] [Range(0f, 0.5f)] private float _clickHitPitchVariation = 0.15f;

        [Header("Enemy Attack")]
        [SerializeField] private AudioClip[] _enemyAttack;
        [SerializeField] [Range(0f, 1f)] private float _enemyAttackVolume = 0.55f;

        [Header("Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _gameResultSource;
        [SerializeField] private AudioSource _loopSfxSource;

        public bool IsMuted { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void SetMuted(bool muted)
        {
            IsMuted = muted;
            AudioListener.volume = muted ? 0f : 1f;
        }

        public void ToggleMute()
        {
            SetMuted(!IsMuted);
        }

        public void PlayMainTheme()
        {
            if (_mainTheme == null) return;
            _musicSource.clip = _mainTheme;
            _musicSource.loop = true;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            _musicSource.Stop();
        }

        public void PlayCraftWorking()
        {
            if (_craftWorking == null) return;
            _loopSfxSource.clip = _craftWorking;
            _loopSfxSource.loop = true;
            _loopSfxSource.Play();
        }

        public void StopCraftWorking()
        {
            _loopSfxSource.Stop();
            _loopSfxSource.clip = null;
        }

        public void PlayCraftComplete()
        {
            if (_craftComplete == null) return;
            _sfxSource.PlayOneShot(_craftComplete);
        }

        public void PlayResourceGather()
        {
            if (_resourceGather == null || _resourceGather.Length == 0) return;
            var clip = _resourceGather[Random.Range(0, _resourceGather.Length)];
            if (clip != null) _sfxSource.PlayOneShot(clip);
        }

        public void PlayPlantPlanting()
        {
            if (_plantPlanting == null || _plantPlanting.Length == 0) return;
            var clip = _plantPlanting[Random.Range(0, _plantPlanting.Length)];
            if (clip != null) _sfxSource.PlayOneShot(clip);
        }

        public void PlayUpgrade()
        {
            if (_upgrade == null) return;
            _sfxSource.PlayOneShot(_upgrade);
        }

        public void PlayBuilding()
        {
            if (_building == null) return;
            _sfxSource.PlayOneShot(_building);
        }

        public void PlayVictory()
        {
            StopMusic();
            if (_victory == null) return;
            _gameResultSource.PlayOneShot(_victory);
        }

        public void PlayDefeat()
        {
            StopMusic();
            if (_defeat == null) return;
            _sfxSource.PlayOneShot(_defeat);
        }

        public void PlayClickHit()
        {
            if (_clickHit == null || _clickHit.Length == 0) return;
            var clip = _clickHit[Random.Range(0, _clickHit.Length)];
            if (clip == null) return;
            _sfxSource.pitch = Random.Range(1f - _clickHitPitchVariation, 1f + _clickHitPitchVariation);
            _sfxSource.PlayOneShot(clip, _clickHitVolume);
            _sfxSource.pitch = 1f;
        }

        public void PlayEnemyAttack()
        {
            if (_enemyAttack == null || _enemyAttack.Length == 0) return;
            var clip = _enemyAttack[Random.Range(0, _enemyAttack.Length)];
            if (clip != null) _sfxSource.PlayOneShot(clip, _enemyAttackVolume);
        }
    }
}
