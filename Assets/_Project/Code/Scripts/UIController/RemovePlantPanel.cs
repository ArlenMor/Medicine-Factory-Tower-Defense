using UnityEngine;
using UnityEngine.UI;
using _Project.Code.Scripts.ServiceLocator;
using _Project.Code.Scripts.Tutorial;

namespace _Project.Code.Scripts.UIService
{
    public class RemovePlantPanel : BasePanel
    {
        [SerializeField] private Button _removeButton;
        private string _registeredTutorialTargetId;

        public override void Initialize(PanelSettings settings)
        {
            base.Initialize(settings);

            if (settings is not RemovePlantPanelSettings castedSettings)
            {
                Debug.LogError("[RemovePlantPanel] - castedSettings is null");
                return;
            }

            _removeButton.onClick.RemoveAllListeners();
            _removeButton.onClick.AddListener(() => castedSettings.Callback?.Invoke());

            transform.position = castedSettings.Position;
            RegisterTutorialTarget(castedSettings.TutorialTargetId);
        }

        private void OnDestroy()
        {
            UnregisterTutorialTarget();
        }

        private void RegisterTutorialTarget(string targetId)
        {
            if (string.IsNullOrEmpty(targetId))
            {
                UnregisterTutorialTarget();
                return;
            }

            if (!S.TryGet<ITutorialTargetRegistry>(out var registry))
                return;

            if (!string.IsNullOrEmpty(_registeredTutorialTargetId) && _registeredTutorialTargetId != targetId)
                registry.Unregister(_registeredTutorialTargetId);

            _registeredTutorialTargetId = targetId;
            registry.Register(targetId, transform);
        }

        private void UnregisterTutorialTarget()
        {
            if (string.IsNullOrEmpty(_registeredTutorialTargetId))
                return;

            if (S.TryGet<ITutorialTargetRegistry>(out var registry))
                registry.Unregister(_registeredTutorialTargetId);

            _registeredTutorialTargetId = null;
        }
    }
}
