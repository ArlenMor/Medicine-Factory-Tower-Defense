using System;
using System.Collections;
using _Project.Code.Scripts.Data;
using _Project.Code.Scripts.Game;
using _Project.Code.Scripts.Localization;
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
        private LocalizationService _loc;

        public override void Initialize(PanelSettings settings)
        {
            base.Initialize(settings);

            if (settings is not GameOverPanelSettings gameOverSettings)
            {
                Debug.LogError("[GameOverPanel] Invalid settings");
                return;
            }

            _gameOverSettings = gameOverSettings;
            _loc = S.Get<LocalizationService>();

            if (gameOverSettings.IsGameComplete)
            {
                _titleText.text = _loc.GetString("game_complete_title");
                _finalButtonText.text = _loc.GetString("game_complete_new_game");
                _finalButton.onClick.AddListener(StartNewGame);
            }
            else if (gameOverSettings.IsVictory)
            {
                _titleText.text = _loc.GetString("gameover_victory");
                _finalButtonText.text = _loc.GetString("gameover_next_level");
                _finalButton.onClick.AddListener(HidePanel);
            }
            else
            {
                _titleText.text = _loc.GetString("gameover_defeat");
                _finalButtonText.text = _loc.GetString("gameover_retry_level", gameOverSettings.LevelIndex);
                _finalButton.onClick.AddListener(HidePanel);
            }

            var stats = GameData.Instance.Stats;
            var time = TimeSpan.FromSeconds(stats.TimePlayed);
            _statsText.text = string.Join("\n",
                _loc.GetString("stat_enemies_killed", stats.EnemiesKilled),
                _loc.GetString("stat_resources_collected", stats.ResourcesCollected),
                _loc.GetString("stat_plants_planted", stats.PlantsPlanted),
                _loc.GetString("stat_credits_earned", stats.CreditsEarned),
                _loc.GetString("stat_time_played", time.ToString(@"mm\:ss")),
                _loc.GetString("stat_upgrades_purchased", stats.UpgradesPurchased),
                _loc.GetString("stat_turrets_built", stats.TurretsBuilt),
                _loc.GetString("stat_barricades_built", stats.BarricadesBuilt)
            );

            _canvasGroup.alpha = 0f;
            StartCoroutine(FadeIn());
        }

        private void HidePanel()
        {
            S.Get<IPanelShower>().HideView(PanelType.GameOver);
        }

        private void StartNewGame()
        {
            S.Get<GameController>().StartNewGame();
            HidePanel();
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
            _finalButton.onClick.RemoveAllListeners();
        }
    }
}
