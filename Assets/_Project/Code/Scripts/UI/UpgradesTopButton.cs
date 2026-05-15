using _Project.Code.Scripts.ServiceLocator;
using _Project.Code.Scripts.Tutorial;
using _Project.Code.Scripts.UIPanels.Settings;
using _Project.Code.Scripts.UIService;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.Scripts.UI
{
    public class UpgradesTopButton: MonoBehaviour
    {
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private Transform _popupParent;

        private IPanelShower _shower;
        
        public void Initialize(IPanelShower panelShower)
        {
            _shower = panelShower;
            
            _upgradeButton.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            S.TryGet<ITutorialService>(out var tutorial);
            if (tutorial != null && tutorial.IsUpgradesDisabled)
                return;

            var settings = new UpgradeScreenSettings
            {
                Shower = _shower,
            };
            _shower.ShowView(PanelType.UpgradesPopup, settings, _popupParent);
            tutorial?.NotifyEvent(TutorialEventType.UpgradesPanelOpened);
        }
    }
}