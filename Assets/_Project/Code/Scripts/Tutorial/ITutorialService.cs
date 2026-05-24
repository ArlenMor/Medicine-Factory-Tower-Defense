using System;

namespace _Project.Code.Scripts.Tutorial
{
    public interface ITutorialService
    {
        event Action<TutorialStepData> OnStepStarted;

        event Action<TutorialStepData> OnStepCompleted;

        event Action OnStepHidden;

        event Action OnSequenceCompleted;

        bool IsActive { get; }

        TutorialStepData ActiveStep { get; }

        bool IsBuildingDisabled { get; }

        bool IsUpgradesDisabled { get; }

        void StartSequence(TutorialSequenceData sequence);

        void StopSequence();

        void AdvanceManually();

        void NotifyEvent(TutorialEventType eventType);
    }
}
