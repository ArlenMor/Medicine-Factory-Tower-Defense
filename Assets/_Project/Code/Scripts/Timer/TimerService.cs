using System;
using System.Collections.Generic;
using _Project.Code.Scripts.Game;

namespace _Project.Code.Scripts.Timer
{
    public class TimerService : ITimerService, IManualUpdate
    {
        private int _nextId = 1;
        private readonly List<TimerEntry> _timers = new();

        private struct TimerEntry
        {
            public int Id;
            public float StartTime;
            public float Remaining;
            public bool Paused;
            public Action OnComplete;
            public Action OnHalfTime;
        }

        public TimerHandle Start(float duration, Action onComplete, Action onHalfTime = null)
        {
            var id = _nextId++;
            _timers.Add(new TimerEntry
            {
                Id = id,
                StartTime = duration,
                Remaining = duration,
                Paused = false,
                OnComplete = onComplete,
                OnHalfTime = onHalfTime
            });
            return new TimerHandle(id);
        }

        public void Pause(TimerHandle handle)
        {
            for (int i = 0; i < _timers.Count; i++)
            {
                if (_timers[i].Id != handle.Id) continue;
                var entry = _timers[i];
                entry.Paused = true;
                _timers[i] = entry;
                return;
            }
        }

        public void Resume(TimerHandle handle)
        {
            for (int i = 0; i < _timers.Count; i++)
            {
                if (_timers[i].Id != handle.Id) continue;
                var entry = _timers[i];
                entry.Paused = false;
                _timers[i] = entry;
                return;
            }
        }

        public void Cancel(TimerHandle handle)
        {
            for (int i = 0; i < _timers.Count; i++)
            {
                if (_timers[i].Id != handle.Id) continue;
                _timers.RemoveAt(i);
                return;
            }
        }

        public bool IsActive(TimerHandle handle)
        {
            for (int i = 0; i < _timers.Count; i++)
            {
                if (_timers[i].Id == handle.Id)
                    return true;
            }
            return false;
        }

        public void ManualUpdate(float deltaTime)
        {
            for (int i = _timers.Count - 1; i >= 0; i--)
            {
                var entry = _timers[i];
                if (entry.Paused) continue;

                entry.Remaining -= deltaTime;

                if (entry.Remaining <= 0f)
                {
                    _timers.RemoveAt(i);
                    entry.OnComplete?.Invoke();
                }
                else
                {
                    _timers[i] = entry;
                }
                
                if (entry.Remaining <= entry.StartTime / 2f)
                {
                    entry.OnHalfTime?.Invoke();
                }
            }
        }
    }
}
