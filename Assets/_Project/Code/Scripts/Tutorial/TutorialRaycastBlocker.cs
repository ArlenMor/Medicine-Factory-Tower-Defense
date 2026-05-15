using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.Scripts.Tutorial
{
    [RequireComponent(typeof(Graphic))]
    public class TutorialRaycastBlocker : MonoBehaviour, ICanvasRaycastFilter
    {
        private readonly Vector4[] _holes = new Vector4[4];
        private bool _isBlocking;

        public void SetBlocking(bool blocking)
        {
            _isBlocking = blocking;
        }

        public void SetHoles(Vector4[] holes)
        {
            int count = Mathf.Min(holes.Length, _holes.Length);
            
            for (int i = 0; i < count; i++)
                _holes[i] = holes[i];

            for (int i = count; i < _holes.Length; i++)
                _holes[i] = new Vector4(-1, -1, 0, 0);
        }

        // ICanvasRaycastFilter: true = оверлей перехватывает клик (блокировка),
        //                        false = оверлей игнорирует точку (клик проходит насквозь)
        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            if (!_isBlocking) return false;

            float nx = screenPoint.x / Screen.width;
            float ny = screenPoint.y / Screen.height;

            for (int i = 0; i < _holes.Length; i++)
            {
                var h = _holes[i];
                if (h.x < 0 || h.z <= 0 || h.w <= 0) continue; // невалидная дыра

                if (nx >= h.x && nx <= h.x + h.z &&
                    ny >= h.y && ny <= h.y + h.w)
                    return false; // внутри дыры — пропустить клик к подсвеченному элементу
            }

            return true; // вне дыр — оверлей блокирует клик
        }
    }
}
