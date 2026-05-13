using System;
using System.Collections.Generic;
using System.Globalization;
using _Project.Code.Scripts;
using _Project.Code.Scripts.Configs;
using _Project.Code.Scripts.Data;
using _Project.Code.Scripts.Data.TaskData;
using UnityEditor;
using UnityEngine;

namespace _Project.Code.Editor.SpreadsheetImporter
{
    public static class SpreadsheetImportProcessor
    {
        public static void Process(
            Dictionary<string, List<Dictionary<string, string>>> sheets,
            TaskConfig taskConfig,
            LevelOrdersConfig levelOrdersConfig,
            GameConfig gameConfig)
        {
            if (sheets.TryGetValue("Orders", out var ordersRows))
                ProcessOrders(ordersRows, taskConfig);

            if (sheets.TryGetValue("LevelOrders", out var levelOrdersRows))
                ProcessLevelOrders(levelOrdersRows, levelOrdersConfig);

            if (sheets.TryGetValue("Levels", out var levelsRows))
                ProcessLevels(levelsRows, gameConfig);

            EditorUtility.SetDirty(taskConfig);
            EditorUtility.SetDirty(levelOrdersConfig);
            EditorUtility.SetDirty(gameConfig);

            foreach (var level in gameConfig.Levels)
            {
                if (level != null)
                    EditorUtility.SetDirty(level);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // ── Orders ─────────────────────────────────────────────────────────────

        private static void ProcessOrders(List<Dictionary<string, string>> rows, TaskConfig taskConfig)
        {
            var tasks = new List<TaskData>();

            foreach (var row in rows)
            {
                if (!TryGetInt(row, "order_id", out int orderId))
                    continue;

                var task = new TaskData
                {
                    ResultType    = (MedicationsType)orderId,
                    CreditReward  = GetInt(row, "reward_credits"),
                    ProduceTime   = GetFloat(row, "craft_time_sec"),
                    CostInfo = new ProductionCost
                    {
                        CrystalCost = GetInt(row, "crystal_cost"),
                        PolymerCost = GetInt(row, "polymer_cost"),
                        NanoGelCost = GetInt(row, "nanogel_cost"),
                    }
                };

                tasks.Add(task);
            }

            taskConfig.Tasks = tasks;
            Debug.Log($"[SpreadsheetImporter] Orders: импортировано {tasks.Count} записей.");
        }

        // ── LevelOrders ────────────────────────────────────────────────────────

        private static void ProcessLevelOrders(List<Dictionary<string, string>> rows, LevelOrdersConfig config)
        {
            var entries = new List<LevelOrderEntry>();

            foreach (var row in rows)
            {
                if (!TryGetInt(row, "level_id", out int levelId))
                    continue;

                entries.Add(new LevelOrderEntry
                {
                    LevelId    = levelId,
                    OrderIndex = GetInt(row, "order_index"),
                    OrderId    = GetInt(row, "order_id"),
                });
            }

            config.Entries = entries;
            Debug.Log($"[SpreadsheetImporter] LevelOrders: импортировано {entries.Count} записей.");
        }

        // ── Levels ─────────────────────────────────────────────────────────────

        private static void ProcessLevels(List<Dictionary<string, string>> rows, GameConfig gameConfig)
        {
            int updated = 0;

            foreach (var row in rows)
            {
                if (!TryGetInt(row, "level_id", out int levelId))
                    continue;

                // level_id is 1-based; Levels list is 0-based
                int idx = levelId - 1;
                if (idx < 0 || idx >= gameConfig.Levels.Count)
                {
                    Debug.LogWarning($"[SpreadsheetImporter] level_id={levelId} выходит за пределы списка Levels ({gameConfig.Levels.Count} уровней). Пропущено.");
                    continue;
                }

                var levelConfig = gameConfig.Levels[idx];
                if (levelConfig == null)
                    continue;

                levelConfig.StartCredits = GetInt(row, "start_credits");
                updated++;
            }

            Debug.Log($"[SpreadsheetImporter] Levels: обновлено {updated} LevelConfig.");
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static bool TryGetInt(Dictionary<string, string> row, string key, out int value)
        {
            value = 0;
            if (!row.TryGetValue(key, out var s))
                return false;
            return TryParseInt(s, out value);
        }

        private static int GetInt(Dictionary<string, string> row, string key)
        {
            if (row.TryGetValue(key, out var s) && TryParseInt(s, out int v))
                return v;
            return 0;
        }

        private static bool TryParseInt(string s, out int value)
        {
            s = s.Trim();
            if (int.TryParse(s, out value))
                return true;
            if (float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out float f))
            {
                value = (int)f;
                return true;
            }
            value = 0;
            return false;
        }

        private static float GetFloat(Dictionary<string, string> row, string key)
        {
            if (row.TryGetValue(key, out var s) &&
                float.TryParse(s.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out float v))
                return v;
            return 0f;
        }
    }
}
