using System;
using _Project.Code.Scripts.Garden;
using UnityEngine;

namespace _Project.Code.Scripts.UIService
{
    public class PlantChoosePanelSettings: PanelSettings
    {
        public Vector2 Position { get; set; }
        public Action<PlantType> Callback { get; set; }
    }
}