using System.Collections.Generic;
using _Project.Code.Scripts.Data;
using _Project.Code.Scripts.EnemySystem;
using _Project.Code.Scripts.Game;
using _Project.Code.Scripts.InputResolverService;
using _Project.Code.Scripts.UIService;
using _Project.Code.Scripts.Configs;
using _Project.Code.Scripts.Garden;
using _Project.Code.Scripts.TaskSystem;
using _Project.Code.Scripts.Timer;
using _Project.Code.Scripts.CameraController;
using _Project.Code.Scripts.CraftSystem;
using _Project.Code.Scripts.UI;
using _Project.Code.Scripts.Cheats;
using _Project.Code.Scripts.Audio;
using _Project.Code.Scripts.GameOver;
using UnityEngine;
using _Project.Code.Scripts.BattleField;

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
        [SerializeField] private TaskConfig _taskConfig;
        [SerializeField] private GardenBed _gardenBed;
        [SerializeField] private GardenAttentionAnimator _gardenAttentionAnimator;
        [SerializeField] private WaveSpawner _waveSpawner;
        [SerializeField] private PlayerClickDamage _playerClickDamage;
        [SerializeField] private CameraEdgeScroll _cameraEdgeScroll;
        [SerializeField] private FieldSystem _fieldSystem;
        [SerializeField] private DefenseDragController _defenseDragController;
        [SerializeField] private DefenseShopView _defenseShopView;
        [SerializeField] private BrainView _brain;
        [SerializeField] private List<StoredResourceView> _storedResources;
        [SerializeField] private UpgradesTopButton _upgradesTopButton;
        [SerializeField] private CheatCompleteTask _cheatCompleteTask;
        [SerializeField] private AudioManager _audioManager;
        [SerializeField] private LoadingScreen _loadingScreen;
        private GameData _gameData;
        private ITimerService _timerService;
        private ITaskService _taskService;

        private void Awake() {
            _loadingScreen.gameObject.SetActive(true);
            
            InitConfig();
            _timerService = new TimerService();

            
            var manualUpdates = new List<IManualUpdate> {   _inputResolver, 
                                                            _timerService as IManualUpdate,
                                                            _craftStantionView,
                                                            _waveSpawner,};

            //Input
            _inputResolver.ManualAwake();
            //_cameraEdgeScroll.Initialize(_inputResolver);
            //UI
            _uiController.Initialize(_inputResolver);
            foreach (var storedResource in _storedResources) storedResource.Initialize();
            _upgradesTopButton.Initialize(_uiController);
            //Task and Craft
            _taskService = new TaskService(_gameConfig.TaskConfig.Tasks);
            _taskSystemView.ManualAwake(_taskService, _gameConfig.TaskIconConfig);
            _craftStantionView.ManualAwake(_taskService, _timerService, _gameConfig.ResourceIconConfig, _gameConfig.TaskIconConfig, _gardenAttentionAnimator, _gardenBed);
            //Garden
            _gardenBed.Initialize(_uiController, _gameConfig, _inputResolver, _timerService, _gardenAttentionAnimator);
            //Field
            _fieldSystem.Initialize(_gameConfig.FieldConfig);
            //Defense Shop
            _defenseShopView.Initialize(_gameConfig.DefenseShopConfig);


            //Enemies
            _waveSpawner.ManualAwake(_gameConfig.EnemyConfig, _gameConfig.WaveConfig);
            _playerClickDamage.ManualAwake(_inputResolver);
            //Game
            GameContext context = new GameContext(_brain, _taskService, _timerService, _defenseDragController);

            _gameController.ManualAwake(context);
            if (_cheatCompleteTask != null) _cheatCompleteTask.Initialize(_taskService);
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

    public class GameContext
    {
        public List<IManualUpdate> ManualUpdates { get; } = new();
        public IPlayerDamageEventProvider PlayerDamageEventProvider { get; }
        public ITaskService TaskService { get; }
        public ITimerService TimerService { get; }
        public DefenseDragController DefenseDragController { get; }

        public GameContext(IPlayerDamageEventProvider playerDamageEventProvider, ITaskService taskService, ITimerService timerService, DefenseDragController defenseDragController)
        {
            PlayerDamageEventProvider = playerDamageEventProvider;
            TaskService = taskService;
            TimerService = timerService;
            DefenseDragController = defenseDragController;
        }
    }
}