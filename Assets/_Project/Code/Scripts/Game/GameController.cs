using System.Collections.Generic;
using _Project.Code.Scripts.Cheats;
using _Project.Code.Scripts.Data;
using _Project.Code.Scripts.Game.LvlController;
using _Project.Code.Scripts.Localization;
using _Project.Code.Scripts.ServiceLocator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.Scripts.Game
{
    public class GameController : MonoBehaviour, IGamePauseHandler {
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private Button _restartButton;
        [SerializeField] private CheatSkipLevel _cheatSkipLevel;

        private bool _paused;
        private LevelController _levelController;
        private int _currentLevel = 1;

        public void ManualAwake(List<IManualUpdate> manualUpdates) {
            _levelController = new LevelController(manualUpdates, this);
            _levelController.OnVictory += () => _currentLevel++;
            _restartButton?.onClick.AddListener(OnRestartPressed);
            _cheatSkipLevel?.Initialize(_levelController);
            S.Get<LocalizationService>().OnLocaleChanged += UpdateLevelText;
        }

        private void OnDestroy()
        {
            _restartButton?.onClick.RemoveListener(OnRestartPressed);
            if (S.TryGet<LocalizationService>(out var loc))
                loc.OnLocaleChanged -= UpdateLevelText;
        }

        private void OnRestartPressed()
        {
            _levelController.RequestRestart();
        }

        public void SetPaused(bool paused)
        {
            _paused = paused;
        }

        public void StartNewGame()
        {
            _currentLevel = 1;
            GameData.Instance.Stats.Reset();
            _levelController.AutoStartTutorial = true;
            _levelController.RequestRestart();
        }

        public void StartTutorial()
        {
            _levelController.StartTutorial();
        }

        public void SetAutoStartTutorial()
        {
            _levelController.AutoStartTutorial = true;
        }

        private void UpdateLevelText()
        {
            if (_levelText == null) return;
            var totalLevels = GameData.Instance.GameConfig.Levels.Count;
            _levelText.text = S.Get<LocalizationService>().GetString("level_label", _currentLevel, totalLevels);
        }

        public void Update()
        {
            GameData.Instance.Stats.TimePlayed += Time.deltaTime;

            if(_levelController.State == LevelState.NonPlaying)
            {
                UpdateLevelText();
                _levelController.StartLevel(_currentLevel);
            }
            else if (_levelController.State == LevelState.Playing)
            {
                if (_paused) return;
                _levelController.ManualUpdate(Time.deltaTime);
            }
        }
    }
}