using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.Scripts.BattleField
{
    public class DefenseBuyButtonView : MonoBehaviour
    {
        [SerializeField] private DefenseType _defenseType;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _priceText;

        public DefenseType DefenseType => _defenseType;
        public int Price { get; private set; }

        public void Initialize(DefenseItemData data)
        {
            Price = data.CreditCost;
            if (_icon != null) _icon.sprite = data.Icon;
            if (_priceText != null) _priceText.text = data.CreditCost.ToString();
        }
    }
}
