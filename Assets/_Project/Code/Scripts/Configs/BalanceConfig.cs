using System.Collections.Generic;
using _Project.Code.Scripts.Data;
using _Project.Code.Scripts.Data.TaskData;
using _Project.Code.Scripts.Garden;
using UnityEngine;

namespace _Project.Code.Scripts.Configs
{
    [System.Serializable]
    public struct LevelBalanceData
    {
        public int LevelIndex;
        public int StartCredits;
        public int TotalOrderReward;
        public int TotalAvailableCredits;
        public int OptimalUpgradeCost;
        public int CreditsForDefense;
        public float MinClearTimeSec;
        public float ClickDistractionSec;
        public int ReachingBrain;
        public float EstimatedClearTimeSec;
        public float TargetTimeSec;
        public bool TimeInTarget;
        public int TotalEnemies;
        public float TotalEnemyHP;
        public float EnemyDpsToBrain;
        public float BrainSurvivalSec;
        public int MaxTurretsAffordable;
        public int MaxBarricadesAffordable;
        public float DifficultyScore;
        public List<string> Warnings;
    }

    [CreateAssetMenu(fileName = "BalanceConfig", menuName = "GameConfig/BalanceConfig")]
    public class BalanceConfig : ScriptableObject
    {
        private const int GardenSlots = 6;
        private const float BrainHP = 200f;
        private const float ScoutHP = 18f;
        private const float GnawerHP = 42f;
        private const float TankHP = 105f;
        private const float ScoutCenterDPS = 1f / 0.7f;
        private const float GnawerCenterDPS = 2f / 0.9f;
        private const float TankCenterDPS = 5f / 1.15f;
        private const float AvgGrowMult = 1.2f;
        private const float AvgCraftMult = 1.35f;
        private const float DefaultGrowthC = 6f;
        private const float DefaultGrowthP = 8f;
        private const float DefaultGrowthN = 10f;
        private const int DefaultProductivity = 1;

        private const float TurretDPS = 8f / 0.75f;
        private const float TurretCoverPathUnits = 3.5f;
        private const float VisiblePathLength = 5f;
        private const float TotalPathLength = 7.5f;
        private const float ClickDamage = 7f;
        private const float ClickTimePenalty = 0.35f;
        private const float BarricadeHP = 80f;

        private static readonly int[] UpgradeCosts = { 10, 10, 17, 20, 20, 22 };

        public GameConfig GameConfig;
        public List<float> TargetPlayTimes = new() { 60, 60, 60, 90, 90, 90, 120, 120, 120, 120 };
        public List<float> FudgeFactors = new() { 2.5f, 2.0f, 1.8f, 1.4f, 1.2f, 1.3f, 1.3f, 1.25f, 1.2f, 1.15f };
        public AnimationCurve TargetDifficultyCurve = new(new Keyframe(1, 0f), new Keyframe(10, 1f));

        public List<LevelBalanceData> LevelMetrics = new();
        public List<string> GlobalWarnings = new();
        public float EstimatedTotalMinutes;
        public bool HasErrors;

        public LevelBalanceData GetLevel(int idx) =>
            LevelMetrics != null && idx >= 0 && idx < LevelMetrics.Count ? LevelMetrics[idx] : default;

        [ContextMenu("Recalculate")]
        public void Recalculate()
        {
            if (GameConfig == null)
            {
                Debug.LogError("BalanceConfig: GameConfig is not assigned!");
                return;
            }

            LevelMetrics = new List<LevelBalanceData>();
            GlobalWarnings = new List<string>();
            EstimatedTotalMinutes = 0;
            HasErrors = false;

            int levelCount = GameConfig.Levels?.Count ?? 0;
            for (int i = 1; i <= levelCount; i++)
            {
                var data = CalculateLevel(i);
                LevelMetrics.Add(data);
                EstimatedTotalMinutes += data.EstimatedClearTimeSec;
                if (data.Warnings != null && data.Warnings.Count > 0)
                {
                    GlobalWarnings.AddRange(data.Warnings);
                    HasErrors = true;
                }
            }

            EstimatedTotalMinutes /= 60f;
        }

