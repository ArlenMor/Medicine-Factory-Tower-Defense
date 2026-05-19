using System;
using _Project.Code.Scripts.Audio;
using _Project.Code.Scripts.EnemySystem;
using _Project.Code.Scripts.ServiceLocator;
using _Project.Code.Scripts.TaskSystem;

namespace _Project.Code.Scripts.GameOver
{
    public class LevelEndChecker
    {
        private readonly IPlayerDamageEventProvider _playerDamageEventProvider;
        private readonly ITaskService _taskService;
        private bool _gameOver;

        public event Action<bool> OnLevelEnd;

        public LevelEndChecker()
        {
            _gameOver = false;

            _playerDamageEventProvider = S.Get<IPlayerDamageEventProvider>();
            _taskService = S.Get<ITaskService>();

            _playerDamageEventProvider.OnDied += HandleDefeat;
            _taskService.OnAllTasksCompleted += HandleAllTasksCompleted;
        }

        public void Reset()
        {
            _gameOver = false;
        }

        public void ForceVictory()
        {
            if (_gameOver) return;
            EndGame(true);
        }

        private void OnDestroy()
        {
            _playerDamageEventProvider.OnDied -= HandleDefeat;
            _taskService.OnAllTasksCompleted -= HandleAllTasksCompleted;
        }

        private void HandleDefeat()
        {
            if (_gameOver) return;

            EndGame(false);
        }

        private void HandleAllTasksCompleted()
        {
            if (_gameOver) return;

            EndGame(true);
        }

        private void EndGame(bool isVictory)
        {
            _gameOver = true;

            if (isVictory)
                AudioManager.Instance.PlayVictory();
            else
                AudioManager.Instance.PlayDefeat();

            OnLevelEnd?.Invoke(isVictory);
        }
    }
}
