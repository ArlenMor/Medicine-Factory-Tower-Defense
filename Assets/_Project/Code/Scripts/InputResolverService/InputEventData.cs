using System;
using UnityEngine;

namespace _Project.Code.Scripts.InputResolverService
{
    public enum MouseButton
    {
        Left = 0,
        Right = 1,
        Middle = 2
    }

    public enum InputTarget
    {
        None,
        Canvas,
        World
    }

    public readonly struct InputEventData
    {
        public readonly MouseButton Button;
        public readonly InputTarget Target;
        public readonly Vector2 ScreenPosition;
        public readonly GameObject HitObject;
        public readonly RaycastHit? WorldHit;
        public readonly RaycastHit2D? WorldHit2D;

        public InputEventData(MouseButton button, InputTarget target, Vector2 screenPosition,
            GameObject hitObject, RaycastHit? worldHit, RaycastHit2D? worldHit2D = null)
        {
            Button = button;
            Target = target;
            ScreenPosition = screenPosition;
            HitObject = hitObject;
            WorldHit = worldHit;
            WorldHit2D = worldHit2D;
        }

        public bool IsCanvasHit => Target == InputTarget.Canvas;
        public bool IsWorldHit => Target == InputTarget.World;
    }
}
