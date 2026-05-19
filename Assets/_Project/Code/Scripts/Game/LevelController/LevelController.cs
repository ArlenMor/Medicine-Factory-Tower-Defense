using System;
using System.Collections.Generic;
using _Project.Code.Scripts.Audio;
using _Project.Code.Scripts.CraftSystem;
using _Project.Code.Scripts.Data;
using _Project.Code.Scripts.Data.TaskData;
using _Project.Code.Scripts.EnemySystem;
using _Project.Code.Scripts.Garden;
using _Project.Code.Scripts.GameOver;
using _Project.Code.Scripts.ServiceLocator;
using _Project.Code.Scripts.TaskSystem;
using _Project.Code.Scripts.Tutorial;
using _Project.Code.Scripts.UIPanels.Settings;
using _Project.Code.Scripts.UIService;

namespace _Project.Code.Scripts.Game.LvlController
{
    public enum LevelState
    {
        NonPlaying = 0,
        Playing = 1,
        Win = 2,
        Loss = 3
    }

    public class LevelController: IManualUpdate, IManualUpdateRegistrar {
        
        private readonly List<IManualUpdate> _pendingAdd = new();
        private readonly List<IManualUpdate> _pendingRemove = new();
        private bool _pendingMonoBehaviourCleanup;
        private IDefenseDragController _defenseDragController;
        private LevelEndChecker _levelEndChecker;
        private WaveSpawner _waveSpawner;
        private ITaskService _taskService;
        private GardenBed _gardenBed;
        private IFieldSystem _fieldSystem;
        private BrainView _brain;
        private CraftStantionView _craftStantionView;
        private const float ShowDelay = 1.5f;

        private List<IManualUpdate> _manualUpdates;
        private readonly IGamePauseHandler _gamePauseHandler;
        private readonly IPanelShower _panelShower;
        private int _currentLevelIndex;

        public LevelState State { get; private set; } = LevelState.NonPlaying;

        public event Action OnVictory;

        public LevelController(List<IManualUpdate> manualUpdates, IGamePauseHandler gamePauseHandler)
        {
            _manualUpdates = manualUpdates;
            _levelEndChecker = new LevelEndChecker();
            _gamePauseHandler = gamePauseHandler;
            _defenseDragController = S.Get<IDefenseDragController>();
            _panelShower = S.Get<IPanelShower>();
            _waveSpawner = S.Get<WaveSpawner>();
            _taskService = S.Get<ITaskService>();
            _gardenBed = S.Get<GardenBed>();
            _fieldSystem = S.Get<IFieldSystem>();
            _brain = S.Get<BrainView>();
            _craftStantionView = S.Get<CraftStantionView>();

            _defenseDragController.Initialize(GameData.Instance.GameConfig.DefenseShopConfig, this);

            _levelEndChecker.OnLevelEnd += (isVictory) => EndLevelRoutine(isVictory);
        }

        #region IManualUpdate

        public void ManualUpdate(float deltaTime)
        {
            if (_pendingMonoBehaviourCleanup)
            {
                _manualUpdates.RemoveAll(u => u is UnityEngine.MonoBehaviour mb && mb == null);
                _pendingAdd.RemoveAll(u => u is UnityEngine.MonoBehaviour mb && mb == null);
                _pendingMonoBehaviourCleanup = false;
            }

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

        #region Level Setup logic

        public void RequestRestart()
        {
            if (State == LevelState.Playing)
                State = LevelState.NonPlaying;
        }

        public void ForceVictory()
        {
            if (State == LevelState.Playing)
                _levelEndChecker.ForceVictory();
        }

        public void StartLevel(int levelIndex)
        {
            _currentLevelIndex = levelIndex;
            var gameConfig = GameData.Instance.GameConfig;
            var levelConfig = gameConfig.GetLevel(levelIndex);
            GameData.Instance.ResetResources(levelConfig.StartCredits);
            GameData.Instance.ResetUpgrades();
            _craftStantionView.Reset();
            _fieldSystem.Reset();
            _brain.Reset();
            _pendingMonoBehaviourCleanup = true;
            _waveSpawner.StartLevel(levelConfig.WaveConfig, levelConfig.PauseSpawnDuringTutorial);
            _taskService.Reset(BuildTaskList(levelIndex, gameConfig));
            _gardenBed.StartLevel(levelConfig.InitialPlants);
            _levelEndChecker.Reset();
            AudioManager.Instance.PlayMainTheme();
            State = LevelState.Playing;

            if (levelConfig.TutorialSequence != null && S.TryGet<ITutorialService>(out var tutorial))
                tutorial.StartSequence(levelConfig.TutorialSequence);
        }

        private static List<TaskData> BuildTaskList(int levelIndex, GameConfig gameConfig)
        {
            var entries = gameConfig.LevelOrdersConfig?.Entries;
            var allTasks = gameConfig.TaskConfig?.Tasks;

            if (entries == null || allTasks == null)
                return new List<TaskData>();

            var levelEntries = new List<LevelOrderEntry>();
            foreach (var entry in entries)
            {
                if (entry.LevelId == levelIndex)
                    levelEntries.Add(entry);
            }

            levelEntries.Sort((a, b) => a.OrderIndex.CompareTo(b.OrderIndex));

            var result = new List<TaskData>(levelEntries.Count);
            foreach (var entry in levelEntries)
            {
                var task = allTasks.Find(t => (int)t.ResultType == entry.OrderId);
                result.Add(task);
            }

            return result;
        }


        #endregion

        private void EndLevelRoutine(bool isVictory)
        {
            //TODO: add coroutimeService 
            //yield return new WaitForSeconds(ShowDelay);

            State = isVictory ? LevelState.Win : LevelState.Loss;
            if (isVictory)
                OnVictory?.Invoke();

            _panelShower.ShowView(PanelType.GameOver, new GameOverPanelSettings { IsVictory = isVictory, LevelIndex = _currentLevelIndex }).OnClose += () =>
            {
                State = LevelState.NonPlaying;
            };
        }
    }
}