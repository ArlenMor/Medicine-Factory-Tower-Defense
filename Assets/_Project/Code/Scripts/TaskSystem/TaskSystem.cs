using System;
using System.Collections.Generic;
using _Project.Code.Scripts.Data.TaskData;
using _Project.Code.Scripts.ServiceLocator;
using _Project.Code.Scripts.Tutorial;
using UnityEngine;

namespace _Project.Code.Scripts.TaskSystem
{
    public class TaskService : ITaskService
    {
        public event Action<TaskData> OnTaskStarted;
        public event Action<TaskData> OnTaskCompleted;
        public event Action OnAllTasksCompleted;

        public bool HasActiveTask => _currentTask.HasValue;
        public TaskData? CurrentTask => _currentTask;

        public int CompletedTasksCount => _completedTasksCount;

        public int GoalTaskIndex => _tasks.Count;

        private List<TaskData> _tasks;
        private int _currentIndex;
        private TaskData? _currentTask;
        private int _completedTasksCount;

        public TaskService()
        {
            _tasks = new List<TaskData>();
            StartNext();
        }

        public void Reset(List<TaskData> tasks)
        {
            _tasks = tasks;
            _currentIndex = 0;
            _currentTask = null;
            _completedTasksCount = 0;
            StartNext();
        }

        public void CompleteCurrentTask()
        {
            if (!_currentTask.HasValue)
            {
                Debug.LogWarning("[TaskService] No active task to complete.");
                return;
            }

            var completed = _currentTask.Value;
            _currentTask = null;
            _completedTasksCount++;
            OnTaskCompleted?.Invoke(completed);

            if (S.TryGet<ITutorialService>(out var tutorial))
                tutorial.NotifyEvent(TutorialEventType.TaskCompleted);

            StartNext();
        }

        private void StartNext()
        {
            if (_tasks == null || _tasks.Count == 0)
                return;

            if (_currentIndex >= _tasks.Count)
            {
                OnAllTasksCompleted?.Invoke();
                Debug.Log("[TaskService] All tasks completed.");
                return;
            }

            _currentTask = _tasks[_currentIndex++];
            OnTaskStarted?.Invoke(_currentTask.Value);
        }
    }
}