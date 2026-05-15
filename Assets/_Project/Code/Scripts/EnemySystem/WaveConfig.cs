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

        /// <summary>
        /// Заполняет дефолтные волны для короткой сессии 3-7 минут.
        /// Старт игры более динамичный: первая волна приходит рано.
        /// </summary>
        [ContextMenu("Fill Default Waves")]
        private void FillDefaultWaves()
        {
            Waves = new List<WaveData>
            {
                new() { WaveId = 1,  StartTime = 8f,   ScoutCount = 2, GnawerCount = 0, TankCount = 0, IntraSpawnInterval = 0.85f, Comment = "Первая мягкая волна почти сразу после старта" },
                new() { WaveId = 2,  StartTime = 13f,  ScoutCount = 2, GnawerCount = 0, TankCount = 0, IntraSpawnInterval = 0.5f, Comment = "" },
                new() { WaveId = 3,  StartTime = 20f,  ScoutCount = 3, GnawerCount = 0, TankCount = 0, IntraSpawnInterval = 0.8f,  Comment = "Поддерживает раннюю активность игрока" },
                new() { WaveId = 4,  StartTime = 35f,  ScoutCount = 2, GnawerCount = 1, TankCount = 0, IntraSpawnInterval = 0.8f,  Comment = "Первый Gnawer" },

                new() { WaveId = 5,  StartTime = 55f,  ScoutCount = 4, GnawerCount = 0, TankCount = 0, IntraSpawnInterval = 0.75f },
                new() { WaveId = 6,  StartTime = 78f,  ScoutCount = 3, GnawerCount = 2, TankCount = 0, IntraSpawnInterval = 0.75f },
                new() { WaveId = 7,  StartTime = 105f, ScoutCount = 2, GnawerCount = 3, TankCount = 0, IntraSpawnInterval = 0.72f, Comment = "К этому моменту желательно иметь первую защиту" },

                new() { WaveId = 8,  StartTime = 135f, ScoutCount = 3, GnawerCount = 2, TankCount = 1, IntraSpawnInterval = 0.82f, Comment = "Первый Tank" },
                new() { WaveId = 9,  StartTime = 170f, ScoutCount = 5, GnawerCount = 2, TankCount = 0, IntraSpawnInterval = 0.7f },
                new() { WaveId = 10, StartTime = 205f, ScoutCount = 3, GnawerCount = 3, TankCount = 1, IntraSpawnInterval = 0.78f },

                new() { WaveId = 11, StartTime = 245f, ScoutCount = 6, GnawerCount = 3, TankCount = 0, IntraSpawnInterval = 0.65f },
                new() { WaveId = 12, StartTime = 290f, ScoutCount = 4, GnawerCount = 4, TankCount = 1, IntraSpawnInterval = 0.72f },
                new() { WaveId = 13, StartTime = 340f, ScoutCount = 5, GnawerCount = 4, TankCount = 1, IntraSpawnInterval = 0.7f },

                new() { WaveId = 14, StartTime = 395f, ScoutCount = 6, GnawerCount = 4, TankCount = 2, IntraSpawnInterval = 0.78f, Comment = "Финальная волна" },
            };
        }
    }
}