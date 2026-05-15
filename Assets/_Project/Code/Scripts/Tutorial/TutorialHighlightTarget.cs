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

        public string TargetId;

        public RectTransform UITarget;

        public Transform WorldTarget;

        public float WorldSize = 1f;

        public float Padding = 8f;
    }
}
