namespace _Project.Code.Scripts.Data.TaskData
{
    public static class TaskResultTypeExtensions
    {
        public static string ToDisplayString(this MedicationsType type) => type switch
        {
            MedicationsType.GelPotion => "Gel Potion",
            MedicationsType.GelСapsules => "Gel Capsules",
            MedicationsType.GelSyringe => "Gel Syringe",
            MedicationsType.CryustalGelSyringe => "Crystal Gel Syringe",
            MedicationsType.CrystalSyringe => "Crystal Syringe",
            MedicationsType.CrystalСapsulesBox => "Crystal Capsules Box",
            MedicationsType.CrystalСapsulesRaw => "Crystal Capsules Raw",
            MedicationsType.CrystalPlastine => "Crystal Plastine",
            MedicationsType.PolymerСapsules => "Polymer Capsules",
            MedicationsType.PolymerBandage => "Polymer Bandage",
            MedicationsType.PolymerСapsulesBox => "Polymer Capsules Box",
            MedicationsType.FirstAidKit => "First Aid Kit",
            _                    => "—"
        };
    }
}
