using _Project.Code.Scripts.Data;

namespace _Project.Code.Scripts.Garden
{
    public enum PlantType
    {
        NuN = 0,
        Crystal = 1,
        NanoGel = 2,
        Polymer = 3,
    }
    
    public static class PlantTypeExtension
    {
        public static ResourceType GetResourceType(this PlantType plantType)
        {
            return plantType switch
            {
                PlantType.Crystal => ResourceType.Crystal,
                PlantType.NanoGel => ResourceType.NanoGel,
                PlantType.Polymer => ResourceType.Polymer,
                PlantType.NuN => throw new System.NotImplementedException(),
                _ => throw new System.NotImplementedException(),
            };
        }
    }
}