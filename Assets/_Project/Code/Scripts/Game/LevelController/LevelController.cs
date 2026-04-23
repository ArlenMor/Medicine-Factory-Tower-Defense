using System.Collections;
using System.Collections.Generic;
using _Project.Code.Scripts.BattleField;
using _Project.Code.Scripts.Data;
using _Project.Code.Scripts.EnemySystem;
using _Project.Code.Scripts.GameOver;
using _Project.Code.Scripts.TaskSystem;
using _Project.Code.Scripts.UIPanels.Settings;
using _Project.Code.Scripts.UIService;
using UnityEngine;

namespace _Project.Code.Scripts.Game.LvlController
{
    public class LevelController: IManualUpdate, IManualUpdateRegistrar {
        
        private readonly List<IManualUpdate> _pendingAdd = new();
        private readonly List<IManualUpdate> _pendingRemove = new();
        private DefenseDragController _defenseDragController;
        private LevelEndChecker _levelEndChecker;
        private const float ShowDelay = 1.5f;

        private List<IManualUpdate> _manualUpdates;
        private readonly IGamePauseHandler _gamePauseHandler;
        private readonly IPanelShower _panelShower;

        public LevelController(List<IManualUpdate> manualUpdates, IPlayerDamageEventProvider playerDamageEventProvider, ITaskService taskService, IGamePauseHandler gamePauseHandler, IPanelShower panelShower)
        {
            _manualUpdates = manualUpdates;
            _levelEndChecker = new LevelEndChecker(playerDamageEventProvider, taskService);
            _levelEndChecker.OnPlayerLose += () => EndGameRoutine(false);
            _gamePauseHandler = gamePauseHandler;
            _panelShower = panelShower;

            _defenseDragController.Initialize(_inputResolver, _fieldSystem, GameData.Instance.GameConfig.DefenseShopConfig, _gameController);

        }

        #region IManualUpdate

        public void ManualUpdate(float deltaTime)
        {
            if (_pendingAdd.Count > 0)
            {
                _manualUpdates.AddRange(_pendingAdd);
                _pendingAdd.Clear();
            }

            if (_pendingRemove.Count > 0)
            {
                foreach (var item in _pendingRemove)
                    _manualUpdates.Remove(item);
                _pendingRemove.Clear();
            }
            
            foreach (var manualUpdate in _manualUpdates)
                manualUpdate.ManualUpdate(deltaTime);
        }

        #endregion

        #region IManualUpdateRegistrar
        public void Register(IManualUpdate manualUpdate)
        {   
            _pendingAdd.Add(manualUpdate);
        }

        public void Unregister(IManualUpdate manualUpdate)
        {
            _pendingRemove.Add(manualUpdate);
        }
        
        #endregion

        private IEnumerator EndGameRoutine(bool isVictory)
        {
            yield return new WaitForSeconds(ShowDelay);

            _gamePauseHandler.SetPaused(true);
            _panelShower.ShowView(PanelType.GameOver, new GameOverPanelSettings { IsVictory = isVictory });
        }
    }
}