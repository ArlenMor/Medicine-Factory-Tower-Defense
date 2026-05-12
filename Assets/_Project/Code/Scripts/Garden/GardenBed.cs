using System.Collections.Generic;
using System.Linq;
using _Project.Code.Scripts.Configs;
using _Project.Code.Scripts.InputResolverService;
using _Project.Code.Scripts.Timer;
using _Project.Code.Scripts.UI;
using _Project.Code.Scripts.UIService;
using UnityEngine;

namespace _Project.Code.Scripts.Garden
{
    public class GardenBed: MonoBehaviour
    {
        [SerializeField] private Transform _canvasParent;
        [SerializeField] private GardenBedSlot[] _slots;
        
        private IPanelShower _panelShower;
        private IInputResolver _inputResolver;
        private ITimerService _timerService;
        private GameConfig _gameConfig;
        private GardenAttentionAnimator _attentionAnimator;

        public int OccupiedSlotsCount => _slots.Count(s => s.IsOccupied);

        public void Initialize(IPanelShower panelShower, GameConfig gameConfig, IInputResolver inputResolver, ITimerService timerService, GardenAttentionAnimator attentionAnimator = null)
        {
            _panelShower = panelShower;
            _inputResolver = inputResolver;
            _gameConfig = gameConfig;
            _timerService = timerService;
            _attentionAnimator = attentionAnimator;
            
            _inputResolver.OnPointerDown += OnPointerDown;
            
            foreach (var slot in _slots)
            {
                slot.Initialize(_panelShower, _gameConfig, _timerService, _canvasParent);
            }
        }

        public void StartLevel(List<SlotPlantSetup> initialPlants)
        {
            foreach (var slot in _slots)
                slot.Clear();

            foreach (var setup in initialPlants)
            {
                if (setup.SlotIndex >= 0 && setup.SlotIndex < _slots.Length)
                    _slots[setup.SlotIndex].ManualPlantChoose(setup.PlantType, setup.IsAlreadyGrown);
            }
        }

        private void OnPointerDown(InputEventData inputData)
        {
            if (inputData.Target == InputTarget.Canvas)
            {
                if (inputData.HitObject != null)
                {
                    if (!inputData.HitObject.TryGetComponent<PlantChooseView>(out var view))
                    {
                        _panelShower.HideView(PanelType.PlantPanelInfo);
                    }
                }
            }
            else
            {
                _panelShower.HideView(PanelType.PlantPanelInfo);
            }
            
            if (inputData.Target != InputTarget.World) return;

            if (inputData.HitObject == null) return;
            
            if (inputData.HitObject.TryGetComponent<GardenBedSlot>(out var gardenBedSlot))
            {
                gardenBedSlot.OnClicked();
                _attentionAnimator?.StopAttention();
            }
        }

        private void OnDestroy()
        {
            _inputResolver.OnPointerDown -= OnPointerDown;
        }
    }
}