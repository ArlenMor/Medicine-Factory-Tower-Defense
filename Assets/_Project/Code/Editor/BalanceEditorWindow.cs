using System.Collections.Generic;
using _Project.Code.Scripts.Configs;
using _Project.Code.Scripts.Data;
using _Project.Code.Scripts.Data.TaskData;
using UnityEditor;
using UnityEngine;

namespace _Project.Code.Editor
{
    public class BalanceEditorWindow : EditorWindow
    {
        private const string PrefsKey = "BalanceEditor_ConfigGuid";

        private BalanceConfig _config;
        private int _selectedTab;
        private int _selectedLevel = 1;
        private Vector2 _scrollPos;
        private GUIStyle _barStyle;
        private GUIStyle _greenLabel;
        private GUIStyle _yellowLabel;
        private GUIStyle _redLabel;

        private readonly string[] _tabNames = { "Dashboard", "Per-Level", "Economy", "Enemies", "Warnings" };
        private static readonly Color GreenOk = new(0.3f, 0.8f, 0.3f);
        private static readonly Color YellowWarn = new(0.9f, 0.8f, 0.1f);
        private static readonly Color RedBad = new(0.9f, 0.2f, 0.2f);
        private static readonly Color LevelColors = new(0.6f, 0.8f, 1f, 0.6f);

        [MenuItem("Tools/Balance Editor")]
        public static void ShowWindow()
        {
            var w = GetWindow<BalanceEditorWindow>("Balance Editor");
            w.minSize = new Vector2(700, 500);
            w.Show();
        }

        private void OnEnable()
        {
            string guid = EditorPrefs.GetString(PrefsKey, "");
            if (!string.IsNullOrEmpty(guid))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                _config = AssetDatabase.LoadAssetAtPath<BalanceConfig>(path);
            }
            EnsureStyles();
        }

