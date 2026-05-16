using System.Collections.Generic;
using UnityEngine;

namespace _Project.Code.Scripts.Tutorial
{
    public class TutorialTargetRegistry : ITutorialTargetRegistry
    {
        public event System.Action OnTargetsChanged;

        private readonly Dictionary<string, Transform> _targets = new();

        public void Register(string id, Transform target)
        {
            if (string.IsNullOrEmpty(id)) return;
            _targets[id] = target;
            OnTargetsChanged?.Invoke();
        }

        public void Unregister(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (_targets.Remove(id))
                OnTargetsChanged?.Invoke();
        }

        public bool TryGet(string id, out Transform target)
        {
            return _targets.TryGetValue(id, out target);
        }

        public bool TryGetRect(string id, out RectTransform target)
        {
            if (_targets.TryGetValue(id, out var t) && t is RectTransform rt)
            {
                target = rt;
                return true;
            }
            target = null;
            return false;
        }
    }
}