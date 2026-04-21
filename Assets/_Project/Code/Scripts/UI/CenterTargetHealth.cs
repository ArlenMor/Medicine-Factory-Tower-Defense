using TMPro;
using UnityEngine;

namespace _Project.Code.Scripts.UI
{
    public class CenterTargetHealth: MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        
        public void Initialize(float maxHp)
        {
            SetHealth(maxHp);
        }
        
        public void SetHealth(float currentHp)
        {
            _text.text = $"{Mathf.CeilToInt(currentHp)}";
        }
    }
}