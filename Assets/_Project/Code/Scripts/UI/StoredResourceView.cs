using System;
using _Project.Code.Scripts.Data;
using TMPro;
using UnityEngine;

namespace _Project.Code.Scripts.UI
{
    public class StoredResourceView: MonoBehaviour
    {
        [SerializeField] private ResourceType resourceType;
        [SerializeField] private TMP_Text _resourceAmount;

        public void Initialize()
        {
            GameData.Instance.OnResourcesChanged += OnResourcesChanged;
            OnResourcesChanged();
        }

        private void OnResourcesChanged()
        {
            _resourceAmount.text = GameData.Instance.Resources[resourceType].ToString();
        }

        private void OnDestroy()
        {
            GameData.Instance.OnResourcesChanged -= OnResourcesChanged;
        }
    }
}