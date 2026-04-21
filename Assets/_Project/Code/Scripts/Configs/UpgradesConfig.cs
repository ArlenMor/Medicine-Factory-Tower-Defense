using UnityEngine;

namespace _Project.Code.Scripts.Configs
{
    [CreateAssetMenu(fileName = "UpgradesConfig", menuName = "GameConfig/UpgradesConfig")]
    public class UpgradesConfig: ScriptableObject
    {
        public UpgradeDef[] Upgrades;
    }
}