        private LevelBalanceData CalculateLevel(int levelIndex)
        {
            var data = new LevelBalanceData
            {
                LevelIndex = levelIndex,
                Warnings = new List<string>(),
                TargetTimeSec = GetTargetTime(levelIndex)
            };

            var level = GameConfig.GetLevel(levelIndex);
            if (level == null) return data;

            data.StartCredits = level.StartCredits;

            var orders = BuildTaskList(levelIndex);
            data.TotalOrderReward = SumOrderRewards(orders);
            data.TotalAvailableCredits = data.StartCredits + data.TotalOrderReward;
            data.OptimalUpgradeCost = GetOptimalUpgradeCost(orders, data.TotalAvailableCredits);
            data.CreditsForDefense = Mathf.Max(0, data.TotalAvailableCredits - data.OptimalUpgradeCost);
            data.MaxTurretsAffordable = data.CreditsForDefense / 10;
            data.MaxBarricadesAffordable = data.CreditsForDefense / 5;

            data.MinClearTimeSec = SimulateMinTime(orders);

            // Defense simulation: how much enemy clicking distracts from farming
            var defense = ComputeDefenseImpact(level, data.MaxTurretsAffordable, data.MinClearTimeSec);
            data.ClickDistractionSec = defense.clickTime;
            data.ReachingBrain = defense.reachingBrain;
            CalculateWaveMetrics(level, ref data);

            // Estimated time = (farming time + click distraction) × fudge factor
            data.EstimatedClearTimeSec = (data.MinClearTimeSec + data.ClickDistractionSec) * GetFudgeFactor(levelIndex);
            float diff = Mathf.Abs(data.EstimatedClearTimeSec - data.TargetTimeSec);
            data.TimeInTarget = data.TargetTimeSec > 0 && diff <= data.TargetTimeSec * 0.3f;

            data.DifficultyScore = CalculateDifficultyScore(ref data);
            GenerateWarnings(ref data, level, orders);

            return data;
        }

        private float GetTargetTime(int levelIndex)
        {
            int idx = levelIndex - 1;
            return TargetPlayTimes != null && idx < TargetPlayTimes.Count ? TargetPlayTimes[idx] : 60f;
        }

        private float GetFudgeFactor(int levelIndex)
        {
            int idx = levelIndex - 1;
            return FudgeFactors != null && idx < FudgeFactors.Count ? FudgeFactors[idx] : 1.5f;
        }

        // ─── ORDER HELPERS ───────────────────────────────────────────────

        private List<TaskData> BuildTaskList(int levelIndex)
        {
            if (GameConfig.LevelOrdersConfig?.Entries == null || GameConfig.TaskConfig?.Tasks == null)
                return new List<TaskData>();

            var levelEntries = new List<LevelOrderEntry>();
            foreach (var entry in GameConfig.LevelOrdersConfig.Entries)
            {
                if (entry.LevelId == levelIndex)
                    levelEntries.Add(entry);
            }
            levelEntries.Sort((a, b) => a.OrderIndex.CompareTo(b.OrderIndex));

            var result = new List<TaskData>(levelEntries.Count);
            foreach (var entry in levelEntries)
            {
                var allTasks = GameConfig.TaskConfig.Tasks;
                for (int i = 0; i < allTasks.Count; i++)
                {
                    if ((int)allTasks[i].ResultType == entry.OrderId)
                    {
                        result.Add(allTasks[i]);
                        break;
                    }
                }
            }
            return result;
        }

        private int SumOrderRewards(List<TaskData> orders)
        {
            int sum = 0;
            foreach (var o in orders) sum += o.CreditReward;
            return sum;
        }

        private int GetOptimalUpgradeCost(List<TaskData> orders, int availableCredits)
        {
            int count = orders.Count;
            if (count <= 3) return 0;

            float budget = availableCredits * 0.5f;
            int maxUpgrades;
            if (count <= 4) maxUpgrades = 0;   // short level → all on defense
            else if (count == 5) maxUpgrades = 1; // GrowSpeed L1 only
            else if (count == 6) maxUpgrades = 2; // GrowSpeed + CraftSpeed
            else maxUpgrades = 3;                  // + Produce L1 if budget allows

            maxUpgrades = Mathf.Min(maxUpgrades, UpgradeCosts.Length);
            int cost = 0;
            for (int i = 0; i < maxUpgrades; i++)
            {
                if (cost + UpgradeCosts[i] <= budget)
                    cost += UpgradeCosts[i];
                else
                    break;
            }
            return cost;
        }

        // ─── ECONOMIC SIMULATION ─────────────────────────────────────────

