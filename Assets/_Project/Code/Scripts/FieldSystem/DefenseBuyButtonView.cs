using _Project.Code.Scripts.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.Scripts.BattleField
{
    public class DefenseBuyButtonView : MonoBehaviour
    {
        private static readonly Color ColorAffordable    = Color.white;
        private static readonly Color ColorNotAffordable = new Color(0x9A / 255f, 0x23 / 255f, 0x24 / 255f);

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

            GameData.Instance.OnResourcesChanged += RefreshPriceColor;
            RefreshPriceColor();
        }

        private void OnDestroy()
        {
            if (GameData.Instance != null)
                GameData.Instance.OnResourcesChanged -= RefreshPriceColor;
        }

        private void RefreshPriceColor()
        {
            if (_priceText == null) return;
            int credits = GameData.Instance.Resources[ResourceType.Credit];
            _priceText.color = credits >= Price ? ColorAffordable : ColorNotAffordable;
        }
    }
}
