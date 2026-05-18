using System;
using UnityEngine;

namespace _Project.Code.Scripts.Tutorial
{
    public class TutorialService : ITutorialService
    {
        public event Action<TutorialStepData> OnStepStarted;
        public event Action<TutorialStepData> OnStepCompleted;
        public event Action OnStepHidden;
        public event Action OnSequenceCompleted;

        public bool IsActive { get; private set; }
        public TutorialStepData ActiveStep => CurrentStep();
        public bool IsBuildingDisabled => IsActive && _currentSequence != null && _currentSequence.DisableBuilding;
        public bool IsUpgradesDisabled => IsActive && _currentSequence != null && _currentSequence.DisableUpgrades;

        private TutorialSequenceData _currentSequence;
        private int _currentStepIndex;

        private bool _waitingForCondition;
        private int _eventCounter;

        // ── Public API ──────────────────────────────────────────────────────────

        public void StartSequence(TutorialSequenceData sequence)
        {
            if (sequence == null || sequence.Steps == null || sequence.Steps.Count == 0)
                return;

            _currentSequence = sequence;
            _currentStepIndex = 0;
            IsActive = true;

            ShowStep(0);
        }

        public void AdvanceManually()
        {
            if (!IsActive || _waitingForCondition) return;

            var step = CurrentStep();
            if (step == null || step.Trigger != StepTrigger.ManualButton) return;

            MoveNext();
        }

        public void NotifyEvent(TutorialEventType eventType)
        {
            if (!IsActive) return;

            var step = CurrentStep();
            if (step == null) return;

            if (_waitingForCondition)
            {
                // Ждём WaitCondition — шаг ещё не показан
                if (EventMatchesTrigger(eventType, step.WaitCondition))
                {
                    _eventCounter++;
                    if (_eventCounter >= step.WaitCount)
                    {
                        _waitingForCondition = false;
                        _eventCounter = 0;
                        OnStepStarted?.Invoke(step);
                    }
                }
                return;
            }

            // Шаг показан — ждём Trigger для перехода к следующему
            if (EventMatchesTrigger(eventType, step.Trigger))
            {
                _eventCounter++;
                if (_eventCounter >= Mathf.Max(1, step.TriggerCount))
                    MoveNext();
            }
        }

        // ── Private helpers ─────────────────────────────────────────────────────

        private void ShowStep(int index)
        {
            _eventCounter = 0;

            var step = _currentSequence.Steps[index];
            if (step == null)
            {
                Debug.LogWarning($"[TutorialService] Step {index} is null, skipping.");
                MoveNext();
                return;
            }

            if (step.WaitCondition != StepTrigger.None)
            {
                // Шаг скрыт до выполнения условия — прячем оверлей
                _waitingForCondition = true;
                OnStepHidden?.Invoke();
                return;
            }

            _waitingForCondition = false;
            OnStepStarted?.Invoke(step);
        }

        private void MoveNext()
        {
            var completedStep = CurrentStep();
            _currentStepIndex++;
            _eventCounter = 0;

            OnStepCompleted?.Invoke(completedStep);

            if (_currentStepIndex >= _currentSequence.Steps.Count)
            {
                IsActive = false;
                _currentSequence = null;
                OnSequenceCompleted?.Invoke();
                return;
            }

            ShowStep(_currentStepIndex);
        }

        private TutorialStepData CurrentStep()
        {
            if (_currentSequence == null) return null;
            if (_currentStepIndex < 0 || _currentStepIndex >= _currentSequence.Steps.Count) return null;
            return _currentSequence.Steps[_currentStepIndex];
        }

        private static bool EventMatchesTrigger(TutorialEventType ev, StepTrigger trigger) => trigger switch
        {
            StepTrigger.ResourceHarvested  => ev == TutorialEventType.ResourceHarvested,
            StepTrigger.CraftButtonClicked => ev == TutorialEventType.CraftButtonClicked,
            StepTrigger.TaskCompleted      => ev == TutorialEventType.TaskCompleted,
            StepTrigger.AllTasksCompleted  => ev == TutorialEventType.TaskCompleted,
            StepTrigger.BuildingPlaced     => ev == TutorialEventType.BuildingPlaced,
            StepTrigger.EnemyKilled        => ev == TutorialEventType.EnemyKilled,
            StepTrigger.PlantPlanted       => ev == TutorialEventType.PlantPlanted,
            StepTrigger.CreditsEarned      => ev == TutorialEventType.CreditsEarned,
            StepTrigger.WaveStarted        => ev == TutorialEventType.WaveStarted,
            StepTrigger.WaveCleared        => ev == TutorialEventType.WaveCleared,
            StepTrigger.UpgradePurchased   => ev == TutorialEventType.UpgradePurchased,
            StepTrigger.UpgradesPanelOpened => ev == TutorialEventType.UpgradesPanelOpened,
            StepTrigger.PlantRemoved       => ev == TutorialEventType.PlantRemoved,
            _                              => false,
        };
    }
}
