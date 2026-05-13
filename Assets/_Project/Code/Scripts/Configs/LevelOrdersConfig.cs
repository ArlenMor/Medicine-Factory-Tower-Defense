using System.Collections.Generic;
using _Project.Code.Scripts.Data;
using UnityEngine;

namespace _Project.Code.Scripts.Configs
{
    [CreateAssetMenu(fileName = "LevelOrdersConfig", menuName = "GameConfig/LevelOrdersConfig")]
    public class LevelOrdersConfig : ScriptableObject
    {
        public List<LevelOrderEntry> Entries;
    }
}