        private float SimulateMinTime(List<TaskData> orders)
        {
            if (orders.Count == 0) return 30f;

            float growthC = DefaultGrowthC, growthP = DefaultGrowthP, growthN = DefaultGrowthN;
            var garden = GameConfig?.GardenConfig;
            if (garden?.GrowableResources != null)
            {
                foreach (var r in garden.GrowableResources)
                {
                    if (r.ResourceType == ResourceType.Crystal && r.GrowthTime > 0) growthC = r.GrowthTime;
                    else if (r.ResourceType == ResourceType.Polymer && r.GrowthTime > 0) growthP = r.GrowthTime;
                    else if (r.ResourceType == ResourceType.NanoGel && r.GrowthTime > 0) growthN = r.GrowthTime;
                }
            }

            const int slots = 6;
            float t = 0f;
            int[] res = { 0, 0, 0 };

            PlantType[] plants = { PlantType.Crystal, PlantType.Crystal, PlantType.Crystal, PlantType.Polymer, PlantType.Polymer, PlantType.NanoGel };
            float[] harvestTimes = new float[slots];
            for (int i = 0; i < slots; i++)
            {
                float gt = plants[i] == PlantType.Crystal ? growthC : plants[i] == PlantType.Polymer ? growthP : growthN;
                harvestTimes[i] = gt;
            }

            int orderIdx = 0;
            bool crafting = false;
            float craftEnd = -1f;

            while (orderIdx < orders.Count)
            {
                float nextH = float.MaxValue;
                for (int i = 0; i < slots; i++)
                    if (harvestTimes[i] < nextH) nextH = harvestTimes[i];

                float nextC = crafting ? craftEnd : float.MaxValue;
                float evt = Mathf.Min(nextH, nextC);
                if (evt >= float.MaxValue) break;
                t = evt;

                for (int i = 0; i < slots; i++)
                {
                    if (harvestTimes[i] <= t + 0.001f)
                    {
                        int ri = (int)plants[i] - 1;
                        if (ri >= 0 && ri < 3) res[ri] += DefaultProductivity;

                        if (orderIdx < orders.Count)
                        {
                            int needC = 0, needP = 0, needN = 0;
                            for (int o = orderIdx; o < orders.Count; o++)
                            {
                                needC += Mathf.Max(0, orders[o].CostInfo.CrystalCost - res[0]);
                                needP += Mathf.Max(0, orders[o].CostInfo.PolymerCost - res[1]);
                                needN += Mathf.Max(0, orders[o].CostInfo.NanoGelCost - res[2]);
                            }
                            float gt;
                            if (needN >= needP && needN >= needC) { plants[i] = PlantType.NanoGel; gt = growthN / AvgGrowMult; }
                            else if (needP >= needC) { plants[i] = PlantType.Polymer; gt = growthP / AvgGrowMult; }
                            else { plants[i] = PlantType.Crystal; gt = growthC / AvgGrowMult; }
                            harvestTimes[i] = t + Mathf.Max(gt, 0.1f);
                        }
                        else
                        {
                            harvestTimes[i] = float.MaxValue;
                        }
                    }
                }

                if (crafting && craftEnd <= t + 0.001f)
                {
                    crafting = false;
                    orderIdx++;
                }

                if (!crafting && orderIdx < orders.Count)
                {
                    var order = orders[orderIdx];
                    if (res[0] >= order.CostInfo.CrystalCost && res[1] >= order.CostInfo.PolymerCost && res[2] >= order.CostInfo.NanoGelCost)
                    {
                        res[0] -= order.CostInfo.CrystalCost;
                        res[1] -= order.CostInfo.PolymerCost;
                        res[2] -= order.CostInfo.NanoGelCost;
                        crafting = true;
                        craftEnd = t + order.ProduceTime / AvgCraftMult;
                    }
                }
            }

            return Mathf.Max(t, 5f);
        }

        // ─── DEFENSE MODEL (time-aware, wave StartTime, turret throughput) ──

