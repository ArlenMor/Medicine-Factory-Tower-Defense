using _Project.Code.Scripts.Game.LvlController;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Code.Scripts.Cheats
{
    public class CheatSkipLevel : MonoBehaviour
    {
        [SerializeField] private Key _key = Key.F2;

        private LevelController _levelController;

        public void Initialize(LevelController levelController)
        {
            _levelController = levelController;
        }

        private void Update()
        {
            if (_levelController == null) return;
            if (Keyboard.current != null && Keyboard.current[_key].wasPressedThisFrame)
                _levelController.ForceVictory();
        }
    }
}
