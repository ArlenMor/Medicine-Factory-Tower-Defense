using UnityEngine;

namespace _Project.Code.Scripts.Cursor
{
    public class ChangeCursor : MonoBehaviour
    {
        [SerializeField] private Texture2D _cursorTexture;
        [SerializeField] private Vector2 _hotSpot = Vector2.zero;
        [SerializeField] private CursorMode _cursorMode = CursorMode.Auto;

        private void Start()
        {
            UnityEngine.Cursor.SetCursor(_cursorTexture, _hotSpot, _cursorMode);
        }

        private void OnDestroy()
        {
            UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
}