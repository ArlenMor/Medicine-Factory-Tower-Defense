using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Code.Scripts.UIService
{
    [CreateAssetMenu(fileName = "PanelPrefabConfig", menuName = "UI/PanelPrefabConfig")]
    public class PanelPrefabConfig : ScriptableObject
    {
        [Serializable]
        public struct PanelEntry
        {
            public PanelType Type;
            public BasePanel Prefab;
        }

        [SerializeField] private List<PanelEntry> _panels = new();

        private Dictionary<PanelType, BasePanel> _lookup;

        public BasePanel GetPrefab(PanelType type)
        {
            if (_lookup == null)
            {
                _lookup = new Dictionary<PanelType, BasePanel>();
                foreach (var entry in _panels)
                    _lookup[entry.Type] = entry.Prefab;
            }

            return _lookup.TryGetValue(type, out var prefab) ? prefab : null;
        }
    }
}
