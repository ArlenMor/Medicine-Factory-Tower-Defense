using System.Collections;
using _Project.Code.Scripts.Audio;
using _Project.Code.Scripts.EnemySystem;
using _Project.Code.Scripts.Game;
using _Project.Code.Scripts.TaskSystem;
using _Project.Code.Scripts.UIPanels.Settings;
using _Project.Code.Scripts.UIService;
using UnityEngine;

namespace _Project.Code.Scripts.GameOver
{
    public class GameOverService
    {
        private readonly Brain _brain;
        private readonly ITaskService _taskService;
        private readonly IPanelShower _panelShower;
        private readonly GameController _gameController;
        private bool _gameOver;

        private const float ShowDelay = 1.5f;

        public GameOverService(Brain brain, ITaskService taskService, IPanelShower panelShower, GameController gameController)
        {
            _brain = brain;
            _taskService = taskService;
            _panelShower = panelShower;
            _gameController = gameController;

            _brain.OnDied += HandleDefeat;
            _taskService.OnTaskCompleted += HandleTaskCompleted;
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
            _brain.OnDied -= HandleDefeat;
            _taskService.OnTaskCompleted -= HandleTaskCompleted;

            if (isVictory)
                AudioManager.Instance.PlayVictory();
            else
                AudioManager.Instance.PlayDefeat();

            _gameController.StartCoroutine(EndGameRoutine(isVictory));
        }

        private IEnumerator EndGameRoutine(bool isVictory)
        {
            yield return new WaitForSeconds(ShowDelay);

            _gameController.SetPaused(true);
            _panelShower.ShowView(PanelType.GameOver, new GameOverPanelSettings { IsVictory = isVictory });
        }
    }
}
