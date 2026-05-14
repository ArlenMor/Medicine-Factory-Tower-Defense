using System;

namespace _Project.Code.Scripts.Tutorial
{
    public interface ITutorialService
    {
        event Action<TutorialStepData> OnStepStarted;

        event Action OnStepHidden;

        event Action OnSequenceCompleted;

        bool IsActive { get; }

        bool IsBuildingDisabled { get; }

        bool IsUpgradesDisabled { get; }

        void StartSequence(TutorialSequenceData sequence);

        void AdvanceManually();

        void NotifyEvent(TutorialEventType eventType);
    }
}