        private (float clickTime, int reachingBrain, float brainSurvival)
            ComputeDefenseImpact(LevelConfig level, int numTurrets, float T_econ)
        {
            if (level.WaveConfig?.Waves == null || level.WaveConfig.Waves.Count == 0)
                return (0f, 0, 999f);

            float coverage = Mathf.Min(1f, numTurrets * TurretCoverPathUnits / VisiblePathLength);
            float totalClickTime = 0f;
            int totalReachingBrain = 0;
            float brainDamageRate = 0f;

            int waveCount = level.WaveConfig.Waves.Count;
            bool hasLoop = level.WaveConfig.LoopLastWave;
            float loopDelay = level.WaveConfig.LoopDelay;

            for (int w = 0; w < waveCount; w++)
            {
                var wave = level.WaveConfig.Waves[w];
                bool isLastLoop = hasLoop && (w == waveCount - 1);

                // Step 1: filter by StartTime – wave never starts
                if (wave.StartTime >= T_econ) break;

                // Step 2: how much of this wave fits before T_econ
                float lastSpawnOffset = (Mathf.Max(wave.ScoutCount, wave.GnawerCount, wave.TankCount) - 1)
                                        * wave.IntraSpawnInterval;
                float waveEnd = wave.StartTime + lastSpawnOffset;
                float waveAvail = Mathf.Max(0f, T_econ - wave.StartTime);

                // Fraction of enemies that actually spawn before T_econ
                float fraction = waveAvail > 0f
                    ? Mathf.Min(1f, waveAvail / (lastSpawnOffset + 1f))
                    : 0f;

                int scouts = Mathf.RoundToInt(wave.ScoutCount * fraction);
                int gnawers = Mathf.RoundToInt(wave.GnawerCount * fraction);
                int tanks = Mathf.RoundToInt(wave.TankCount * fraction);

                // Step 3: loop cycles for the last wave
                if (isLastLoop && waveEnd < T_econ)
                {
                    float remaining = T_econ - waveEnd;
                    // Estimate how long it takes to kill the full wave (turret throughput)
                    int totalInWave = wave.ScoutCount + wave.GnawerCount + wave.TankCount;
                    float avgTTK = (wave.ScoutCount * 18f + wave.GnawerCount * 42f + wave.TankCount * 105f)
                                   / (totalInWave * TurretDPS * Mathf.Max(1, numTurrets));
                    float killTime = totalInWave * avgTTK / Mathf.Max(1, numTurrets);
                    float loopCycle = Mathf.Max(1f, killTime + loopDelay);
                    int extraLoops = Mathf.FloorToInt(remaining / loopCycle);
                    if (extraLoops > 0)
                    {
                        scouts += wave.ScoutCount * extraLoops;
                        gnawers += wave.GnawerCount * extraLoops;
                        tanks += wave.TankCount * extraLoops;
                    }
                }

                // Step 4: per-type processing with time window
                totalClickTime += ProcessEnemyType(numTurrets, scouts, 18f, 1.9f,
                    coverage, 0.7f, waveAvail,
                    ref totalReachingBrain, ref brainDamageRate);
                totalClickTime += ProcessEnemyType(numTurrets, gnawers, 42f, 1.25f,
                    coverage, 0.9f, waveAvail,
                    ref totalReachingBrain, ref brainDamageRate);
                totalClickTime += ProcessEnemyType(numTurrets, tanks, 105f, 0.78f,
                    coverage, 1.15f, waveAvail,
                    ref totalReachingBrain, ref brainDamageRate);
            }

            float brainSurvival = brainDamageRate > 0f ? BrainHP / brainDamageRate : 999f;
            return (totalClickTime, totalReachingBrain, brainSurvival);
        }

        private float ProcessEnemyType(int numTurrets, int count, float hp, float speed,
            float coverage, float attackInterval, float timeWindow,
            ref int reachingBrain, ref float brainDamageRate)
        {
            if (count <= 0) return 0f;

            float timeInRange = TurretCoverPathUnits / speed;
            float dmgPass = TurretDPS * timeInRange * coverage;
            int turretsOnPath = Mathf.Min(numTurrets, 3);
            float travelTime = TotalPathLength / speed;

            if (dmgPass >= hp)
            {
                // Turrets CAN kill this type – throughput limited.
                float ttk = hp / TurretDPS;
                float effectiveTime = Mathf.Min(ttk, timeInRange);
                float killRate = turretsOnPath / effectiveTime;
                int killed = Mathf.RoundToInt(killRate * timeWindow);
                int remaining = Mathf.Max(0, count - killed);
                if (remaining <= 0) return 0f;

                // Remaining: no turret damage (turrets were busy)
                int clicksPer = Mathf.CeilToInt(hp / ClickDamage);
                return EnemyClickModel(remaining, clicksPer, travelTime,
                    ref reachingBrain, ref brainDamageRate, attackInterval);
            }
            else
            {
                // Turrets cannot kill – all survive with remaining HP.
                float remainingHP = hp - dmgPass;
                int clicksPer = Mathf.Max(1, Mathf.CeilToInt(remainingHP / ClickDamage));
                return EnemyClickModel(count, clicksPer, travelTime,
                    ref reachingBrain, ref brainDamageRate, attackInterval);
            }
        }

