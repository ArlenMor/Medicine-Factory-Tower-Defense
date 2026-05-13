using System.Collections.Generic;
using _Project.Code.Scripts.BattleField;
using _Project.Code.Scripts.Configs;
using _Project.Code.Scripts.EnemySystem;
using _Project.Code.Scripts.UI;
using UnityEngine;

namespace _Project.Code.Scripts
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "GameConfig/GameConfig")]
    public class GameConfig: ScriptableObject
    {
        public GardenConfig GardenConfig;
        public ResourceIconConfig ResourceIconConfig;
        public TaskIconConfig TaskIconConfig;
        public TaskConfig TaskConfig;
        public EnemyConfig EnemyConfig;
        public UpgradesConfig UpgradesConfig;
        public FieldConfig FieldConfig;
        public DefenseShopConfig DefenseShopConfig;
        public DamagePopupConfig DamagePopupConfig;
        public LevelOrdersConfig LevelOrdersConfig;

        public List<LevelConfig> Levels;
        public LevelConfig GetLevel(int levelIndex)
        {
            int index = Mathf.Clamp(levelIndex - 1, 0, Levels.Count - 1);
            return Levels[index];
        }
    }
}