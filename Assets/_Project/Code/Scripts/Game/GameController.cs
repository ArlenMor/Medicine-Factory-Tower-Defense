using System.Collections.Generic;
using _Project.Code.Scripts.Data;
using _Project.Code.Scripts.Game.LvlController;
using UnityEngine;

namespace _Project.Code.Scripts.Game
{
    public class GameController : MonoBehaviour, IGamePauseHandler {
        private bool _paused;
        private LevelController _levelController;
        private int _currentLevel = 1;

        public void ManualAwake(List<IManualUpdate> manualUpdates) {
            _levelController = new LevelController(manualUpdates, this);
            _levelController.OnVictory += () => _currentLevel++;
        }

        public void SetPaused(bool paused)
        {
            _paused = paused;
        }

        public void Update()
        {
            GameData.Instance.Stats.TimePlayed += Time.deltaTime;

            if(_levelController.State == LevelState.NonPlaying)
            {
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