        private float EnemyClickModel(int count, int clicksPerEnemy, float travelTime,
            ref int reachingBrain, ref float brainDamageRate, float attackInterval)
        {
            const float maxClickRate = 3f;
            int totalClicks = clicksPerEnemy * count;
            float clickWindow = travelTime * 0.5f;
            int clickCapacity = Mathf.RoundToInt(maxClickRate * clickWindow);
            int clicksShort = Mathf.Max(0, totalClicks - clickCapacity);

            if (clicksShort > 0)
            {
                int toBrain = clicksShort / clicksPerEnemy;
                if (toBrain > 0)
                {
                    reachingBrain += toBrain;
                    brainDamageRate += toBrain / attackInterval;
                }
            }

            int clicksDone = Mathf.Min(totalClicks, clickCapacity);
            return clicksDone * ClickTimePenalty * 0.5f;
        }

        // ─── WAVE METRICS ────────────────────────────────────────────────

        private void CalculateWaveMetrics(LevelConfig level, ref LevelBalanceData data)
        {
            if (level.WaveConfig?.Waves == null || level.WaveConfig.Waves.Count == 0) return;

            int totalEnemies = 0;
            float totalHP = 0f;
            float totalDps = 0f;

            foreach (var wave in level.WaveConfig.Waves)
            {
                totalEnemies += wave.ScoutCount + wave.GnawerCount + wave.TankCount;
                totalHP += wave.ScoutCount * ScoutHP + wave.GnawerCount * GnawerHP + wave.TankCount * TankHP;
                totalDps += wave.ScoutCount * ScoutCenterDPS + wave.GnawerCount * GnawerCenterDPS + wave.TankCount * TankCenterDPS;
            }

            data.TotalEnemies = totalEnemies;
            data.TotalEnemyHP = totalHP;
            data.EnemyDpsToBrain = totalDps;
            data.BrainSurvivalSec = totalDps > 0f ? BrainHP / totalDps : 999f;
        }

        // ─── DIFFICULTY SCORE ────────────────────────────────────────────

        private float CalculateDifficultyScore(ref LevelBalanceData data)
        {
            float timeRatio = data.EstimatedClearTimeSec > 0 ? data.TargetTimeSec / data.EstimatedClearTimeSec : 0f;
            float timeScore = Mathf.Clamp01(timeRatio);
            float survivalScore = 1f - Mathf.Clamp01(data.BrainSurvivalSec / 60f);
            float enemyScore = Mathf.Clamp01(data.TotalEnemies / 60f);
            float distractionScore = Mathf.Clamp01(data.ClickDistractionSec / data.EstimatedClearTimeSec);

            return 0.15f * timeScore + 0.35f * survivalScore + 0.35f * enemyScore + 0.15f * distractionScore;
        }

        // ─── WARNINGS ────────────────────────────────────────────────────

        private void GenerateWarnings(ref LevelBalanceData data, LevelConfig level, List<TaskData> orders)
        {
            if (data.EstimatedClearTimeSec > data.TargetTimeSec * 1.5f && data.TargetTimeSec > 0)
                data.Warnings.Add($"Est.time {data.EstimatedClearTimeSec:F0}s >50% above target {data.TargetTimeSec:F0}s");

            if (data.BrainSurvivalSec < 10f)
                data.Warnings.Add($"Brain survives {data.BrainSurvivalSec:F1}s — {data.TotalEnemies} enemies, {data.EnemyDpsToBrain:F1} DPS");

            if (data.ClickDistractionSec > data.MinClearTimeSec * 0.5f)
                data.Warnings.Add($"Click distraction {data.ClickDistractionSec:F0}s — player spends >50% extra time on defense");

            if (level.WaveConfig != null && level.WaveConfig.Waves != null && level.WaveConfig.Waves.Count > 0)
            {
                float firstWave = level.WaveConfig.Waves[0].StartTime;
                if (firstWave < 6f)
                    data.Warnings.Add($"First wave at {firstWave:F0}s — before first harvest (6s)");
            }

            if (data.CreditsForDefense < 5 && data.TotalEnemies > 0)
                data.Warnings.Add($"Only {data.CreditsForDefense}cr left for defense against {data.TotalEnemies} enemies");

            if (data.StartCredits < 10 && data.TotalEnemies > 10)
                data.Warnings.Add($"Low start ({data.StartCredits}cr) with {data.TotalEnemies} enemies");
        }
    }
}
