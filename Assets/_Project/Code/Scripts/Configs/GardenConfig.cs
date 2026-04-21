using _Project.Code.Scripts.Data;
using UnityEngine;

namespace _Project.Code.Scripts.Configs
{
    [CreateAssetMenu(fileName = "GardenConfig", menuName = "GameConfig/GardenConfig")]
    public class GardenConfig: ScriptableObject
    {
        public int CreditStartAmount;
        public GrowableResourceData[] GrowableResources;
    }

    public static class GardenConfigExtension
    {
        public static GrowableResourceData GetGrowableResourceData(this GardenConfig gardenConfig, ResourceType resourceType)
        {
            foreach (var resource in gardenConfig.GrowableResources)
            {
                if (resource.ResourceType == resourceType)
                {
                    return resource;
                }
            }

            return default;
        }
    }
}