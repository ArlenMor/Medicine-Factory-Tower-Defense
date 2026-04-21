using System.Collections.Generic;
using UnityEngine;

namespace _Project.Code.Scripts.BattleField
{
    [CreateAssetMenu(fileName = "DefenseShopConfig", menuName = "GameConfig/DefenseShopConfig")]
    public class DefenseShopConfig : ScriptableObject
    {
        public List<DefenseItemData> Items;
    }
}
