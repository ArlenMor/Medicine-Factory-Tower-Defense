using System;
using System.Collections;
using _Project.Code.Scripts.Data;
using _Project.Code.Scripts.ServiceLocator;
using _Project.Code.Scripts.UIPanels.Settings;
using _Project.Code.Scripts.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.Scripts.UIPanels.Panels
{
    public class GameOverPanel : BasePanel
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _statsText;
        [SerializeField] private Button _finalButton;
        [SerializeField] private TMP_Text _finalButtonText;
        [SerializeField] private float _fadeDuration = 0.5f;

        private GameOverPanelSettings _gameOverSettings;

        public override void Initialize(PanelSettings settings)
        {
            base.Initialize(settings);

            if (settings is not GameOverPanelSettings gameOverSettings)
            {
                Debug.LogError("[GameOverPanel] Invalid settings");
                return;
            }

            _gameOverSettings = gameOverSettings;
            _finalButtonText.text = gameOverSettings.IsVictory ? "Next Level" : $"Retry Level {gameOverSettings.LevelIndex}";
            _titleText.text = gameOverSettings.IsVictory ? "Victory!" : "Defeat!";

            var stats = GameData.Instance.Stats;
            var time = TimeSpan.FromSeconds(stats.TimePlayed);
            _statsText.text = $"Enemies killed: {stats.EnemiesKilled}\n" +
                              $"Resources collected: {stats.ResourcesCollected}\n" +
                              $"Plants planted: {stats.PlantsPlanted}\n" +
                              $"Credits earned: {stats.CreditsEarned}\n" +
                              $"Time played: {time:mm\\:ss}\n" +
                              $"Upgrades purchased: {stats.UpgradesPurchased}\n" +
                              $"Turrets built: {stats.TurretsBuilt}\n" +
                              $"Barricades built: {stats.BarricadesBuilt}";

            _finalButton.onClick.AddListener(HidePanel);

            _canvasGroup.alpha = 0f;
            StartCoroutine(FadeIn());
        }

        private void HidePanel()
        {
            S.Get<IPanelShower>().HideView(PanelType.GameOver);
        }

        private IEnumerator FadeIn()
        {
            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Clamp01(elapsed / _fadeDuration);
                yield return null;
            }
            _canvasGroup.alpha = 1f;
        }

        private void OnDestroy()
        {
            _finalButton.onClick.RemoveListener(HidePanel);
        }
    }
}
