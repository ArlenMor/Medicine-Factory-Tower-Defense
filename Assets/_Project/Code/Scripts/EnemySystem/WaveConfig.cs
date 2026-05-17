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
        private void WavesForLevel4()
        {
            LoopLastWave = true;
            LoopDelay = 14f;

            Waves = new List<WaveData>
            {
                new()
                {
                    WaveId = 1,
                    StartTime = 12f,
                    ScoutCount = 2,
                    GnawerCount = 0,
                    TankCount = 0,
                    IntraSpawnInterval = 1.2f,
                    Comment = "L4 warmup"
                },
                new()
                {
                    WaveId = 2,
                    StartTime = 38f,
                    ScoutCount = 2,
                    GnawerCount = 1,
                    TankCount = 0,
                    IntraSpawnInterval = 1.1f,
                    Comment = "L4 first pressure"
                },
                new()
                {
                    WaveId = 3,
                    StartTime = 68f,
                    ScoutCount = 3,
                    GnawerCount = 1,
                    TankCount = 0,
                    IntraSpawnInterval = 1.0f,
                    Comment = "L4 loop wave"
                }
            };
        }

        [ContextMenu("Waves for level 5")]
        private void WavesForLevel5()
        {
            LoopLastWave = true;
            LoopDelay = 13f;

            Waves = new List<WaveData>
            {
                new()
                {
                    WaveId = 1,
                    StartTime = 10f,
                    ScoutCount = 2,
                    GnawerCount = 0,
                    TankCount = 0,
                    IntraSpawnInterval = 1.15f,
                    Comment = "L5 opener"
                },
                new()
                {
                    WaveId = 2,
                    StartTime = 32f,
                    ScoutCount = 3,
                    GnawerCount = 1,
                    TankCount = 0,
                    IntraSpawnInterval = 1.05f,
                    Comment = "L5 light mixed"
                },
                new()
                {
                    WaveId = 3,
                    StartTime = 58f,
                    ScoutCount = 2,
                    GnawerCount = 2,
                    TankCount = 0,
                    IntraSpawnInterval = 1.0f,
                    Comment = "L5 gnawer pressure"
                },
                new()
                {
                    WaveId = 4,
                    StartTime = 84f,
                    ScoutCount = 4,
                    GnawerCount = 2,
                    TankCount = 0,
                    IntraSpawnInterval = 0.95f,
                    Comment = "L5 loop wave"
                }
            };
        }

        [ContextMenu("Waves for level 6")]
        private void WavesForLevel6()
        {
            LoopLastWave = true;
            LoopDelay = 12f;

            Waves = new List<WaveData>
            {
                new()
                {
                    WaveId = 1,
                    StartTime = 10f,
                    ScoutCount = 3,
                    GnawerCount = 1,
                    TankCount = 0,
                    IntraSpawnInterval = 1.05f,
                    Comment = "L6 opener"
                },
                new()
                {
                    WaveId = 2,
                    StartTime = 30f,
                    ScoutCount = 2,
                    GnawerCount = 2,
                    TankCount = 0,
                    IntraSpawnInterval = 1.0f,
                    Comment = "L6 mixed"
                },
                new()
                {
                    WaveId = 3,
                    StartTime = 54f,
                    ScoutCount = 4,
                    GnawerCount = 2,
                    TankCount = 0,
                    IntraSpawnInterval = 0.95f,
                    Comment = "L6 pressure"
                },
                new()
                {
                    WaveId = 4,
                    StartTime = 78f,
                    ScoutCount = 3,
                    GnawerCount = 2,
                    TankCount = 1,
                    IntraSpawnInterval = 0.95f,
                    Comment = "L6 first tank"
                },
                new()
                {
                    WaveId = 5,
                    StartTime = 96f,
                    ScoutCount = 4,
                    GnawerCount = 2,
                    TankCount = 1,
                    IntraSpawnInterval = 0.9f,
                    Comment = "L6 loop wave"
                }
            };
        }

        [ContextMenu("Waves for level 7")]
        private void WavesForLevel7()
        {
            LoopLastWave = true;
            LoopDelay = 11f;

            Waves = new List<WaveData>
            {
                new()
                {
                    WaveId = 1,
                    StartTime = 8f,
                    ScoutCount = 3,
                    GnawerCount = 2,
                    TankCount = 0,
                    IntraSpawnInterval = 1.0f,
                    Comment = "L7 hard opener"
                },
                new()
                {
                    WaveId = 2,
                    StartTime = 28f,
                    ScoutCount = 4,
                    GnawerCount = 2,
                    TankCount = 0,
                    IntraSpawnInterval = 0.95f,
                    Comment = "L7 pressure"
                },
                new()
                {
                    WaveId = 3,
                    StartTime = 50f,
                    ScoutCount = 3,
                    GnawerCount = 3,
                    TankCount = 1,
                    IntraSpawnInterval = 0.9f,
                    Comment = "L7 tank mixed"
                },
                new()
                {
                    WaveId = 4,
                    StartTime = 74f,
                    ScoutCount = 5,
                    GnawerCount = 3,
                    TankCount = 1,
                    IntraSpawnInterval = 0.85f,
                    Comment = "L7 late pressure"
                },
                new()
                {
                    WaveId = 5,
                    StartTime = 96f,
                    ScoutCount = 4,
                    GnawerCount = 4,
                    TankCount = 1,
                    IntraSpawnInterval = 0.85f,
                    Comment = "L7 loop wave"
                }
            };
        }

        [ContextMenu("Waves for level 8")]
        private void WavesForLevel8()
        {
            LoopLastWave = true;
            LoopDelay = 10f;

            Waves = new List<WaveData>
            {
                new()
                {
                    WaveId = 1,
                    StartTime = 8f,
                    ScoutCount = 4,
                    GnawerCount = 2,
                    TankCount = 0,
                    IntraSpawnInterval = 0.95f,
                    Comment = "L8 opener"
                },
                new()
                {
                    WaveId = 2,
                    StartTime = 26f,
                    ScoutCount = 3,
                    GnawerCount = 3,
                    TankCount = 0,
                    IntraSpawnInterval = 0.9f,
                    Comment = "L8 gnawers"
                },
                new()
                {
                    WaveId = 3,
                    StartTime = 46f,
                    ScoutCount = 5,
                    GnawerCount = 3,
                    TankCount = 1,
                    IntraSpawnInterval = 0.85f,
                    Comment = "L8 tank 1"
                },
                new()
                {
                    WaveId = 4,
                    StartTime = 68f,
                    ScoutCount = 4,
                    GnawerCount = 4,
                    TankCount = 1,
                    IntraSpawnInterval = 0.85f,
                    Comment = "L8 pressure"
                },
                new()
                {
                    WaveId = 5,
                    StartTime = 90f,
                    ScoutCount = 6,
                    GnawerCount = 4,
                    TankCount = 1,
                    IntraSpawnInterval = 0.8f,
                    Comment = "L8 late"
                },
                new()
                {
                    WaveId = 6,
                    StartTime = 108f,
                    ScoutCount = 5,
                    GnawerCount = 4,
                    TankCount = 2,
                    IntraSpawnInterval = 0.8f,
                    Comment = "L8 loop wave"
                }
            };
        }

        [ContextMenu("Waves for level 9")]
        private void WavesForLevel9()
        {
            LoopLastWave = true;
            LoopDelay = 9f;

            Waves = new List<WaveData>
            {
                new()
                {
                    WaveId = 1,
                    StartTime = 8f,
                    ScoutCount = 5,
                    GnawerCount = 3,
                    TankCount = 0,
                    IntraSpawnInterval = 0.9f,
                    Comment = "L9 opener"
                },
                new()
                {
                    WaveId = 2,
                    StartTime = 24f,
                    ScoutCount = 4,
                    GnawerCount = 4,
                    TankCount = 1,
                    IntraSpawnInterval = 0.85f,
                    Comment = "L9 tank pressure"
                },
                new()
                {
                    WaveId = 3,
                    StartTime = 44f,
                    ScoutCount = 6,
                    GnawerCount = 4,
                    TankCount = 1,
                    IntraSpawnInterval = 0.8f,
                    Comment = "L9 mixed"
                },
                new()
                {
                    WaveId = 4,
                    StartTime = 64f,
                    ScoutCount = 5,
                    GnawerCount = 5,
                    TankCount = 1,
                    IntraSpawnInterval = 0.8f,
                    Comment = "L9 heavy mixed"
                },
                new()
                {
                    WaveId = 5,
                    StartTime = 84f,
                    ScoutCount = 7,
                    GnawerCount = 5,
                    TankCount = 2,
                    IntraSpawnInterval = 0.75f,
                    Comment = "L9 late heavy"
                },
                new()
                {
                    WaveId = 6,
                    StartTime = 106f,
                    ScoutCount = 6,
                    GnawerCount = 6,
                    TankCount = 2,
                    IntraSpawnInterval = 0.75f,
                    Comment = "L9 loop wave"
                }
            };
        }

        [ContextMenu("Waves for level 10")]
        private void WavesForLevel10()
        {
            LoopLastWave = true;
            LoopDelay = 12f;

            Waves = new List<WaveData>
            {
                new()
                {
                    WaveId = 1,
                    StartTime = 8f,
                    ScoutCount = 5,
                    GnawerCount = 3,
                    TankCount = 0,
                    IntraSpawnInterval = 0.9f,
                    Comment = "L10 opener, no tank"
                },
                new()
                {
                    WaveId = 2,
                    StartTime = 28f,
                    ScoutCount = 5,
                    GnawerCount = 4,
                    TankCount = 1,
                    IntraSpawnInterval = 0.85f,
                    Comment = "L10 first tank"
                },
                new()
                {
                    WaveId = 3,
                    StartTime = 50f,
                    ScoutCount = 6,
                    GnawerCount = 4,
                    TankCount = 1,
                    IntraSpawnInterval = 0.8f,
                    Comment = "L10 mixed pressure"
                },
                new()
                {
                    WaveId = 4,
                    StartTime = 72f,
                    ScoutCount = 6,
                    GnawerCount = 5,
                    TankCount = 1,
                    IntraSpawnInterval = 0.8f,
                    Comment = "L10 sustained pressure"
                },
                new()
                {
                    WaveId = 5,
                    StartTime = 94f,
                    ScoutCount = 7,
                    GnawerCount = 5,
                    TankCount = 2,
                    IntraSpawnInterval = 0.75f,
                    Comment = "L10 late heavy"
                },
                new()
                {
                    WaveId = 6,
                    StartTime = 116f,
                    ScoutCount = 7,
                    GnawerCount = 6,
                    TankCount = 2,
                    IntraSpawnInterval = 0.75f,
                    Comment = "L10 loop wave"
                }
            };
        }

    }
}