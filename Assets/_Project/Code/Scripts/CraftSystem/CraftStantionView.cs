using System.Collections.Generic;
using _Project.Code.Scripts.Configs;
using _Project.Code.Scripts.Data;
using _Project.Code.Scripts.Data.TaskData;
using _Project.Code.Scripts.Garden;
using _Project.Code.Scripts.ServiceLocator;
using _Project.Code.Scripts.TaskSystem;
using _Project.Code.Scripts.Timer;
using _Project.Code.Scripts.Tutorial;
using _Project.Code.Scripts.UI;
using DG.Tweening;
using TMPro;
using _Project.Code.Scripts.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.Scripts.CraftSystem
{
    public class CraftStantionView : MonoBehaviour, IManualUpdate
    {
        [SerializeField] private ResourceView _resourceSlotPrefab;
        [SerializeField] private Transform _resourceSlotsContainer;
        [SerializeField] private Button _craftButton;
        [SerializeField] private Image _taskIcon;
        [SerializeField] private RectTransform _animRectTransform;
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private ResourcesAnimatorWidget _resourcesAnimatorWidget;


        [Header("Shake Settings")]
        [SerializeField] private float _shakeDuration = 0.3f;
        [SerializeField] private float _shakeStrength = 0.2f;

        [Header("Craft Animation Settings")]
        [SerializeField] private float _craftScaleAmount = 1.08f;
        [SerializeField] private float _craftScaleDuration = 0.4f;
        [SerializeField] private float _bumpScale = 1.2f;
        [SerializeField] private float _bumpDuration = 0.25f;
        [SerializeField] private float _wiggleDuration = 0.18f;
        [SerializeField] private float _pressScale = 0.88f;
        [SerializeField] private float _pressDuration = 0.18f;

        private ITaskService _taskService;
        private ITimerService _timerService;
        private ResourceIconConfig _resourceIconConfig;
        private TaskIconConfig _taskIconConfig;
        private GardenAttentionAnimator _gardenAttentionAnimator;
        private GardenBed _gardenBed;
        private readonly List<ResourceView> _spawnedSlots = new();
        private readonly List<(ResourceType type, int cost)> _activeCosts = new();
        private TimerHandle _craftTimerHandle;
        private bool _isCrafting;
        private Tween _craftScaleTween;
        private float _craftTimeRemaining;
        private float _craftTotalTime;
        private Sequence _readyWiggleTween;
        private bool _wasReady;
        private bool _firstCraftStarted;
        private bool _firstOrderCompleted;

        public void ManualAwake(ITaskService taskService, ITimerService timerService, ResourceIconConfig resourceIconConfig, TaskIconConfig taskIconConfig, GardenAttentionAnimator gardenAttentionAnimator = null, GardenBed gardenBed = null)
        {
            _taskService = taskService;
            _timerService = timerService;
            _resourceIconConfig = resourceIconConfig;
            _taskIconConfig = taskIconConfig;
            _gardenAttentionAnimator = gardenAttentionAnimator;
            _gardenBed = gardenBed;

            _taskService.OnTaskStarted += OnTaskStarted;
            _craftButton.onClick.AddListener(OnCraftClicked);

            if (_taskService.HasActiveTask)
                OnTaskStarted(_taskService.CurrentTask.Value);
        }

        public void ManualUpdate(float deltaTime)
        {
            if (_activeCosts.Count > 0)
                RefreshSlots();

            if (_isCrafting)
            {
                _craftTimeRemaining -= deltaTime;
                if (_craftTimeRemaining < 0f) _craftTimeRemaining = 0f;
                UpdateTimerText(_craftTimeRemaining);
            }
        }

        private void OnDestroy()
        {
            if (_taskService != null)
                _taskService.OnTaskStarted -= OnTaskStarted;

            _craftButton.onClick.RemoveListener(OnCraftClicked);
            _shakeTween?.Kill();
            _readyWiggleTween?.Kill();
            StopCraftAnimation();
        }

        private void OnTaskStarted(TaskData task)
        {
            _activeCosts.Clear();

            var cost = task.CostInfo;
            if (cost.CrystalCost > 0) _activeCosts.Add((ResourceType.Crystal, cost.CrystalCost));
            if (cost.PolymerCost > 0) _activeCosts.Add((ResourceType.Polymer, cost.PolymerCost));
            if (cost.NanoGelCost > 0) _activeCosts.Add((ResourceType.NanoGel, cost.NanoGelCost));

            RebuildSlots();
            RefreshSlots();
            RefreshIcon(task.ResultType);

            var totalTime = task.ProduceTime * GameData.Instance.UpgradesData[UpgradeType.CraftSpeed].Multiplier;
            UpdateTimerText(totalTime);
        }

        private void RefreshIcon(MedicationsType resultType)
        {
            var icon = _taskIconConfig.GetIcon(resultType);
            _taskIcon.sprite = icon;
            _taskIcon.enabled = icon != null;
        }

        private void RebuildSlots()
        {
            foreach (var slot in _spawnedSlots)
                Destroy(slot.gameObject);
            _spawnedSlots.Clear();

            foreach (var (type, cost) in _activeCosts)
            {
                var slot = Instantiate(_resourceSlotPrefab, _resourceSlotsContainer);
                slot.SetData(type, 0, cost, _resourceIconConfig);
                _spawnedSlots.Add(slot);
            }
        }

        private void RefreshSlots()
        {
            var resources = GameData.Instance.Resources;

            for (var i = 0; i < _activeCosts.Count && i < _spawnedSlots.Count; i++)
            {
                var (type, required) = _activeCosts[i];
                var available = resources[type];
                var slot = _spawnedSlots[i];

                slot.SetData(type, available, required, _resourceIconConfig);
            }

            if (!_isCrafting)
            {
                var canCraft = _taskService.HasActiveTask && CanCraft(_taskService.CurrentTask.Value.CostInfo);
                if (canCraft != _wasReady)
                {
                    _wasReady = canCraft;
                    if (canCraft) StartReadyWiggle();
                    else StopReadyWiggle();
                }
            }
        }

        private void OnCraftClicked()
        {
            if (!_taskService.HasActiveTask || _isCrafting)
                return;

            if (S.TryGet<ITutorialService>(out var tutorial))
                tutorial.NotifyEvent(TutorialEventType.CraftButtonClicked);

            var task = _taskService.CurrentTask.Value;

            if (CanCraft(task.CostInfo))
            {
                SpendResources(task.CostInfo);
                _isCrafting = true;
                _craftButton.interactable = false;
                _craftTotalTime = task.ProduceTime * GameData.Instance.UpgradesData[UpgradeType.CraftSpeed].Multiplier;
                _craftTimeRemaining = _craftTotalTime;
                StopReadyWiggle();
                StartCraftAnimation();
                AudioManager.Instance.PlayCraftWorking();
                _resourceSlotsContainer.gameObject.SetActive(false);
                _craftTimerHandle = _timerService.Start(_craftTotalTime, () => OnCraftFinished(task));

                if (!_firstCraftStarted)
                {
                    _firstCraftStarted = true;
                    _gardenAttentionAnimator?.PlayAttention();
                }
            }
            else
            {
                Debug.Log("Not enough resources to craft!");
                Shake();
            }
        }

        private void OnCraftFinished(TaskData task)
        {
            _isCrafting = false;
            _craftButton.interactable = true;
            StopCraftAnimation();
            _resourceSlotsContainer.gameObject.SetActive(true);
            AudioManager.Instance.StopCraftWorking();
            AudioManager.Instance.PlayCraftComplete();

            GameData.Instance.AddResource(ResourceType.Credit, task.CreditReward);
            _resourcesAnimatorWidget.PlayAnimation(ResourceType.Credit, task.CreditReward);
            if (S.TryGet<FlyingIconService>(out var flyService))
                flyService.FlyCoin(transform);
            else
                Debug.LogWarning("[CraftStantionView] FlyingIconService not found in S.");
            ClearAll();
            _taskService.CompleteCurrentTask();

            if (!_firstOrderCompleted)
            {
                _firstOrderCompleted = true;
                if (_gardenBed != null && _gardenBed.OccupiedSlotsCount < 3)
                    _gardenAttentionAnimator?.PlayAttention();
            }

            PlayBumpAnimation();
        }

        private void StartReadyWiggle()
        {
            _readyWiggleTween?.Kill();
            _animRectTransform.localRotation = Quaternion.identity;
            _animRectTransform.localScale = Vector3.one;

            _readyWiggleTween = DOTween.Sequence()
                // wiggle
                .Append(_animRectTransform.DOLocalRotate(new Vector3(0f, 0f, 5f), _wiggleDuration).SetEase(Ease.InOutSine))
                .Append(_animRectTransform.DOLocalRotate(new Vector3(0f, 0f, -5f), _wiggleDuration * 2f).SetEase(Ease.InOutSine))
                .Append(_animRectTransform.DOLocalRotate(new Vector3(0f, 0f, 0f), _wiggleDuration).SetEase(Ease.InOutSine))
                .AppendInterval(0.15f)
                // press
                .Append(_animRectTransform.DOScale(_pressScale, _pressDuration).SetEase(Ease.InQuad))
                .Append(_animRectTransform.DOScale(1f, _pressDuration * 1.5f).SetEase(Ease.OutBack))
                .AppendInterval(0.15f)
                // wiggle again
                .Append(_animRectTransform.DOLocalRotate(new Vector3(0f, 0f, 5f), _wiggleDuration).SetEase(Ease.InOutSine))
                .Append(_animRectTransform.DOLocalRotate(new Vector3(0f, 0f, -5f), _wiggleDuration * 2f).SetEase(Ease.InOutSine))
                .Append(_animRectTransform.DOLocalRotate(new Vector3(0f, 0f, 0f), _wiggleDuration).SetEase(Ease.InOutSine))
                .AppendInterval(0.8f)
                .SetLoops(-1)
                .SetLink(gameObject);
        }

        private void StopReadyWiggle()
        {
            _readyWiggleTween?.Kill();
            _readyWiggleTween = null;
            _wasReady = false;
            if (_animRectTransform != null)
            {
                _animRectTransform.localRotation = Quaternion.identity;
                _animRectTransform.localScale = Vector3.one;
            }
        }

        private void StartCraftAnimation()
        {
            _craftScaleTween?.Kill();
            _animRectTransform.localScale = Vector3.one;
            _craftScaleTween = _animRectTransform
                .DOScale(_craftScaleAmount, _craftScaleDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject);
        }

        private void StopCraftAnimation()
        {
            _craftScaleTween?.Kill();
            _craftScaleTween = null;
            if (_animRectTransform != null)
                _animRectTransform.localScale = Vector3.one;
        }

        private void PlayBumpAnimation()
        {
            _animRectTransform.localScale = Vector3.one;
            _animRectTransform
                .DOPunchScale(Vector3.one * (_bumpScale - 1f), _bumpDuration, vibrato: 1, elasticity: 0.5f)
                .SetEase(Ease.OutBack)
                .SetLink(gameObject);
        }

        private void ClearAll()
        {
            foreach (var slot in _spawnedSlots)
                Destroy(slot.gameObject);

            _spawnedSlots.Clear();
            _activeCosts.Clear();
            _taskIcon.sprite = null;
            _taskIcon.enabled = false;
            UpdateTimerText(0f);
        }

        private bool CanCraft(ProductionCost cost)
        {
            var resources = GameData.Instance.Resources;

            if (cost.CrystalCost > 0 && resources[ResourceType.Crystal] < cost.CrystalCost)
                return false;
            if (cost.PolymerCost > 0 && resources[ResourceType.Polymer] < cost.PolymerCost)
                return false;
            if (cost.NanoGelCost > 0 && resources[ResourceType.NanoGel] < cost.NanoGelCost)
                return false;

            return true;
        }

        private void SpendResources(ProductionCost cost)
        {
            if (cost.CrystalCost > 0)
                GameData.Instance.AddResource(ResourceType.Crystal, -cost.CrystalCost);
            if (cost.PolymerCost > 0)
                GameData.Instance.AddResource(ResourceType.Polymer, -cost.PolymerCost);
            if (cost.NanoGelCost > 0)
                GameData.Instance.AddResource(ResourceType.NanoGel, -cost.NanoGelCost);
        }

        private Tweener _shakeTween;

        private void Shake()
        {
            _shakeTween?.Kill();
            _craftButton.interactable = false;
            _animRectTransform.anchoredPosition = Vector2.zero;
            _shakeTween = _animRectTransform
                .DOShakeAnchorPos(_shakeDuration, new Vector2(_shakeStrength, 0), vibrato: 3, randomness: 0, fadeOut: true)
                .SetEase(Ease.OutSine)
                .SetLink(gameObject)
                .OnKill(() => _craftButton.interactable = true);
        }

        public void Reset()
        {
            if (_isCrafting)
            {
                _timerService.Cancel(_craftTimerHandle);
                _isCrafting = false;
                _craftButton.interactable = true;
                StopCraftAnimation();
                AudioManager.Instance.StopCraftWorking();
                _resourceSlotsContainer.gameObject.SetActive(true);
            }

            StopReadyWiggle();
            ClearAll();
            _firstCraftStarted = false;
            _firstOrderCompleted = false;
        }

        private void UpdateTimerText(float time)
        {
            if (_timerText == null) return;

            if (time <= 0f)
            {
                _timerText.text = string.Empty;
                return;
            }

            var minutes = Mathf.FloorToInt(time / 60f);
            var seconds = Mathf.FloorToInt(time % 60f);
            _timerText.text = minutes > 0 ? $"{minutes:0}:{seconds:00}" : $"{seconds}s";
        }
    }
}
