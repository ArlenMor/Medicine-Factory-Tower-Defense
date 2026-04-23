using System;
using _Project.Code.Scripts.Audio;
using _Project.Code.Scripts.EnemySystem;
using _Project.Code.Scripts.TaskSystem;

namespace _Project.Code.Scripts.GameOver
{
    public class LevelEndChecker
    {
        private readonly IPlayerDamageEventProvider _playerDamageEventProvider;
        private readonly ITaskService _taskService;
        private bool _gameOver;

        public event Action OnPlayerLose;

        public LevelEndChecker(IPlayerDamageEventProvider playerDamageEventProvider, ITaskService taskService)
        {
            _playerDamageEventProvider = playerDamageEventProvider;
            _taskService = taskService;

            _playerDamageEventProvider.OnDied += HandleDefeat;
            _taskService.OnTaskCompleted += HandleTaskCompleted;
        }

        private void OnDestroy()
        {
            _playerDamageEventProvider.OnDied -= HandleDefeat;
            _taskService.OnTaskCompleted -= HandleTaskCompleted;
        }

        private void HandleDefeat()
        {
            if (_gameOver) return;

            EndGame(false);
        }

        private void HandleTaskCompleted(Data.TaskData.TaskData _)
        {
            if (_gameOver) return;
            if (_taskService.CompletedTasksCount >= _taskService.GoalTaskIndex)
                EndGame(true);
        }

        private void EndGame(bool isVictory)
        {
            _gameOver = true;

            if (isVictory)
                AudioManager.Instance.PlayVictory();
            else
                AudioManager.Instance.PlayDefeat();

            OnPlayerLose?.Invoke();
        }
    }
}
