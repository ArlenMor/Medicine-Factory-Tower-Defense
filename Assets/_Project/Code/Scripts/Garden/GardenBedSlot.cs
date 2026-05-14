using System.Linq;
using _Project.Code.Scripts.Audio;
using _Project.Code.Scripts.Configs;
using _Project.Code.Scripts.Data;
using _Project.Code.Scripts.ServiceLocator;
using _Project.Code.Scripts.Timer;
using _Project.Code.Scripts.Tutorial;
using _Project.Code.Scripts.UI;
using _Project.Code.Scripts.UIService;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Code.Scripts.Garden
{
    public class GardenBedSlot: MonoBehaviour
    {
        [SerializeField] private Transform _plantParent;
        [SerializeField] private Transform _panelPosition;
        [SerializeField] private Plant[] _plantPrefabs;
        [SerializeField] private Transform _message;
        [SerializeField] private Animator _animator;
        [SerializeField] private ResourcesAnimatorWidget _resourcesAnimatorWidget;
        
        private GameConfig _config;
        private Transform _canvasParent;
        private ITimerService _timer;
        private Plant _plantInstance;
        private IPanelShower _panelShower;
        private bool _isOccupied;

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
                        var productivityMultiplier = GameData.Instance.UpgradesData[UpgradeType.Produce].Multiplier;
                        var amount = GetDefaultProductivity(resourceType) * Mathf.RoundToInt(productivityMultiplier);
                        GameData.Instance.AddResource(resourceType, amount);
                        GameData.Instance.Stats.ResourcesCollected++;
                        _resourcesAnimatorWidget.PlayAnimation(resourceType, amount);
                        AudioManager.Instance.PlayResourceGather();
                        if (S.TryGet<ITutorialService>(out var tutorial))
                            tutorial.NotifyEvent(TutorialEventType.ResourceHarvested);
                        Destroy(_plantInstance.gameObject);
                        HandleMassage(false);
                        _isOccupied = false;
                    }
                }
            }
            else
            {
                var settings = new PlantChoosePanelSettings()
                {
                    Callback = OnPlantChosen,
                    Position = _panelPosition.position,
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
                Destroy(_plantInstance.gameObject);
                _plantInstance = null;
            }
            _isOccupied = false;
        }

        private void HandleMassage(bool isShown)
        {
            _message.gameObject.SetActive(isShown);
            _animator.SetBool("OnShown", isShown);
        }
        
        private void OnPlantGrown()
        {
            HandleMassage(true);
        }
        
        private void OnPlantChosen(PlantType plantType)
        {
            var plant = _plantPrefabs.FirstOrDefault(plant => plant.Type == plantType);
            _plantInstance = Instantiate(plant, _plantParent);
            _plantInstance.Initialize(_config, _timer, OnPlantGrown);
            _isOccupied = true;
            GameData.Instance.Stats.PlantsPlanted++;
            AudioManager.Instance.PlayPlantPlanting();
            _panelShower.HideView(PanelType.PlantPanelInfo);
            if (S.TryGet<ITutorialService>(out var tutorial))
                tutorial.NotifyEvent(TutorialEventType.PlantPlanted);
        }
        
        private int GetDefaultProductivity(ResourceType resourceType) => 
            _config.GardenConfig.GetGrowableResourceData(resourceType).DefaultProductivity;
    }
}