        private void OnDisable()
        {
            if (_config != null)
            {
                string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_config));
                EditorPrefs.SetString(PrefsKey, guid);
            }
        }

        private void EnsureStyles()
        {
            if (_barStyle != null) return;
            _barStyle = new GUIStyle { normal = { background = Texture2D.whiteTexture } };
            _greenLabel = new GUIStyle(EditorStyles.label) { normal = { textColor = GreenOk } };
            _yellowLabel = new GUIStyle(EditorStyles.label) { normal = { textColor = YellowWarn } };
            _redLabel = new GUIStyle(EditorStyles.label) { normal = { textColor = RedBad } };
        }

        private void OnGUI()
        {
            EnsureStyles();
            DrawConfigField();

            if (_config == null)
            {
                EditorGUILayout.HelpBox("Assign a BalanceConfig asset to begin.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space(5);
            DrawToolbar();
            EditorGUILayout.Space(5);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            switch (_selectedTab)
            {
                case 0: DrawDashboard(); break;
                case 1: DrawPerLevel(); break;
                case 2: DrawEconomy(); break;
                case 3: DrawEnemies(); break;
                case 4: DrawWarnings(); break;
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawConfigField()
        {
            EditorGUILayout.BeginHorizontal();
            _config = (BalanceConfig)EditorGUILayout.ObjectField("Config", _config, typeof(BalanceConfig), false);
            if (GUILayout.Button("Recalculate", GUILayout.Width(120), GUILayout.Height(20)))
            {
                _config.Recalculate();
                if (_config.HasErrors)
                    Debug.LogWarning("Balance: warnings found. Check Warnings tab.");
                else
                    Debug.Log("Balance: OK");
            }
            if (GUILayout.Button("Open Config", GUILayout.Width(100), GUILayout.Height(20)))
            {
                Selection.activeObject = _config;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            for (int i = 0; i < _tabNames.Length; i++)
            {
                bool selected = _selectedTab == i;
                if (GUILayout.Toggle(selected, _tabNames[i], EditorStyles.toolbarButton))
                    _selectedTab = i;
            }
            GUILayout.EndHorizontal();
        }

        // ─── TAB 0: Dashboard ────────────────────────────────────────────

        private void DrawDashboard()
        {
            float totalTarget = 0;
            foreach (var t in _config.TargetPlayTimes) totalTarget += t;
            totalTarget /= 60f;

            EditorGUILayout.LabelField("Game Balance Overview", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);

            EditorGUILayout.LabelField($"Total Time: {_config.EstimatedTotalMinutes:F1}m / {totalTarget:F1}m target",
                _config.EstimatedTotalMinutes <= totalTarget * 1.15f ? _greenLabel :
                _config.EstimatedTotalMinutes <= totalTarget * 1.3f ? _yellowLabel : _redLabel);

            EditorGUILayout.Space(10);

            // Difficulty curve
            EditorGUILayout.LabelField("Difficulty Curve", EditorStyles.boldLabel);
            var curveRect = EditorGUILayout.GetControlRect(false, 150);
            DrawCurveWithPoints(curveRect);

            EditorGUILayout.Space(10);

            // Summary table
            EditorGUILayout.LabelField("Per-Level Summary", EditorStyles.boldLabel);
            DrawSummaryTable();
        }

        private void DrawCurveWithPoints(Rect rect)
        {
            if (_config.LevelMetrics == null || _config.LevelMetrics.Count == 0) return;

            // Background
            EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f, 0.9f));

            float pad = 4f;
            Rect inner = new(rect.x + pad, rect.y + pad, rect.width - pad * 2, rect.height - pad * 2);

            // Grid lines
            Handles.color = new Color(1f, 1f, 1f, 0.1f);
            for (int i = 0; i <= 4; i++)
            {
                float y = inner.y + inner.height * (1f - i / 4f);
                Handles.DrawLine(new Vector3(inner.x, y), new Vector3(inner.x + inner.width, y));
            }

            // Target curve
            Handles.color = new Color(0.3f, 0.6f, 1f, 0.6f);
            int points = 50;
            Vector3 prev = Vector3.zero;
            bool first = true;
            for (int i = 0; i <= points; i++)
            {
                float level = 1f + 9f * i / points;
                float val = _config.TargetDifficultyCurve.Evaluate(level);
                float x = inner.x + (level - 1f) / 9f * inner.width;
                float y = inner.y + inner.height * (1f - val);
                if (!first)
                    Handles.DrawLine(prev, new Vector3(x, y, 0));
                else
                    first = false;
                prev = new Vector3(x, y, 0);
            }

            // Level points
            foreach (var m in _config.LevelMetrics)
            {
                float x = inner.x + (m.LevelIndex - 1f) / 9f * inner.width;
                float y = inner.y + inner.height * (1f - Mathf.Clamp01(m.DifficultyScore));
                Handles.color = m.Warnings != null && m.Warnings.Count > 0 ? RedBad : LevelColors;
                Handles.DrawSolidDisc(new Vector3(x, y, 0), Vector3.forward, 4f);

                // Label
                GUI.Label(new Rect(x - 8, y - 18, 20, 16), m.LevelIndex.ToString(), EditorStyles.miniLabel);
            }

            // Axis labels
            GUI.Label(new Rect(inner.x, inner.y + inner.height - 2, 20, 16), "0", EditorStyles.miniLabel);
            GUI.Label(new Rect(inner.x + inner.width - 18, inner.y + inner.height - 2, 20, 16), "10", EditorStyles.miniLabel);
            GUI.Label(new Rect(inner.x, inner.y - 2, 20, 16), "1", EditorStyles.miniLabel);
        }

        private void DrawSummaryTable()
        {
            float colW = 60;
            float[] colWidths = { 40, colW, colW, colW, colW, colW, colW + 20 };
            string[] headers = { "Lvl", "Est.Time", "Target", "Orders°", "Credits°", "Enemies", "Score" };

            // Header
            GUILayout.BeginHorizontal();
            for (int i = 0; i < headers.Length; i++)
            {
                EditorGUILayout.LabelField(headers[i], EditorStyles.boldLabel, GUILayout.Width(colWidths[i]));
            }
            GUILayout.EndHorizontal();

            if (_config.LevelMetrics == null) return;

            foreach (var m in _config.LevelMetrics)
            {
                GUILayout.BeginHorizontal();

                // Level indicator with color
                bool warn = m.Warnings != null && m.Warnings.Count > 0;
                string lvlLabel = warn ? $"⚑ {m.LevelIndex}" : $"  {m.LevelIndex}";
                EditorGUILayout.LabelField(lvlLabel, warn ? _yellowLabel : EditorStyles.label, GUILayout.Width(colWidths[0]));

                Color textColor = m.TimeInTarget ? GreenOk : YellowWarn;
                var style = new GUIStyle(EditorStyles.label) { normal = { textColor = textColor } };
                EditorGUILayout.LabelField($"{m.EstimatedClearTimeSec / 60f:F1}m", style, GUILayout.Width(colWidths[1]));
                EditorGUILayout.LabelField($"{m.TargetTimeSec / 60f:F1}m", GUILayout.Width(colWidths[2]));
                EditorGUILayout.LabelField($"{m.TotalOrderReward}", GUILayout.Width(colWidths[3]));
                EditorGUILayout.LabelField($"{m.TotalAvailableCredits}", GUILayout.Width(colWidths[4]));
                EditorGUILayout.LabelField($"{m.TotalEnemies}", GUILayout.Width(colWidths[5]));

                // Difficulty bar
                float diff = Mathf.Clamp01(m.DifficultyScore);
                var barRect = EditorGUILayout.GetControlRect(false, 16, GUILayout.Width(colWidths[6]));
                EditorGUI.DrawRect(barRect, new Color(0.2f, 0.2f, 0.2f));
                EditorGUI.DrawRect(new Rect(barRect.x, barRect.y, barRect.width * diff, barRect.height),
                    Color.Lerp(GreenOk, RedBad, diff));

                GUILayout.EndHorizontal();
            }
        }

        // ─── TAB 1: Per-Level ────────────────────────────────────────────

        private void DrawPerLevel()
        {
            var metrics = _config.LevelMetrics;
            if (metrics == null || metrics.Count == 0)
            {
                EditorGUILayout.HelpBox("No level data. Hit Recalculate.", MessageType.Info);
                return;
            }

            _selectedLevel = Mathf.Clamp(_selectedLevel, 1, metrics.Count);
            int idx = _selectedLevel - 1;
            var m = metrics[idx];

            // Level selector
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Level");
            _selectedLevel = EditorGUILayout.IntSlider(_selectedLevel, 1, metrics.Count);
            EditorGUILayout.LabelField(GetLevelLabel(_selectedLevel), GUILayout.Width(200));
            if (GUILayout.Button("Open LevelConfig", GUILayout.Width(130)))
            {
                var level = _config.GameConfig.GetLevel(_selectedLevel);
                if (level != null) Selection.activeObject = level;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            DrawMetricSection("Economy", () =>
            {
                DrawMetricRow("Start Credits", m.StartCredits.ToString());
                DrawMetricRow("Order Rewards", $"+{m.TotalOrderReward}", $"total: {m.TotalAvailableCredits}");
                DrawMetricRow("Optimal Upgrade Cost", $"-{m.OptimalUpgradeCost}");
                DrawMetricRow("For Defense", m.CreditsForDefense.ToString(),
                    m.CreditsForDefense > 0 ? $"{m.CreditsForDefense / 10} turrets or {m.CreditsForDefense / 5} barricades" : "none");
                DrawMetricRow("Min Clear Time", $"{m.MinClearTimeSec:F1}s");
                DrawMetricRow("Click Distraction", $"+{m.ClickDistractionSec:F1}s",
                    m.ClickDistractionSec > m.MinClearTimeSec * 0.5f ? "heavy" : "light",
                    m.ClickDistractionSec > m.MinClearTimeSec * 0.5f ? YellowWarn : GreenOk);
                DrawMetricRow("Estimated Time", $"{m.EstimatedClearTimeSec:F1}s",
                    m.TimeInTarget ? "✓ in target" : $"target {m.TargetTimeSec:F1}s",
                    m.TimeInTarget ? GreenOk : YellowWarn);
            });

            EditorGUILayout.Space(8);
            DrawMetricSection("Defense", () =>
            {
                DrawMetricRow("Total Enemies", m.TotalEnemies.ToString());
                DrawMetricRow("Total Enemy HP", $"{m.TotalEnemyHP:F0}");
                DrawMetricRow("Enemy DPS to Brain", $"{m.EnemyDpsToBrain:F1}");
                DrawMetricRow("Brain Survival (no def)", $"{m.BrainSurvivalSec:F1}s",
                    m.BrainSurvivalSec < 10 ? "critical" : m.BrainSurvivalSec < 20 ? "tight" : "ok",
                    m.BrainSurvivalSec < 10 ? RedBad : m.BrainSurvivalSec < 20 ? YellowWarn : GreenOk);
                DrawMetricRow("Affordable Turrets", m.MaxTurretsAffordable > 0 ? $"up to {m.MaxTurretsAffordable}" : "none");
                DrawMetricRow("Affordable Barricades", m.MaxBarricadesAffordable > 0 ? $"up to {m.MaxBarricadesAffordable}" : "none");
            });

            // Warnings for this level
            if (m.Warnings != null && m.Warnings.Count > 0)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField("Warnings", EditorStyles.boldLabel);
                foreach (var w in m.Warnings)
                    EditorGUILayout.HelpBox(w, MessageType.Warning);
            }
        }

        private string GetLevelLabel(int levelIndex)
        {
            if (levelIndex <= 3) return "— Tutorial";
            return levelIndex <= 7 ? "— Mid game" : "— End game";
        }

        // ─── TAB 2: Economy ──────────────────────────────────────────────

        private void DrawEconomy()
        {
            var metrics = _config.LevelMetrics;
            if (metrics == null || metrics.Count == 0) return;

            EditorGUILayout.LabelField("Economy Analysis per Level", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // Summary table
            string[] ecoHeaders = { "Lvl", "StartCr", "Orders", "Reward", "UpgrCost", "DefBudget", "GrowR", "CraftR" };
            float[] ecoW = { 35, 55, 55, 55, 65, 65, 55, 55 };

            GUILayout.BeginHorizontal();
            for (int i = 0; i < ecoHeaders.Length; i++)
                EditorGUILayout.LabelField(ecoHeaders[i], EditorStyles.boldLabel, GUILayout.Width(ecoW[i]));
            GUILayout.EndHorizontal();

            foreach (var m in metrics)
            {
                var level = _config.GameConfig.GetLevel(m.LevelIndex);
                var orders = GetOrdersForLevel(m.LevelIndex);
                float avgGrowRate = 0f, avgCraftRate = 0f;

                float totalCraft = 0f;
                int needC = 0, needP = 0, needN = 0;
                foreach (var o in orders)
                {
                    needC += o.CostInfo.CrystalCost;
                    needP += o.CostInfo.PolymerCost;
                    needN += o.CostInfo.NanoGelCost;
                    totalCraft += o.ProduceTime;
                }
                float totalNeed = needC + needP + needN;
                if (totalNeed > 0)
                {
                    float slotsC = 6f * needC / totalNeed;
                    float slotsP = 6f * needP / totalNeed;
                    float slotsN = 6f - slotsC - slotsP;
                    if (slotsN < 0) { slotsP += slotsN; slotsN = 0; }

                    float rateC = slotsC / 6f, rateP = slotsP / 8f, rateN = slotsN / 10f;
                    avgGrowRate = (rateC + rateP + rateN) * 1.2f;
                }
                if (totalCraft > 0 && orders.Count > 0)
                    avgCraftRate = orders.Count / (totalCraft / 1.35f);

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"L{m.LevelIndex}", GUILayout.Width(ecoW[0]));
                EditorGUILayout.LabelField($"{m.StartCredits}", GUILayout.Width(ecoW[1]));
                EditorGUILayout.LabelField($"{orders.Count}", GUILayout.Width(ecoW[2]));
                EditorGUILayout.LabelField($"{m.TotalOrderReward}", GUILayout.Width(ecoW[3]));
                EditorGUILayout.LabelField($"{m.OptimalUpgradeCost}", GUILayout.Width(ecoW[4]));
                var defColor = m.CreditsForDefense < 5 ? RedBad : m.CreditsForDefense < 15 ? YellowWarn : GreenOk;
                var defStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = defColor } };
                EditorGUILayout.LabelField($"{m.CreditsForDefense}", defStyle, GUILayout.Width(ecoW[5]));
                EditorGUILayout.LabelField($"{avgGrowRate:F2}/s", GUILayout.Width(ecoW[6]));
                EditorGUILayout.LabelField($"{avgCraftRate:F1}/s", GUILayout.Width(ecoW[7]));
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("How to read:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "GrowR = effective resource gathering rate (resources/sec, with avg upgrades).\n" +
                "CraftR = orders completed per second (with avg upgrades).\n" +
                "If GrowR > CraftR, the level is craft-bound (you wait on crafting).\n" +
                "If CraftR > GrowR, the level is resource-bound (you wait on plants).\n" +
                "DefBudget = credits remaining for defense after buying optimal upgrades.",
                MessageType.Info);
        }

        // ─── TAB 3: Enemies ──────────────────────────────────────────────

        private void DrawEnemies()
        {
            _selectedLevel = Mathf.Clamp(_selectedLevel, 1, _config.LevelMetrics?.Count ?? 1);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Level");
            _selectedLevel = EditorGUILayout.IntSlider(_selectedLevel, 1, _config.LevelMetrics?.Count ?? 10);
            EditorGUILayout.EndHorizontal();

            var level = _config.GameConfig.GetLevel(_selectedLevel);
            if (level?.WaveConfig == null || level.WaveConfig.Waves == null || level.WaveConfig.Waves.Count == 0)
            {
                EditorGUILayout.HelpBox("No waves for this level.", MessageType.Info);
                return;
            }

            var idx = _selectedLevel - 1;
            var m = _config.LevelMetrics[idx];

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Wave Breakdown", EditorStyles.boldLabel);

            float[] waveW = { 30, 55, 35, 35, 35, 65, 65, 60 };
            string[] waveH = { "#", "Start", "S", "G", "T", "HP", "DPS", "Interval" };
            GUILayout.BeginHorizontal();
            for (int i = 0; i < waveH.Length; i++)
                EditorGUILayout.LabelField(waveH[i], EditorStyles.boldLabel, GUILayout.Width(waveW[i]));
            GUILayout.EndHorizontal();

            foreach (var wave in level.WaveConfig.Waves)
            {
                float hp = wave.ScoutCount * 18f + wave.GnawerCount * 42f + wave.TankCount * 105f;
                float dps = wave.ScoutCount * (1f / 0.7f) + wave.GnawerCount * (2f / 0.9f) + wave.TankCount * (5f / 1.15f);

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"W{wave.WaveId}", GUILayout.Width(waveW[0]));
                EditorGUILayout.LabelField($"{wave.StartTime:F0}s", GUILayout.Width(waveW[1]));
                EditorGUILayout.LabelField($"{wave.ScoutCount}", GUILayout.Width(waveW[2]));
                EditorGUILayout.LabelField($"{wave.GnawerCount}", GUILayout.Width(waveW[3]));
                EditorGUILayout.LabelField($"{wave.TankCount}", GUILayout.Width(waveW[4]));
                EditorGUILayout.LabelField($"{hp:F0}", GUILayout.Width(waveW[5]));
                EditorGUILayout.LabelField($"{dps:F1}", GUILayout.Width(waveW[6]));
                EditorGUILayout.LabelField($"{wave.IntraSpawnInterval:F2}s", GUILayout.Width(waveW[7]));
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(5);
            if (level.WaveConfig.LoopLastWave)
            {
                EditorGUILayout.LabelField($"Loop: last wave repeats (delay {level.WaveConfig.LoopDelay}s)");
            }

            EditorGUILayout.Space(10);
            DrawMetricSection("Time-Aware Analysis", () =>
            {
                DrawMetricRow("Economic time (T_econ)", $"{m.MinClearTimeSec:F0}s",
                    "waves with start < T_econ matter");
                DrawMetricRow("Click distraction", $"{m.ClickDistractionSec:F1}s",
                    m.ClickDistractionSec > m.MinClearTimeSec * 0.3f ? "heavy" : "light",
                    m.ClickDistractionSec > m.MinClearTimeSec * 0.3f ? YellowWarn : GreenOk);
                DrawMetricRow("Enemies reaching brain", m.ReachingBrain > 0 ? m.ReachingBrain.ToString() : "0 ✓");
                DrawMetricRow("Brain survival", m.BrainSurvivalSec >= 999f ? "∞ safe ✓" : $"{m.BrainSurvivalSec:F1}s",
                    m.BrainSurvivalSec < 10f ? "critical" : m.BrainSurvivalSec < 30f ? "tight" : "safe",
                    m.BrainSurvivalSec < 10f ? RedBad : m.BrainSurvivalSec < 30f ? YellowWarn : GreenOk);
            });

            EditorGUILayout.Space(15);
            DrawMetricSection("Turret Effectiveness", () =>
            {
                DrawMetricRow($"Scout (18 HP)", $"TTK: {18f / 8f * 0.75f:F1}s", $"2 shots");
                DrawMetricRow($"Gnawer (42 HP)", $"TTK: {42f / 8f * 0.75f:F1}s", $"6 shots");
                DrawMetricRow($"Tank (105 HP)", $"TTK: {105f / 8f * 0.75f:F1}s", $"14 shots");
                DrawMetricRow("Turret DPS", $"{8f / 0.75f:F1}");
            });

            EditorGUILayout.Space(8);
            DrawMetricSection("Barricade Absorption", () =>
            {
                DrawMetricRow("Vs Scout", $"40 attacks (80 HP / 2 dmg)");
                DrawMetricRow("Vs Gnawer", $"16 attacks (80 HP / 5 dmg)");
                DrawMetricRow("Vs Tank", $"7 attacks (80 HP / 11 dmg)");
            });
        }

        // ─── TAB 4: Warnings ─────────────────────────────────────────────

        private void DrawWarnings()
        {
            EditorGUILayout.LabelField("Balance Warnings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            if (_config.GlobalWarnings == null || _config.GlobalWarnings.Count == 0)
            {
                EditorGUILayout.HelpBox("No warnings — all levels look balanced!", MessageType.Info);
                return;
            }

            foreach (var w in _config.GlobalWarnings)
            {
                bool critical = w.Contains("Brain survives") || w.Contains("Only ") || w.Contains(">50%");
                EditorGUILayout.HelpBox(w, critical ? MessageType.Error : MessageType.Warning);
            }
        }

        // ─── HELPERS ─────────────────────────────────────────────────────

        private void DrawMetricSection(string title, System.Action drawContent)
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            drawContent();
            EditorGUI.indentLevel--;
        }

        private void DrawMetricRow(string label, string value, string hint = "", Color? color = null)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(180));
            var style = new GUIStyle(EditorStyles.label);
            if (color.HasValue) style.normal.textColor = color.Value;
            EditorGUILayout.LabelField(value, style, GUILayout.Width(140));
            if (!string.IsNullOrEmpty(hint))
                EditorGUILayout.LabelField(hint, EditorStyles.miniLabel);
            GUILayout.EndHorizontal();
        }

        private List<TaskData> GetOrdersForLevel(int levelIndex)
        {
            return _config != null ? BuildTaskListInternal(levelIndex) : new List<TaskData>();
        }

        private List<TaskData> BuildTaskListInternal(int levelIndex)
        {
            if (_config.GameConfig?.LevelOrdersConfig?.Entries == null || _config.GameConfig.TaskConfig?.Tasks == null)
                return new List<TaskData>();

            var entries = new List<LevelOrderEntry>();
            foreach (var e in _config.GameConfig.LevelOrdersConfig.Entries)
            {
                if (e.LevelId == levelIndex)
                    entries.Add(e);
            }
            entries.Sort((a, b) => a.OrderIndex.CompareTo(b.OrderIndex));

            var result = new List<TaskData>(entries.Count);
            foreach (var entry in entries)
            {
                var allTasks = _config.GameConfig.TaskConfig.Tasks;
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
    }
}

