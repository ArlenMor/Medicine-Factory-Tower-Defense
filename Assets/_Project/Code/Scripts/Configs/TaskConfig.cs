using System.Collections.Generic;
using _Project.Code.Scripts.Data.TaskData;
using UnityEngine;

namespace _Project.Code.Scripts.Configs
{   
    [CreateAssetMenu(fileName = "TaskConfig", menuName = "TaskConfig")]
    public class TaskConfig : ScriptableObject
    {
        public List<TaskData> Tasks;
    }
}