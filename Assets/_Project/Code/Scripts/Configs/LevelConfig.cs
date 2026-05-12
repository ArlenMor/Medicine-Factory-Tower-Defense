using System;
using System.Collections.Generic;
using _Project.Code.Scripts.EnemySystem;
using _Project.Code.Scripts.Garden;
using UnityEngine;

namespace _Project.Code.Scripts.Configs
{
    [Serializable]
    public struct SlotPlantSetup
    {
        public int SlotIndex;
        public PlantType PlantType;
        public bool IsAlreadyGrown;
    }

    [CreateAssetMenu(fileName = "LevelConfig", menuName = "GameConfig/LevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        public WaveConfig WaveConfig;
        public TaskConfig TaskConfig;
        public List<SlotPlantSetup> InitialPlants;
    }
}
