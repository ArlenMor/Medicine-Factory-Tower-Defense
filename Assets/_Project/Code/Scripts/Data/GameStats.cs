namespace _Project.Code.Scripts.Data
{
    public class GameStats
    {
        public int EnemiesKilled;
        public int ResourcesCollected;
        public int PlantsPlanted;
        public int CreditsEarned;
        public float TimePlayed;
        public int UpgradesPurchased;
        public int TurretsBuilt;
        public int BarricadesBuilt;

        public void Reset()
        {
            EnemiesKilled = 0;
            ResourcesCollected = 0;
            PlantsPlanted = 0;
            CreditsEarned = 0;
            TimePlayed = 0f;
            UpgradesPurchased = 0;
            TurretsBuilt = 0;
            BarricadesBuilt = 0;
        }
    }
}
