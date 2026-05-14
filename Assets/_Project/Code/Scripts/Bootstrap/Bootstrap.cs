using System.Collections.Generic;
using _Project.Code.Scripts.Data;
using _Project.Code.Scripts.EnemySystem;
using _Project.Code.Scripts.Game;
using _Project.Code.Scripts.InputResolverService;
using _Project.Code.Scripts.UIService;
using _Project.Code.Scripts.Garden;
using _Project.Code.Scripts.TaskSystem;
using _Project.Code.Scripts.Timer;
using _Project.Code.Scripts.CraftSystem;
using _Project.Code.Scripts.UI;
using _Project.Code.Scripts.Cheats;
using _Project.Code.Scripts.Audio;
using _Project.Code.Scripts.Tutorial;
using UnityEngine;
using _Project.Code.Scripts.BattleField;
using _Project.Code.Scripts.ServiceLocator;

namespace _Project.Code.Scripts.Bootstrap
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private InputResolver _inputResolver;
        [SerializeField] private UIController _uiController;
        [SerializeField] private GameController _gameController;
        [SerializeField] private TaskSystemView _taskSystemView;
        [SerializeField] private CraftStantionView _craftStantionView;
        [SerializeField] private GameConfig _gameConfig;
        [SerializeField] private GardenBed _gardenBed;
        [SerializeField] private GardenAttentionAnimator _gardenAttentionAnimator;
        [SerializeField] private DefenseDragController _defenseDragController;
        [SerializeField] private WaveSpawner _waveSpawner;
        [SerializeField] private PlayerClickDamage _playerClickDamage;
        [SerializeField] private FieldSystem _fieldSystem;
        [SerializeField] private DefenseShopView _defenseShopView;
        [SerializeField] private BrainView _brain;
        [SerializeField] private List<StoredResourceView> _storedResources;
        [SerializeField] private UpgradesTopButton _upgradesTopButton;
        [SerializeField] private CheatCompleteTask _cheatCompleteTask;
        [SerializeField] private AudioManager _audioManager;
        [SerializeField] private LoadingScreen _loadingScreen;
        [SerializeField] private TutorialOverlayView _tutorialOverlayView;
        private GameData _gameData;
        private ITimerService _timerService;
        private ITaskService _taskService;

        private void Awake() {
            _loadingScreen.gameObject.SetActive(true);
            
            S.Reset();

            InitConfig();
            _timerService = new TimerService();
            S.Register<ITimerService>(_timerService);

            
            var manualUpdates = new List<IManualUpdate> {   _inputResolver, 
                                                            _timerService as IManualUpdate,
                                                            _craftStantionView,
                                                            _waveSpawner,};

            //Input
            _inputResolver.ManualAwake();
            S.Register<IInputResolver>(_inputResolver);
            //UI
            _uiController.Initialize(_inputResolver);
            S.Register<IPanelShower>(_uiController);
            foreach (var storedResource in _storedResources) storedResource.Initialize();
            _upgradesTopButton.Initialize(_uiController);
            //Task and Craft
            _taskService = new TaskService();
            S.Register<ITaskService>(_taskService);
            _taskSystemView.ManualAwake(_taskService, _gameConfig.TaskIconConfig);
            _craftStantionView.ManualAwake(_taskService, _timerService, _gameConfig.ResourceIconConfig, _gameConfig.TaskIconConfig, _gardenAttentionAnimator, _gardenBed);
            //Garden
            _gardenBed.Initialize(_uiController, _gameConfig, _inputResolver, _timerService, _gardenAttentionAnimator);
            S.Register<GardenBed>(_gardenBed);
            //Field
            _fieldSystem.Initialize(_gameConfig.FieldConfig);
            S.Register<IFieldSystem>(_fieldSystem);

            //Defense Shop
            _defenseShopView.Initialize(_gameConfig.DefenseShopConfig);

            //Enemies
            _waveSpawner.ManualAwake(_gameConfig.EnemyConfig);
            S.Register<WaveSpawner>(_waveSpawner);
            _playerClickDamage.ManualAwake(_inputResolver);
            //Game
            S.Register<IPlayerDamageEventProvider>(_brain);

            S.Register<IDefenseDragController>(_defenseDragController);

            _gameController.ManualAwake(manualUpdates);
            S.Register<IGamePauseHandler>(_gameController);
            if (_cheatCompleteTask != null) _cheatCompleteTask.Initialize(_taskService);
            //Tutorial
            var tutorialService = new TutorialService();
            S.Register<ITutorialService>(tutorialService);
            if (_tutorialOverlayView != null)
                _tutorialOverlayView.Initialize(tutorialService);
            //Audio
            _audioManager.PlayMainTheme();
            //Loading
            _loadingScreen.Hide();
        }

        private void Start() {
            
        }
        
        private void InitConfig() {
            _gameData = new GameData(_gameConfig, _mainCamera);

            _gameData.Initialize();
        }
    }
}