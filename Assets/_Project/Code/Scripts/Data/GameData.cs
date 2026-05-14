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
        
        public int ProductionProductivityMultiplier = 1;
        
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