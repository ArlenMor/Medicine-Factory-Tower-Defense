using _Project.Code.Scripts.Data;

namespace _Project.Code.Scripts.Garden
{
    public enum PlantType
    {
        NuN = 0,
        Crystal = 1,
        Polymer = 2,
        NanoGel = 3,
    }
    
    public static class PlantTypeExtension
    {
        public static ResourceType GetResourceType(this PlantType plantType)
        {
            return plantType switch
            {
                PlantType.Crystal => ResourceType.Crystal,
                PlantType.Polymer => ResourceType.Polymer,
                PlantType.NanoGel => ResourceType.NanoGel,
                PlantType.NuN => throw new System.NotImplementedException(),
                _ => throw new System.NotImplementedException(),
            };
        }
    }
}