using System;
using System.Collections.Generic;
using _Project.Code.Scripts.Data;
using UnityEngine;

namespace _Project.Code.Scripts.Configs
{
    [CreateAssetMenu(fileName = "ResourceIconConfig", menuName = "ResourceIconConfig")]
    public class ResourceIconConfig : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public ResourceType Type;
            public Sprite Icon;
        }

        [SerializeField] private List<Entry> _entries = new();

        private Dictionary<ResourceType, Sprite> _lookup;

        public Sprite GetIcon(ResourceType type)
        {
            if (_lookup == null)
            {
                _lookup = new Dictionary<ResourceType, Sprite>();
                foreach (var entry in _entries)
                    _lookup[entry.Type] = entry.Icon;
            }

            return _lookup.TryGetValue(type, out var sprite) ? sprite : null;
        }
    }
}
