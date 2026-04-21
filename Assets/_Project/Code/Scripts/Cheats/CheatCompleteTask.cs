using _Project.Code.Scripts.TaskSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Code.Scripts.Cheats
{
    public class CheatCompleteTask : MonoBehaviour
    {
        [SerializeField] private Key _key = Key.F1;

        private ITaskService _taskService;

        public void Initialize(ITaskService taskService)
        {
            _taskService = taskService;
        }

        private void Update()
        {
            if (_taskService == null) return;
            if (Keyboard.current != null && Keyboard.current[_key].wasPressedThisFrame && _taskService.HasActiveTask)
                _taskService.CompleteCurrentTask();
        }
    }
}
