using System;
using UnityEngine;

namespace _Project.Code.Scripts.Tutorial
{
    public enum HighlightTargetType
    {
        UI,
        World,
    }

    [Serializable]
    public class TutorialHighlightTarget
    {
        public HighlightTargetType Type = HighlightTargetType.UI;

        [Tooltip("UI-элемент, который нужно выделить (для Type = UI)")]
        public RectTransform UITarget;

        [Tooltip("Мировой объект, который нужно выделить (для Type = World)")]
        public Transform WorldTarget;

        [Tooltip("Размер подсветки в мировых единицах — используется если у объекта нет Renderer (для Type = World)")]
        public float WorldSize = 1f;

        [Tooltip("Отступ вокруг выделенной области в пикселях")]
        public float Padding = 8f;
    }
}
