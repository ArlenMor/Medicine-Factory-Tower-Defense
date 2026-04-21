using System;

namespace _Project.Code.Scripts.Data.TaskData
{
    [Serializable]
    public struct TaskData
    {
        public TaskResultType ResultType;
        public int CreditReward;
        public float ProduceTime;
        public ProductionCost CostInfo;
    }
}