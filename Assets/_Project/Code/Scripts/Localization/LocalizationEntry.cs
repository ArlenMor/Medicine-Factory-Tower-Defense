using UnityEngine;

namespace _Project.Code.Scripts.Localization
{
    [System.Serializable]
    public class LocalizationEntry
    {
        public string Key;
        [TextArea] public string English;
        [TextArea] public string Russian;
        public Sprite EnglishSprite;
        public Sprite RussianSprite;
    }
}
