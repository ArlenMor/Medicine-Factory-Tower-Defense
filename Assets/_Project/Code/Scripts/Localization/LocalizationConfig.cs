using System.Collections.Generic;
using UnityEngine;

namespace _Project.Code.Scripts.Localization
{
    [CreateAssetMenu(fileName = "LocalizationConfig", menuName = "Localization/LocalizationConfig")]
    public class LocalizationConfig : ScriptableObject
    {
        public List<LocalizationEntry> Entries = new();
    }
}
