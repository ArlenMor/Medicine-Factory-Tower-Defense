using System;
using System.Collections.Generic;
using _Project.Code.Scripts.Data.TaskData;

namespace _Project.Code.Scripts.TaskSystem
{
    public interface ITaskService
    {
        event Action<TaskData> OnTaskStarted;
        event Action<TaskData> OnTaskCompleted;
        event Action OnAllTasksCompleted;

        bool HasActiveTask { get; }
        TaskData? CurrentTask { get; }
        int CompletedTasksCount { get; }
        int GoalTaskIndex { get; }

        void CompleteCurrentTask();
        void Reset(List<TaskData> tasks);
    }
}