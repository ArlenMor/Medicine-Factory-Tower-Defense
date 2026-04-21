using System;
using System.Globalization;
using _Project.Code.Scripts.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.Scripts.UI
{
    public class UpgradeButtonView: MonoBehaviour
    {
        [SerializeField] private UpgradeType _type;
        [SerializeField] private Button _button;
        [SerializeField] private Transform _upgradeStateParent;
        [SerializeField] private TMP_Text _currentMultiplier;
        [SerializeField] private TMP_Text _nextMultiplier;
        [SerializeField] private TMP_Text _upgradeCost;
        [SerializeField] private Transform _maxStateParent;
        [SerializeField] private TMP_Text _maxText;

        private int _upgradeStep;
        private Action<UpgradeButtonView> _clickAction;

        public UpgradeType Type => _type;
        public bool IsMax { get; private set; }

        public void Initialize(Action<UpgradeButtonView> clickAction)
        {
            _clickAction = clickAction;
            _button.onClick.AddListener(OnClick);
        }

        public void RefreshText(
            string currentMultiplier, 
            string nextMultiplier = "",
            string upgradeCost = "")
        {
            if (IsMax)
            {
                _maxText.text = $"MAX: x{currentMultiplier}";
            }
            else
            {
                _currentMultiplier.text = $"x{currentMultiplier}";
                _nextMultiplier.text = $"x{nextMultiplier}";
                _upgradeCost.text = upgradeCost;
            }
            
            _maxStateParent.gameObject.SetActive(IsMax);
            _upgradeStateParent.gameObject.SetActive(!IsMax);
        }

        public void SetMax(bool isMax)
        {
            IsMax = isMax;
        }

        private void OnClick()
        {
            _clickAction?.Invoke(this);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnClick);
        }
    }
}