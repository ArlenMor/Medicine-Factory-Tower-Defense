using UnityEngine;

namespace _Project.Code.Scripts.BattleField
{
    public class DefenseShopView : MonoBehaviour
    {
        [SerializeField] private DefenseBuyButtonView[] _buttons;

        public void Initialize(DefenseShopConfig config)
        {
            foreach (var button in _buttons)
            {
                foreach (var item in config.Items)
                {
                    if (item.Type == button.DefenseType)
                    {
                        button.Initialize(item);
                        break;
                    }
                }
            }
        }
    }
}
