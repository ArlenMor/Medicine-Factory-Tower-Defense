using System;
using System.Net;
using System.Text.RegularExpressions;
using _Project.Code.Scripts;
using _Project.Code.Scripts.Configs;
using UnityEditor;
using UnityEngine;

namespace _Project.Code.Editor.SpreadsheetImporter
{
    public class SpreadsheetImporterWindow : EditorWindow
    {
        // ── Serialised state (survives domain reload via EditorPrefs key) ──────
        private DefaultAsset _excelFile;
        private string _googleSheetsUrl = string.Empty;

        private TaskConfig _taskConfig;
        private LevelOrdersConfig _levelOrdersConfig;
        private GameConfig _gameConfig;

        private bool _isBusy;

        // ── Menu item ──────────────────────────────────────────────────────────

        [MenuItem("Tools/Spreadsheet Importer")]
        public static void ShowWindow()
        {
            var window = GetWindow<SpreadsheetImporterWindow>("Spreadsheet Importer");
            window.minSize = new Vector2(420, 320);
        }

        // ── GUI ────────────────────────────────────────────────────────────────

        private void OnGUI()
        {
            GUILayout.Label("Источник данных", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Укажите локальный .xlsx файл ИЛИ ссылку на Google Sheets (таблица должна быть открыта для чтения).",
                MessageType.None);

            EditorGUILayout.Space(4);

            _excelFile = (DefaultAsset)EditorGUILayout.ObjectField(
                "Excel файл (.xlsx)", _excelFile, typeof(DefaultAsset), false);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("— или —", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.Space(4);

            _googleSheetsUrl = EditorGUILayout.TextField("Google Sheets URL", _googleSheetsUrl);

            EditorGUILayout.Space(12);
            GUILayout.Label("Целевые ассеты", EditorStyles.boldLabel);

            _taskConfig = (TaskConfig)EditorGUILayout.ObjectField(
                "Task Config", _taskConfig, typeof(TaskConfig), false);

            _levelOrdersConfig = (LevelOrdersConfig)EditorGUILayout.ObjectField(
                "Level Orders Config", _levelOrdersConfig, typeof(LevelOrdersConfig), false);

            _gameConfig = (GameConfig)EditorGUILayout.ObjectField(
                "Game Config", _gameConfig, typeof(GameConfig), false);

            EditorGUILayout.Space(12);

            string validationError = GetValidationError();
            bool canImport = validationError == null && !_isBusy;

            using (new EditorGUI.DisabledScope(!canImport))
            {
                if (GUILayout.Button(_isBusy ? "Импортирую…" : "Import", GUILayout.Height(32)))
                    DoImport();
            }

            if (validationError != null)
                EditorGUILayout.HelpBox(validationError, MessageType.Warning);
        }

        // ── Validation ─────────────────────────────────────────────────────────

        private string GetValidationError()
        {
            if (_excelFile == null && string.IsNullOrWhiteSpace(_googleSheetsUrl))
                return "Укажите Excel файл или Google Sheets URL.";
            if (_taskConfig == null)
                return "Назначьте Task Config.";
            if (_levelOrdersConfig == null)
                return "Назначьте Level Orders Config.";
            if (_gameConfig == null)
                return "Назначьте Game Config.";
            return null;
        }

        // ── Import ─────────────────────────────────────────────────────────────

        private void DoImport()
        {
            _isBusy = true;
            Repaint();

            try
            {
                byte[] data = FetchData();
                var sheets = XlsxReader.Read(data);

                if (sheets.Count == 0)
                    throw new Exception("Не найдено ни одного листа в файле. Проверьте формат (.xlsx).");

                SpreadsheetImportProcessor.Process(sheets, _taskConfig, _levelOrdersConfig, _gameConfig);

                EditorUtility.DisplayDialog("Импорт завершён",
                    $"Успешно импортировано {sheets.Count} листов.", "OK");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SpreadsheetImporter] {e}");
                EditorUtility.DisplayDialog("Ошибка импорта", e.Message, "OK");
            }
            finally
            {
                _isBusy = false;
                Repaint();
            }
        }

        private byte[] FetchData()
        {
            if (_excelFile != null)
            {
                string path = AssetDatabase.GetAssetPath(_excelFile);
                return System.IO.File.ReadAllBytes(path);
            }

            string exportUrl = ToExportUrl(_googleSheetsUrl.Trim());

            var request = (HttpWebRequest)WebRequest.Create(exportUrl);
            request.Timeout = 30_000;        // 30 секунд
            request.Proxy = null;            // отключаем авто-определение прокси (частая причина зависания в Unity Mono)
            request.AllowAutoRedirect = true;

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var ms = new System.IO.MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        // ── Google Sheets URL helpers ──────────────────────────────────────────

        private static string ToExportUrl(string url)
        {
            var match = Regex.Match(url,
                @"docs\.google\.com/spreadsheets/d/([A-Za-z0-9_\-]+)");

            if (!match.Success)
                throw new Exception(
                    "Не удалось распознать Google Sheets URL.\n" +
                    "Ожидается формат: https://docs.google.com/spreadsheets/d/{ID}/...");

            string id = match.Groups[1].Value;
            return $"https://docs.google.com/spreadsheets/d/{id}/export?format=xlsx";
        }
    }
}
