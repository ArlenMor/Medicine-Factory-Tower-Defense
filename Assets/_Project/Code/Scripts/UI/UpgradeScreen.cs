using System;
using System.Globalization;
using System.Linq;
using _Project.Code.Scripts.Audio;
using _Project.Code.Scripts.Data;
using _Project.Code.Scripts.ServiceLocator;
using _Project.Code.Scripts.Tutorial;
using _Project.Code.Scripts.UIPanels.Settings;
using _Project.Code.Scripts.UIService;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.Scripts.UI
{
    public class UpgradeScreen: BasePanel
    {
        [SerializeField] private UpgradeButtonView[] _upgradeButtonViews;
        [SerializeField] private Button _backGroundFade;
        [SerializeField] private Button _closeButton;

        private GameData _gameData;
        private GameConfig _gameConfig;
        private IPanelShower _shower;
        
        public override void Initialize(PanelSettings settings)
        {
            base.Initialize(settings);

            if (settings is not UpgradeScreenSettings upgradeScreenSettings)
            {
                Debug.LogError($"[UpgradeScreen] Invalid settings for {nameof(UpgradeScreenSettings)}");
                return;
            }
            
            _shower = upgradeScreenSettings.Shower;
            
            _gameData = GameData.Instance;
            _gameConfig = _gameData.GameConfig;

            foreach (var upgradeButtonView in _upgradeButtonViews)
            {
                upgradeButtonView.Initialize(Upgrade);
            }
            
            _backGroundFade.onClick.AddListener(HidePanel);
            _closeButton.onClick.AddListener(HidePanel);
            
            RefreshButtons();
        }

        private void HidePanel()
        {
            _shower.HideView(PanelType.UpgradesPopup);
        }
        
        private void Upgrade(UpgradeButtonView upgrade)
        {
            var upgradeData = _gameData.UpgradesData[upgrade.Type];
            var upgradeDef = _gameConfig.UpgradesConfig.Upgrades.FirstOrDefault(u => u.Type == upgrade.Type);
            
            if (upgradeData.IsMax)
            {
                Debug.Log("[UpgradeScreen] - Upgrade is already Max");
                return;
            }
            
            var step = upgradeData.Step;
            
            var isEnoughMoney = upgradeDef.Costs[step + 1] <= _gameData.Resources[ResourceType.Credit];

            if (isEnoughMoney)
            {
                upgradeData.Step += 1;
                _gameData.AddResource(ResourceType.Credit, -upgradeDef.Costs[upgradeData.Step]);
                upgradeData.Multiplier = upgradeDef.Multipliers[upgradeData.Step];
                GameData.Instance.Stats.UpgradesPurchased++;
                if (S.TryGet<ITutorialService>(out var tutorial))
                    tutorial.NotifyEvent(TutorialEventType.UpgradePurchased);
                AudioManager.Instance.PlayUpgrade();
                if (upgradeData.Step >= upgradeDef.Multipliers.Length - 1)
                {
                    upgradeData.IsMax = true;
                }
            }
            
            RefreshButtons();
        }
        
        private void RefreshButtons()
        {
            var upgradesData = GameData.Instance.UpgradesData;

            foreach (var upgradeButtonView in _upgradeButtonViews)
            {
                var currentMultiplier = upgradesData[upgradeButtonView.Type].Multiplier.ToString(CultureInfo.InvariantCulture);
                
                if (upgradesData[upgradeButtonView.Type].IsMax)
                {
                    upgradeButtonView.SetMax(true);
                    upgradeButtonView.RefreshText(currentMultiplier);
                    continue;
                }
                
                var upgradeType = upgradeButtonView.Type;
                
                var currentStep = upgradesData[upgradeType].Step;
                
                var nextMultiplier = string.Empty;
                var upgradeCost = string.Empty;
                foreach (var upgradeDef in _gameConfig.UpgradesConfig.Upgrades)
                {
                    if (upgradeDef.Type != upgradeButtonView.Type) continue;
                    
                    if (currentStep >= upgradeDef.Multipliers.Length - 1)
                    {
                        break;
                    }

                    var nextStep = upgradesData[upgradeButtonView.Type].Step + 1;
                    
                    nextMultiplier = upgradeDef.Multipliers[nextStep].ToString(CultureInfo.InvariantCulture);
                    upgradeCost = upgradeDef.Costs[nextStep].ToString(CultureInfo.InvariantCulture);
                }
                
                upgradeButtonView.RefreshText(currentMultiplier, nextMultiplier, upgradeCost);
            }
        }

        private void OnDestroy()
        {
            _backGroundFade.onClick.RemoveListener(Close);
            _closeButton.onClick.RemoveListener(Close);
        }
    }
}