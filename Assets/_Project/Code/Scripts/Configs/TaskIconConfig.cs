using System;
using System.Collections.Generic;
using _Project.Code.Scripts.Data.TaskData;
using UnityEngine;

namespace _Project.Code.Scripts.Configs
{
    [CreateAssetMenu(fileName = "TaskIconConfig", menuName = "TaskIconConfig")]
    public class TaskIconConfig : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public MedicationsType Type;
            public Sprite Icon;
        }

        [SerializeField] private List<Entry> _entries = new();
        [SerializeField] private Sprite _finalIcon;
        public Sprite FinalIcon => _finalIcon;

        private Dictionary<MedicationsType, Sprite> _lookup;

        public Sprite GetIcon(MedicationsType type)
        {
            if (_lookup == null)
            {
                _lookup = new Dictionary<MedicationsType, Sprite>();
                foreach (var entry in _entries)
                    _lookup[entry.Type] = entry.Icon;
            }

            return _lookup.TryGetValue(type, out var sprite) ? sprite : null;
        }
    }
}
