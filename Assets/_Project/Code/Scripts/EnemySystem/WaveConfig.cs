using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Code.Scripts.EnemySystem
{
    [Serializable]
    public struct WaveData
    {
        public int WaveId;
        public float StartTime;
        public int ScoutCount;
        public int GnawerCount;
        public int TankCount;
        public float IntraSpawnInterval;
        [TextArea] public string Comment;
    }

    [CreateAssetMenu(fileName = "WaveConfig", menuName = "GameConfig/WaveConfig")]
    public class WaveConfig : ScriptableObject
    {
        public List<WaveData> Waves;

        [Header("Loop")]
        [Tooltip("Зацикливать последнюю волну после уничтожения всех врагов.")]
        public bool LoopLastWave;
        [Tooltip("Задержка перед повторным запуском последней волны (секунды).")]
        public float LoopDelay = 10f;

        [ContextMenu("Waves for level 4")]
        private void FillLevel4()
        {
            LoopLastWave = true;
            LoopDelay = 6f;

            Waves = new List<WaveData>
            {
                new() { WaveId = 1, StartTime = 3f,  ScoutCount = 4, GnawerCount = 1, TankCount = 0, IntraSpawnInterval = 0.8f, Comment = "Early pressure" },
                new() { WaveId = 2, StartTime = 18f, ScoutCount = 5, GnawerCount = 2, TankCount = 0, IntraSpawnInterval = 0.7f, Comment = "Need active clicks" },
                new() { WaveId = 3, StartTime = 38f, ScoutCount = 4, GnawerCount = 3, TankCount = 0, IntraSpawnInterval = 0.65f, Comment = "Economy disruption" },
                new() { WaveId = 4, StartTime = 62f, ScoutCount = 5, GnawerCount = 3, TankCount = 1, IntraSpawnInterval = 0.62f, Comment = "Loop wave" }
            };
        }

        [ContextMenu("Waves for level 5")]
        private void FillLevel5()
        {
            LoopLastWave = true;
            LoopDelay = 6f;

            Waves = new List<WaveData>
            {
                new() { WaveId = 1, StartTime = 3f,  ScoutCount = 5, GnawerCount = 1, TankCount = 0, IntraSpawnInterval = 0.75f, Comment = "Fast opening" },
                new() { WaveId = 2, StartTime = 20f, ScoutCount = 5, GnawerCount = 3, TankCount = 0, IntraSpawnInterval = 0.65f, Comment = "Stronger mid pressure" },
                new() { WaveId = 3, StartTime = 42f, ScoutCount = 4, GnawerCount = 3, TankCount = 1, IntraSpawnInterval = 0.62f, Comment = "First serious check" },
                new() { WaveId = 4, StartTime = 68f, ScoutCount = 5, GnawerCount = 4, TankCount = 1, IntraSpawnInterval = 0.58f, Comment = "Loop wave" }
            };
        }

        [ContextMenu("Waves for level 6")]
        private void FillLevel6()
        {
            LoopLastWave = true;
            LoopDelay = 5.5f;

            Waves = new List<WaveData>
            {
                new() { WaveId = 1, StartTime = 2.5f, ScoutCount = 5, GnawerCount = 2, TankCount = 0, IntraSpawnInterval = 0.72f, Comment = "Immediate pressure" },
                new() { WaveId = 2, StartTime = 18f,  ScoutCount = 5, GnawerCount = 3, TankCount = 1, IntraSpawnInterval = 0.62f, Comment = "Pushes first defensive spend" },
                new() { WaveId = 3, StartTime = 41f,  ScoutCount = 4, GnawerCount = 4, TankCount = 1, IntraSpawnInterval = 0.58f, Comment = "Heavy mixed wave" },
                new() { WaveId = 4, StartTime = 70f,  ScoutCount = 5, GnawerCount = 4, TankCount = 1, IntraSpawnInterval = 0.55f, Comment = "Loop wave" }
            };
        }

        [ContextMenu("Waves for level 7")]
        private void FillLevel7()
        {
            LoopLastWave = true;
            LoopDelay = 5.5f;

            Waves = new List<WaveData>
            {
                new() { WaveId = 1, StartTime = 2.5f, ScoutCount = 5, GnawerCount = 2, TankCount = 0, IntraSpawnInterval = 0.7f, Comment = "Quick opening" },
                new() { WaveId = 2, StartTime = 18f,  ScoutCount = 5, GnawerCount = 3, TankCount = 1, IntraSpawnInterval = 0.60f, Comment = "Build check" },
                new() { WaveId = 3, StartTime = 40f,  ScoutCount = 5, GnawerCount = 4, TankCount = 1, IntraSpawnInterval = 0.56f, Comment = "Mid game pressure" },
                new() { WaveId = 4, StartTime = 70f,  ScoutCount = 5, GnawerCount = 4, TankCount = 2, IntraSpawnInterval = 0.52f, Comment = "Loop wave" }
            };
        }

        [ContextMenu("Waves for level 8")]
        private void FillLevel8()
        {
            LoopLastWave = true;
            LoopDelay = 5f;

            Waves = new List<WaveData>
            {
                new() { WaveId = 1, StartTime = 2f,  ScoutCount = 5, GnawerCount = 2, TankCount = 1, IntraSpawnInterval = 0.68f, Comment = "Hard opening" },
                new() { WaveId = 2, StartTime = 18f, ScoutCount = 5, GnawerCount = 4, TankCount = 1, IntraSpawnInterval = 0.58f, Comment = "Pressure on barricades" },
                new() { WaveId = 3, StartTime = 42f, ScoutCount = 5, GnawerCount = 4, TankCount = 2, IntraSpawnInterval = 0.54f, Comment = "Heavy mixed" },
                new() { WaveId = 4, StartTime = 74f, ScoutCount = 6, GnawerCount = 4, TankCount = 2, IntraSpawnInterval = 0.50f, Comment = "Loop wave" }
            };
        }

        [ContextMenu("Waves for level 9")]
        private void FillLevel9()
        {
            LoopLastWave = true;
            LoopDelay = 4.75f;

            Waves = new List<WaveData>
            {
                new() { WaveId = 1, StartTime = 2f,  ScoutCount = 5, GnawerCount = 3, TankCount = 1, IntraSpawnInterval = 0.65f, Comment = "Aggressive opening" },
                new() { WaveId = 2, StartTime = 17f, ScoutCount = 5, GnawerCount = 4, TankCount = 1, IntraSpawnInterval = 0.56f, Comment = "Sustained pressure" },
                new() { WaveId = 3, StartTime = 40f, ScoutCount = 5, GnawerCount = 4, TankCount = 2, IntraSpawnInterval = 0.52f, Comment = "High pressure" },
                new() { WaveId = 4, StartTime = 73f, ScoutCount = 6, GnawerCount = 5, TankCount = 2, IntraSpawnInterval = 0.48f, Comment = "Loop wave" }
            };
        }

        [ContextMenu("Waves for level 10")]
        private void FillLevel10()
        {
            LoopLastWave = true;
            LoopDelay = 4.5f;

            Waves = new List<WaveData>
            {
                new() { WaveId = 1, StartTime = 2f,  ScoutCount = 5, GnawerCount = 3, TankCount = 1, IntraSpawnInterval = 0.62f, Comment = "Final opening" },
                new() { WaveId = 2, StartTime = 16f, ScoutCount = 5, GnawerCount = 4, TankCount = 2, IntraSpawnInterval = 0.54f, Comment = "Strong pressure" },
                new() { WaveId = 3, StartTime = 40f, ScoutCount = 6, GnawerCount = 4, TankCount = 2, IntraSpawnInterval = 0.50f, Comment = "Late game check" },
                new() { WaveId = 4, StartTime = 76f, ScoutCount = 6, GnawerCount = 5, TankCount = 3, IntraSpawnInterval = 0.46f, Comment = "Loop wave" }
            };
        }
    }
}