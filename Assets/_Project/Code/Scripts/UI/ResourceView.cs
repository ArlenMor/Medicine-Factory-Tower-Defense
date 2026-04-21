using _Project.Code.Scripts.Configs;
using _Project.Code.Scripts.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceView : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _amountText;
    [SerializeField] private Color _availableColor;
    [SerializeField] private Color _unavailableColor;

    public void SetData(ResourceType type, int available, int required, ResourceIconConfig iconConfig)
    {
        _icon.sprite = iconConfig.GetIcon(type);
        _amountText.text = $"{available}/{required}";
        _amountText.color = available >= required ? _availableColor : _unavailableColor;
    }

    public void SetData(ResourceType type, int value, ResourceIconConfig iconConfig)
    {
        _icon.sprite = iconConfig.GetIcon(type);
        _amountText.text = value.ToString();
        _amountText.color = Color.white;
    }

    public void SetActive(bool active)
    {
        _icon.gameObject.SetActive(active);
        _amountText.gameObject.SetActive(active);
    }
}