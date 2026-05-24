using System;
using System.Collections.Generic;
using _Project.Code.Scripts.Configs;
using _Project.Code.Scripts.ServiceLocator;
using _Project.Code.Scripts.Tutorial;
using UnityEngine;

namespace _Project.Code.Scripts.Data
{
    public class GameData
    {
        public static GameData Instance;
        
        private readonly GameConfig _config;
        public GameConfig GameConfig => _config;
        
        public Dictionary<ResourceType, int> Resources { get; set; } = new ();
        public event Action OnResourcesChanged;
        public event Action<UpgradeType> OnUpgradePurchased;
        public event Action OnFirstBuildablePlaced;

        public void NotifyUpgradePurchased(UpgradeType type) => OnUpgradePurchased?.Invoke(type);
        public void NotifyFirstBuildablePlaced() => OnFirstBuildablePlaced?.Invoke();
        
        public int ProductionProductivityMultiplier = 1;
        private int _produceDoubleMissStreak;
        
        public Camera Camera;
        public readonly Dictionary<UpgradeType, UpgradeData> UpgradesData = new();
        public GameStats Stats { get; } = new();

        public GameData(GameConfig config, Camera camera)
        {
            _config = config;
            Camera = camera;
        }
        
        public void Initialize()
        {
            Instance = this;
            
            GenerateResourceData();
            
            GenerateMultipliers();
        }

        private void GenerateMultipliers()
        {
            var config = _config.UpgradesConfig;
            foreach (var upgradeDef in config.Upgrades)
            {
                UpgradesData.Add(upgradeDef.Type, new UpgradeData()
                {
                    Type = upgradeDef.Type,
                    Multiplier = upgradeDef.Multipliers[0],
                    Step = 0
                });
            }
        }

        public void AddResource(ResourceType resourceType, int amount)
        {
            Resources[resourceType] += amount;
            if (resourceType == ResourceType.Credit && amount > 0)
            {
                Stats.CreditsEarned += amount;
                if (S.TryGet<ITutorialService>(out var tutorial))
                    tutorial.NotifyEvent(TutorialEventType.CreditsEarned);
            }
            OnResourcesChanged?.Invoke();
        }

        public void SetResource(ResourceType resourceType, int amount)
        {
            Resources[resourceType] = amount;
            OnResourcesChanged?.Invoke();
        }

        public void ResetResources(int startCredits)
        {
            Resources[ResourceType.Crystal] = GetResourceStartAmount(ResourceType.Crystal);
            Resources[ResourceType.Polymer] = GetResourceStartAmount(ResourceType.Polymer);
            Resources[ResourceType.NanoGel] = GetResourceStartAmount(ResourceType.NanoGel);
            Resources[ResourceType.Credit] = startCredits;
            OnResourcesChanged?.Invoke();
        }

        public void ResetUpgrades()
        {
            var config = _config.UpgradesConfig;
            foreach (var upgradeDef in config.Upgrades)
            {
                UpgradesData[upgradeDef.Type].Multiplier = upgradeDef.Multipliers[0];
                UpgradesData[upgradeDef.Type].Step = 0;
                UpgradesData[upgradeDef.Type].IsMax = false;
            }

            _produceDoubleMissStreak = 0;
        }

        public float GetProduceDoubleChance()
        {
            if (!UpgradesData.TryGetValue(UpgradeType.Produce, out var produceUpgrade))
                return 0f;

            // Step 0 means no upgrade purchased yet, so chance must stay 0.
            if (produceUpgrade.Step <= 0)
                return 0f;

            return NormalizeChance(produceUpgrade.Multiplier);
        }

        public float GetProduceExpectedMultiplier() => 1f + GetProduceDoubleChance();

        public bool RollProduceDoubleWithPity()
        {
            if (!UpgradesData.TryGetValue(UpgradeType.Produce, out var produceUpgrade))
                return false;

            if (produceUpgrade.Step <= 0)
                return false;

            int maxAttempts = GetProducePityMaxAttempts(produceUpgrade.Step);
            if (maxAttempts > 0 && _produceDoubleMissStreak >= maxAttempts - 1)
            {
                _produceDoubleMissStreak = 0;
                return true;
            }

            bool doubled = UnityEngine.Random.value < NormalizeChance(produceUpgrade.Multiplier);
            if (doubled)
            {
                _produceDoubleMissStreak = 0;
                return true;
            }

            _produceDoubleMissStreak++;
            return false;
        }

        private static float NormalizeChance(float rawChance)
        {
            // Supports both [0..1] and [0..100] authoring formats in config.
            if (rawChance > 1f)
                rawChance /= 100f;

            return Mathf.Clamp01(rawChance);
        }

        private static int GetProducePityMaxAttempts(int produceStep)
        {
            // Multipliers[0] is base (no purchased upgrade).
            // Step 1 -> guarantee no later than every 4th harvest.
            // Step 2+ -> guarantee no later than every 3rd harvest.
            if (produceStep >= 2)
                return 3;

            if (produceStep == 1)
                return 4;

            return 0;
        }

        private void GenerateResourceData()
        {
            Resources.Add(ResourceType.Crystal, GetResourceStartAmount(ResourceType.Crystal));
            Resources.Add(ResourceType.Polymer, GetResourceStartAmount(ResourceType.Polymer));
            Resources.Add(ResourceType.NanoGel, GetResourceStartAmount(ResourceType.NanoGel));
            Resources.Add(ResourceType.Credit, 0);
        }

        private int GetResourceStartAmount(ResourceType resourceType) => 
            _config.GardenConfig.GetGrowableResourceData(resourceType).StartAmount;
    }
}