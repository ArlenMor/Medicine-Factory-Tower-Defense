using System;
using _Project.Code.Scripts.Configs;
using _Project.Code.Scripts.Data;
using _Project.Code.Scripts.Garden;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.Scripts.UI
{
    public class PlantChooseView: MonoBehaviour
    {
        [SerializeField] private PlantType plantType;
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _productivityText;
        [SerializeField] private TMP_Text _growTimeText;
        
        public PlantType PlantType => plantType;

        public void Initialize(Action<PlantType> action)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => action(plantType));

            var growSpeedMultiplier = GameData.Instance.UpgradesData[UpgradeType.GrowSpeed].Multiplier;
            var config = GameData.Instance.GameConfig;
            var resourceData = config.GardenConfig.GetGrowableResourceData(plantType.GetResourceType());
            var resultProduce = resourceData.DefaultProductivity * GameData.Instance.GetProduceExpectedMultiplier();
            var resultGrowTime = resourceData.GrowthTime / growSpeedMultiplier;
            
            _productivityText.text = $"Productivity \n {resultProduce:0.#} p/s";
            _growTimeText.text = $"Grow Time \n {MathF.Round(resultGrowTime, 1)} sec";
        }

        public void OnDestroy()
        {
            _button.onClick.RemoveAllListeners();
        }
    }
}