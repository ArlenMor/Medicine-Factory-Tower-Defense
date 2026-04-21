using System;
using _Project.Code.Scripts.Data;

namespace _Project.Code.Scripts.Configs
{
    [Serializable]
    public class UpgradeDef
    {
        public UpgradeType Type;
        public float[] Multipliers;
        public int[] Costs;
    }
}