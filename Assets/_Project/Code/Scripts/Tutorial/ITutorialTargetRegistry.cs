using UnityEngine;

namespace _Project.Code.Scripts.Tutorial
{
    public interface ITutorialTargetRegistry
    {
        void Register(string id, Transform target);

        void Unregister(string id);

        bool TryGet(string id, out Transform target);

        bool TryGetRect(string id, out RectTransform target);
    }
}
