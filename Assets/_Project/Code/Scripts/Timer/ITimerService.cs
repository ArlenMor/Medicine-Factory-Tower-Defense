using System;

namespace _Project.Code.Scripts.Timer
{
    public interface ITimerService
    {
        TimerHandle Start(float duration, Action onComplete, Action onHalfTime = null);
        void Pause(TimerHandle handle);
        void Resume(TimerHandle handle);
        void Cancel(TimerHandle handle);
        bool IsActive(TimerHandle handle);
    }
}
