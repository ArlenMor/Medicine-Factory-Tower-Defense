using System;
using System.Collections;
using UnityEngine;
using URandom = UnityEngine.Random;

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

        [Header("Turret Shoot")]
        [SerializeField] private AudioClip[] _turretShoot;
        [SerializeField] [Range(0f, 1f)] private float _turretShootVolume = 0.4f;

        [Header("Wrong Notify")]
        [SerializeField] private AudioClip _wrongNotify;

        [Header("Brain Hit")]
        [SerializeField] private AudioClip[] _brainHit;
        [SerializeField] [Range(0f, 1f)] private float _brainHitVolume = 0.7f;

        [Header("Enemy Attack")]
        [SerializeField] private AudioClip[] _enemyAttack;
        [SerializeField] [Range(0f, 1f)] private float _enemyAttackVolume = 0.55f;

        [Header("Crossfade")]
        [SerializeField] private float _crossfadeDuration = 1.5f;

        [Header("Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _gameResultSource;
        [SerializeField] private AudioSource _loopSfxSource;
        [SerializeField] private AudioSource _brainHitSource;

        private Coroutine _crossfadeCoroutine;

        public bool IsMuted { get; private set; }
        public float Volume => AudioListener.volume;

        public event Action<float> OnVolumeChanged;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            SetVolume(0.35f);
        }

        public void SetMuted(bool muted)
        {
            IsMuted = muted;
            AudioListener.volume = muted ? 0f : 1f;
            OnVolumeChanged?.Invoke(AudioListener.volume);
        }

        public void ToggleMute()
        {
            SetMuted(!IsMuted);
        }

        public void SetVolume(float volume)
        {
            AudioListener.volume = Mathf.Clamp01(volume);
            IsMuted = AudioListener.volume <= 0f;
            OnVolumeChanged?.Invoke(AudioListener.volume);
        }

        public void PlayMainTheme()
        {
            if (_mainTheme == null) return;

            if (_crossfadeCoroutine != null)
                StopCoroutine(_crossfadeCoroutine);

            if (_gameResultSource.isPlaying)
            {
                _crossfadeCoroutine = StartCoroutine(CrossfadeToMainTheme());
            }
            else
            {
                _musicSource.volume = 1f;
                _musicSource.clip = _mainTheme;
                _musicSource.loop = true;
                _musicSource.Play();
            }
        }

        public void StopMusic()
        {
            if (_crossfadeCoroutine != null)
            {
                StopCoroutine(_crossfadeCoroutine);
                _crossfadeCoroutine = null;
            }

            _musicSource.Stop();
        }

        private IEnumerator CrossfadeToMainTheme()
        {
            _musicSource.volume = 0f;
            _musicSource.clip = _mainTheme;
            _musicSource.loop = true;
            _musicSource.Play();

            float startResultVolume = _gameResultSource.volume;
            float elapsed = 0f;

            while (elapsed < _crossfadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _crossfadeDuration);
                _gameResultSource.volume = Mathf.Lerp(startResultVolume, 0f, t);
                _musicSource.volume = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }

            _gameResultSource.volume = startResultVolume;
            _gameResultSource.Stop();
            _musicSource.volume = 1f;
            _crossfadeCoroutine = null;
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
            var clip = _resourceGather[URandom.Range(0, _resourceGather.Length)];
            if (clip != null) _sfxSource.PlayOneShot(clip);
        }

        public void PlayPlantPlanting()
        {
            if (_plantPlanting == null || _plantPlanting.Length == 0) return;
            var clip = _plantPlanting[URandom.Range(0, _plantPlanting.Length)];
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
            _gameResultSource.PlayOneShot(_defeat);
        }

        public void PlayClickHit()
        {
            if (_clickHit == null || _clickHit.Length == 0) return;
            var clip = _clickHit[URandom.Range(0, _clickHit.Length)];
            if (clip == null) return;
            _sfxSource.pitch = URandom.Range(1f - _clickHitPitchVariation, 1f + _clickHitPitchVariation);
            _sfxSource.PlayOneShot(clip, _clickHitVolume);
            _sfxSource.pitch = 1f;
        }

        public void PlayEnemyAttack()
        {
            if (_enemyAttack == null || _enemyAttack.Length == 0) return;
            var clip = _enemyAttack[URandom.Range(0, _enemyAttack.Length)];
            if (clip != null) _sfxSource.PlayOneShot(clip, _enemyAttackVolume);
        }

        public void PlayTurretShoot()
        {
            if (_turretShoot == null || _turretShoot.Length == 0) return;
            var clip = _turretShoot[URandom.Range(0, _turretShoot.Length)];
            if (clip != null) _sfxSource.PlayOneShot(clip, _turretShootVolume);
        }

        public void PlayWrongNotify()
        {
            if (_wrongNotify == null) return;
            _sfxSource.PlayOneShot(_wrongNotify);
        }

        public void PlayBrainHit()
        {
            if (_brainHit == null || _brainHit.Length == 0) return;
            if (_brainHitSource.isPlaying) return;
            var clip = _brainHit[URandom.Range(0, _brainHit.Length)];
            if (clip == null) return;
            _brainHitSource.clip = clip;
            _brainHitSource.volume = _brainHitVolume;
            _brainHitSource.Play();
        }
    }
}
