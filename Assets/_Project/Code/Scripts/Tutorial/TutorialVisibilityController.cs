using System.Collections.Generic;
using UnityEngine;

namespace _Project.Code.Scripts.Tutorial
{
    public class TutorialVisibilityController : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _buildingObjects = new();

        [SerializeField] private List<GameObject> _upgradesObjects = new();

        private ITutorialService _tutorialService;

        public void Initialize(ITutorialService tutorialService)
        {
            _tutorialService = tutorialService;

            _tutorialService.OnStepStarted       += OnStepStartedHandler;
            _tutorialService.OnStepHidden        += Refresh;
        }

        private void OnDestroy()
        {
            if (_tutorialService == null) return;
            _tutorialService.OnStepStarted       -= OnStepStartedHandler;
            _tutorialService.OnStepHidden        -= Refresh;
        }

        private void OnStepStartedHandler(TutorialStepData _) => Refresh();

        private void Refresh()
        {
            bool buildingDisabled = _tutorialService.IsActive && _tutorialService.IsBuildingDisabled;
            bool upgradesDisabled = _tutorialService.IsActive && _tutorialService.IsUpgradesDisabled;

            foreach (var go in _buildingObjects)
                if (go != null) go.SetActive(!buildingDisabled);

            foreach (var go in _upgradesObjects)
                if (go != null) go.SetActive(!upgradesDisabled);
        }
    }
}
