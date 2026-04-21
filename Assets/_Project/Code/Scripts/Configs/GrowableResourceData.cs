using System;
using _Project.Code.Scripts.Data;
using UnityEngine.Serialization;

namespace _Project.Code.Scripts.Configs
{
    [Serializable]
    public struct GrowableResourceData
    {
        public ResourceType ResourceType;
        public int StartAmount;
        public int GrowthTime;
        [FormerlySerializedAs("DefaultProductionProductivity")] public int DefaultProductivity;
    }
}