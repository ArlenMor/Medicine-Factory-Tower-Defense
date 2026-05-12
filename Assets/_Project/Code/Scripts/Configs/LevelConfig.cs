using _Project.Code.Scripts.EnemySystem;
using UnityEngine;

namespace _Project.Code.Scripts.Configs
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "GameConfig/LevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        public WaveConfig WaveConfig;
        public TaskConfig TaskConfig;
    }
}
