using System;
using System.Collections.Generic;
using _Project.Code.Scripts.Data.TaskData;
using UnityEngine;

namespace _Project.Code.Scripts.Localization
{
    public class LocalizationService
    {
        public Language CurrentLocale { get; private set; }
        public event Action OnLocaleChanged;

        private readonly Dictionary<string, LocalizationEntry> _entries;

        public LocalizationService(LocalizationConfig config, Language defaultLocale)
        {
            _entries = new Dictionary<string, LocalizationEntry>(config.Entries.Count);
            foreach (var entry in config.Entries)
            {
                if (!string.IsNullOrEmpty(entry.Key) && !_entries.ContainsKey(entry.Key))
                    _entries.Add(entry.Key, entry);
            }

            CurrentLocale = defaultLocale;
        }

        public void SetLocale(Language lang)
        {
            if (CurrentLocale == lang) return;

            CurrentLocale = lang;
            OnLocaleChanged?.Invoke();
        }

        public string GetString(string key, params object[] args)
        {
            if (!_entries.TryGetValue(key, out var entry))
            {
                Debug.LogWarning($"[Localization] Key not found: {key}");
                return $"[{key}]";
            }

            var text = CurrentLocale switch
            {
                Language.Ru when !string.IsNullOrEmpty(entry.Russian) => entry.Russian,
                Language.En when !string.IsNullOrEmpty(entry.English) => entry.English,
                _ when !string.IsNullOrEmpty(entry.English) => entry.English,
                _ => entry.Russian
            };

            if (string.IsNullOrEmpty(text))
            {
                Debug.LogWarning($"[Localization] Empty value for key: {key} ({CurrentLocale})");
                return $"[{key}]";
            }

            return args.Length > 0 ? string.Format(text, args) : text;
        }

        public Sprite GetSprite(string key)
        {
            if (!_entries.TryGetValue(key, out var entry)) return null;

            return CurrentLocale switch
            {
                Language.Ru when entry.RussianSprite != null => entry.RussianSprite,
                Language.En when entry.EnglishSprite != null => entry.EnglishSprite,
                _ when entry.EnglishSprite != null => entry.EnglishSprite,
                _ => entry.RussianSprite
            };
        }

        public string GetMedicationName(MedicationsType type)
        {
            return GetString("medication_" + type.ToString());
        }
    }
}
