using System.Linq;
using _Project.Code.Scripts.Audio;
using _Project.Code.Scripts.Configs;
using _Project.Code.Scripts.Data;
using _Project.Code.Scripts.Game;
using _Project.Code.Scripts.ServiceLocator;
using _Project.Code.Scripts.Timer;
using _Project.Code.Scripts.Tutorial;
using _Project.Code.Scripts.UI;
using _Project.Code.Scripts.ServiceLocator;
using _Project.Code.Scripts.Stats;
using _Project.Code.Scripts.UIService;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.Scripts.Garden
{
    public class GardenBedSlot: MonoBehaviour
    {
        private const string TutorialCurrentPlantedPlantTargetId = "TutorCurrentPlantedPlant";
        private const string TutorialRemovePlantPanelTargetId = "TutorRemovePlantPanel";

        [SerializeField] private Transform _plantParent;
        [SerializeField] private Transform _selectPanelPosition;
        [SerializeField] private Transform _removePanelPosition;
        [SerializeField] private Plant[] _plantPrefabs;
        [SerializeField] private Transform _message;
        [SerializeField] private SpriteRenderer _messageIcon;
        [SerializeField] private Animator _animator;
        //TODO: fix after
        [SerializeField] private ResourcesAnimatorWidget _resourcesAnimatorWidgetCrystal;
        [SerializeField] private ResourcesAnimatorWidget _resourcesAnimatorWidgetPolymer;
        [SerializeField] private ResourcesAnimatorWidget _resourcesAnimatorWidgetNanoGel;

        
        private GameConfig _config;
        private Transform _canvasParent;
        private ITimerService _timer;
        private Plant _plantInstance;
        private IPanelShower _panelShower;
        private bool _isOccupied;
        private static GardenBedSlot s_removePanelOwner;
        private static Plant s_removePanelPlant;

        private bool _isPausedByTutorialRemove;

        public bool IsOccupied => _isOccupied;
        
        private BasePanel _panel;
        
        public void Initialize(
            IPanelShower panelShower,
            GameConfig config, 
            ITimerService timer,
            Transform canvasParent)
        {
            _panelShower = panelShower;
            _config = config;
            _timer = timer;
            _canvasParent = canvasParent;
        }

        public void OnClicked()
        {
            if (_isOccupied)
            {
                if (_plantInstance != null)
                {
                    if (_plantInstance.IsGrown)
                    {
                        var resourceType = _plantInstance.Type.GetResourceType();
                        var defaultProductivity = GetDefaultProductivity(resourceType);
                        var isDoubled = GameData.Instance.RollProduceDoubleWithPity();
                        var amount = defaultProductivity * (isDoubled ? 2 : 1);
                        GameData.Instance.AddResource(resourceType, amount);
                        GameData.Instance.Stats.ResourcesCollected++;
                        switch (resourceType)
                        {
                            case ResourceType.Crystal:
                                _resourcesAnimatorWidgetCrystal.PlayAnimation(resourceType, amount);
                                break;
                            case ResourceType.Polymer:
                                _resourcesAnimatorWidgetPolymer.PlayAnimation(resourceType, amount);
                                break;
                            case ResourceType.NanoGel:
                                _resourcesAnimatorWidgetNanoGel.PlayAnimation(resourceType, amount);
                                break;
                        }
                        AudioManager.Instance.PlayResourceGather();
                        if (S.TryGet<GameplayLogger>(out var harvLog))
                            harvLog.LogHarvest(resourceType.ToString(), amount, isDoubled);
                        var plantWorldPos = _plantInstance.transform.position;
                        if (S.TryGet<FlyingIconService>(out var flyService))
                        {
                            if (isDoubled)
                                flyService.FlyResourceBurst(plantWorldPos, resourceType, 2);
                            else
                                flyService.FlyResource(plantWorldPos, resourceType);
                        }
                        else
                            Debug.LogWarning("[GardenBedSlot] FlyingIconService not found in S. " +
                                "Assign _flyingIconService in Bootstrap and check scene setup.");
                        if (S.TryGet<ITutorialService>(out var tutorial))
                            tutorial.NotifyEvent(TutorialEventType.ResourceHarvested);
                        UnregisterCurrentPlantedPlantTutorialTargetIfCurrentPlant();
                        Destroy(_plantInstance.gameObject);
                        HandleMassage(false);
                        _isOccupied = false;
                    }
                    else
                    {
                        ShowRemovePlantPanel();
                    }
                }
            }
            else
            {
                var settings = new PlantChoosePanelSettings()
                {
                    Callback = OnPlantChosen,
                    Position = _selectPanelPosition.position,
                };
                _panelShower.ShowView(PanelType.PlantPanelInfo, settings, _canvasParent);
            }
        }

        public void ManualPlantChoose(PlantType plantType, bool isAlreadyGrown)
        {
            var plant = _plantPrefabs.FirstOrDefault(plant => plant.Type == plantType);
            _plantInstance = Instantiate(plant, _plantParent);
            _plantInstance.Initialize(_config, _timer, OnPlantGrown, isAlreadyGrown);
            _isOccupied = true;
        }

        public void Clear()
        {
            if (_plantInstance != null)
            {
                UnregisterCurrentPlantedPlantTutorialTargetIfCurrentPlant();
                Destroy(_plantInstance.gameObject);
                _plantInstance = null;
            }
            ReleaseTutorialRemovePause();
            _message.gameObject.SetActive(false);
            _isOccupied = false;
        }

        private void OnDestroy()
        {
            ReleaseTutorialRemovePause();
        }

        private void HandleMassage(bool isShown)
        {
            _message.gameObject.SetActive(isShown);
            _animator.SetBool("OnShown", isShown);
        }
        
        private void OnPlantGrown()
        {
            if (_messageIcon != null)
            {
                var resourceType = _plantInstance.Type.GetResourceType();
                _messageIcon.sprite = GameData.Instance.GameConfig.ResourceIconConfig.GetIcon(resourceType);
            }
            HandleMassage(true);
            if (OwnsRemovePanelForCurrentPlant())
            {
                _panelShower.HideView(PanelType.RemovePlant);
                ReleaseRemovePanelOwnership();
            }
        }
        
        private void OnRemovePlant()
        {
            UnregisterCurrentPlantedPlantTutorialTargetIfCurrentPlant();
            Destroy(_plantInstance.gameObject);
            _plantInstance = null;
            _isOccupied = false;
            _panelShower.HideView(PanelType.RemovePlant);
            ReleaseRemovePanelOwnership();
            ReleaseTutorialRemovePause();
            if (S.TryGet<ITutorialService>(out var tutorial))
                tutorial.NotifyEvent(TutorialEventType.PlantRemoved);
        }

        private void OnPlantChosen(PlantType plantType)
        {
            var plant = _plantPrefabs.FirstOrDefault(plant => plant.Type == plantType);
            _plantInstance = Instantiate(plant, _plantParent);
            _plantInstance.Initialize(_config, _timer, OnPlantGrown);
            _isOccupied = true;
            RegisterCurrentPlantedPlantTutorialTarget();
            GameData.Instance.Stats.PlantsPlanted++;
            if (S.TryGet<GameplayLogger>(out var plantLog))
                plantLog.LogPlant(plantType);
            AudioManager.Instance.PlayPlantPlanting();
            _panelShower.HideView(PanelType.PlantPanelInfo);
            if (S.TryGet<ITutorialService>(out var tutorial))
            {
                tutorial.NotifyEvent(TutorialEventType.PlantPlanted);
                TryOpenTutorialRemovePanelAfterPlanting(tutorial);
            }
        }
        
        private int GetDefaultProductivity(ResourceType resourceType) => 
            _config.GardenConfig.GetGrowableResourceData(resourceType).DefaultProductivity;

        private void ClaimRemovePanelOwnership()
        {
            s_removePanelOwner = this;
            s_removePanelPlant = _plantInstance;
        }

        private bool OwnsRemovePanelForCurrentPlant()
        {
            return s_removePanelOwner == this && s_removePanelPlant == _plantInstance;
        }

        private void ReleaseRemovePanelOwnership()
        {
            if (s_removePanelOwner != this) return;

            s_removePanelOwner = null;
            s_removePanelPlant = null;
        }

        private void ShowRemovePlantPanel()
        {
            ClaimRemovePanelOwnership();
            var settings = new RemovePlantPanelSettings()
            {
                Callback = OnRemovePlant,
                Position = _removePanelPosition.position,
                TutorialTargetId = TutorialRemovePlantPanelTargetId,
            };
            _panelShower.ShowView(PanelType.RemovePlant, settings, _canvasParent);
        }

        private void RegisterCurrentPlantedPlantTutorialTarget()
        {
            if (_plantInstance == null) return;
            if (!S.TryGet<ITutorialTargetRegistry>(out var registry)) return;

            registry.Register(TutorialCurrentPlantedPlantTargetId, _plantInstance.transform);
        }

        private void UnregisterCurrentPlantedPlantTutorialTargetIfCurrentPlant()
        {
            if (_plantInstance == null) return;
            if (!S.TryGet<ITutorialTargetRegistry>(out var registry)) return;

            if (registry.TryGet(TutorialCurrentPlantedPlantTargetId, out var target) && target == _plantInstance.transform)
                registry.Unregister(TutorialCurrentPlantedPlantTargetId);
        }

        private void TryOpenTutorialRemovePanelAfterPlanting(ITutorialService tutorial)
        {
            if (tutorial == null || !tutorial.IsActive) return;
            if (_plantInstance == null || _plantInstance.IsGrown) return;

            var activeStep = tutorial.ActiveStep;
            if (activeStep == null || activeStep.Trigger != StepTrigger.PlantRemoved)
                return;

            ShowRemovePlantPanel();
            SetTutorialRemovePause();
        }

        private void SetTutorialRemovePause()
        {
            if (_isPausedByTutorialRemove) return;
            if (!S.TryGet<IGamePauseHandler>(out var pauseHandler)) return;

            _isPausedByTutorialRemove = true;
            pauseHandler.SetPaused(true);
        }

        private void ReleaseTutorialRemovePause()
        {
            if (!_isPausedByTutorialRemove) return;
            if (!S.TryGet<IGamePauseHandler>(out var pauseHandler)) return;

            _isPausedByTutorialRemove = false;
            pauseHandler.SetPaused(false);
        }
    }
}