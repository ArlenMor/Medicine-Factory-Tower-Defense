using System.IO;
using System.Text;
using _Project.Code.Scripts.Data;
using _Project.Code.Scripts.Garden;
using UnityEngine;

namespace _Project.Code.Scripts.Stats
{
    public class GameplayLogger
    {
        private readonly StringBuilder _buf = new();
        private readonly int _sessionNumber;
        private float _levelStartTime;
        private int _currentLevel;
        private int _creditsEarned;
        private int _creditsSpent;
        private int _enemiesKilled;
        private int _playerClicks;
        private int _resColC, _resColP, _resColN;
        private int _resSpentC, _resSpentP, _resSpentN;

        public GameplayLogger()
        {
            _sessionNumber = NextSessionNumber();
        }

        private float RelTime => Time.time - _levelStartTime;
        private void Ln(string s = "") { _buf.AppendLine(s); }

        // ── START ────────────────────────────────────────────────────

        public void StartLevel(int levelIndex)
        {
            _currentLevel = levelIndex;
            _levelStartTime = Time.time;
            ResetCounters();
            _buf.Clear();

            Ln("==========================================");
            Ln($"  SESSION {_sessionNumber}  —  LEVEL {levelIndex}");
            Ln("==========================================");
            Ln();
            Ln("── TIMELINE ──");
        }

        private void ResetCounters()
        {
            _creditsEarned = 0; _creditsSpent = 0;
            _enemiesKilled = 0; _playerClicks = 0;
            _resColC = 0; _resColP = 0; _resColN = 0;
            _resSpentC = 0; _resSpentP = 0; _resSpentN = 0;
        }

        // ── WAVES ────────────────────────────────────────────────────

        public void LogWaveStart(int waveId, int scouts, int gnawers, int tanks)
        {
            Ln();
            Ln($"── WAVE {waveId} ──");
            Ln($"  [{RelTime:F2}s] WAVE {waveId} starts  —  S:{scouts}  G:{gnawers}  T:{tanks}");
        }

        public void LogWaveLastSpawn(int waveId)
        {
            Ln($"  Last enemy spawned at {RelTime:F2}s");
        }

        public void LogWaveClear(int waveId)
        {
            Ln($"  Wave {waveId} cleared at {RelTime:F2}s");
        }

        public void LogEnemyKilled(string enemyType)
        {
            _enemiesKilled++;
        }

        // ── PLANTING / HARVEST ───────────────────────────────────────

        public void LogPlant(PlantType type)
        {
            Ln($"  [{RelTime:F2}s] PLANT: {type}");
        }

        public void LogHarvest(string resourceType, int amount, bool doubled)
        {
            string d = doubled ? " (double!)" : "";
            switch (resourceType)
            {
                case "Crystal": _resColC += amount; break;
                case "Polymer": _resColP += amount; break;
                case "NanoGel": _resColN += amount; break;
            }
            Ln($"  [{RelTime:F2}s] HARVEST: +{amount} {resourceType}{d}");
        }

        // ── ORDERS ───────────────────────────────────────────────────

        public void LogResourceSpent(int crystal, int polymer, int nanogel)
        {
            _resSpentC += crystal; _resSpentP += polymer; _resSpentN += nanogel;
        }

        public void LogOrderStart(int orderId, int crystalCost, int polymerCost, int nanogelCost, float craftTime)
        {
            Ln();
            Ln($"  [{RelTime:F2}s] Order {orderId} starts  —  "
               + $"cost: C:{crystalCost}  P:{polymerCost}  N:{nanogelCost}  craft:{craftTime:F1}s");
        }

        public void LogOrderComplete(int orderId, int reward)
        {
            _creditsEarned += reward;
            Ln($"  [{RelTime:F2}s] Order {orderId} done  —  +{reward}cr");
        }

        // ── SPENDING ─────────────────────────────────────────────────

        public void LogPurchase(string item, int cost)
        {
            _creditsSpent += cost;
            int creds = GameData.Instance.Resources[ResourceType.Credit];
            Ln($"  [{RelTime:F2}s] BUY: {item}  —  -{cost}cr  (left: {creds}cr)");
        }

        // ── BRAIN ────────────────────────────────────────────────────

        public void LogBrainHit(string enemyType, int damage, float brainHP)
        {
            Ln($"  ⚡ [{RelTime:F2}s] {enemyType} hits brain for {damage}  (HP: {brainHP:F0})");
        }

        // ── CLICKS ───────────────────────────────────────────────────

        public void LogPlayerClick()
        {
            _playerClicks++;
        }

        // ── END ──────────────────────────────────────────────────────

        public void EndLevel(bool won)
        {
            Ln();
            Ln();
            Ln("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Ln("  SUMMARY");
            Ln("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Ln($"  Won:                          {(won ? "YES" : "NO")}");
            Ln($"  Total time:                   {RelTime:F1}s");
            Ln($"  Player clicks on enemies:     {_playerClicks}");
            Ln($"  Credits earned from orders:   {_creditsEarned}");
            Ln($"  Credits spent (def+upgrades): {_creditsSpent}");
            Ln($"  Enemies killed:               {_enemiesKilled}");
            Ln();
            Ln("  ── Resource balance ──");
            Ln("                    Harvested    Spent    Net");
            Ln($"  Crystal             {_resColC,3}       {_resSpentC,3}     {_resColC - _resSpentC,3}");
            Ln($"  Polymer             {_resColP,3}       {_resSpentP,3}     {_resColP - _resSpentP,3}");
            Ln($"  NanoGel             {_resColN,3}       {_resSpentN,3}     {_resColN - _resSpentN,3}");
            Ln();
            Ln("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            WriteToDisk();
        }

        // ── FILE IO ──────────────────────────────────────────────────

        private static int NextSessionNumber()
        {
            string dir = Path.Combine(Application.persistentDataPath, "GameLogs");
            try { Directory.CreateDirectory(dir); }
            catch { return 1; }

            int n = 1;
            while (Directory.Exists(Path.Combine(dir, $"Session{n}")))
                n++;
            return n;
        }

        private void WriteToDisk()
        {
            string baseDir = Path.Combine(Application.persistentDataPath, "GameLogs");
            try
            {
                string folder = Path.Combine(baseDir, $"Session{_sessionNumber}");
                Directory.CreateDirectory(folder);
                string path = Path.Combine(folder, $"Session{_sessionNumber}_Level{_currentLevel}.txt");
                File.WriteAllText(path, _buf.ToString());
                Debug.Log($"[GameplayLogger] saved: {path}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameplayLogger] failed: {e.Message}");
            }
        }
    }
}
