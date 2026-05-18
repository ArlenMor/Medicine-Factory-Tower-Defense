using System;
using UnityEngine;

namespace _Project.Code.Scripts.UIService
{
    public class RemovePlantPanelSettings : PanelSettings
    {
        public Vector2 Position { get; set; }
        public Action Callback { get; set; }
        public string TutorialTargetId { get; set; }
    }
}