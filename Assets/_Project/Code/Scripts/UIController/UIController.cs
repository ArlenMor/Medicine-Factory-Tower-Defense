using System.Collections.Generic;
using _Project.Code.Scripts.InputResolverService;
using UnityEngine;

namespace _Project.Code.Scripts.UIService
{
    public class UIController : MonoBehaviour, IPanelShower
    {
        [SerializeField] private Transform _panelRoot;
        [SerializeField] private PanelPrefabConfig _config;

        private readonly Dictionary<PanelType, BasePanel> _activePanels = new();
        private IInputResolver _inputResolver;

        public void Initialize(IInputResolver inputResolver)
        {
            _inputResolver = inputResolver;
            _inputResolver.OnPointerDown += HandlePointerDown;
        }

        private void OnDestroy()
        {
            if (_inputResolver != null)
            {
                _inputResolver.OnPointerDown -= HandlePointerDown;
            }
        }

        private void HandlePointerDown(InputEventData data)
        {
            if (!data.IsCanvasHit)
                return;
        }

        public BasePanel ShowView(PanelType type, PanelSettings settings = null, Transform parentOverride = null)
        {
            if (_activePanels.TryGetValue(type, out var existing))
            {
                existing.Initialize(settings);
                return existing;
            }

            var prefab = _config.GetPrefab(type);
            if (prefab == null)
            {
                Debug.LogWarning($"[UIManager] No prefab configured for panel type '{type}'");
                return null;
            }

            var parent = parentOverride ?? _panelRoot;
            var panel = Instantiate(prefab, parent);
            panel.Initialize(settings);
            _activePanels[type] = panel;
            return panel;
        }

        public void HideView(PanelType type)
        {
            if (!_activePanels.TryGetValue(type, out var panel))
                return;

            _activePanels.Remove(type);
            panel.Close();
        }

        public bool IsShown(PanelType type) => _activePanels.ContainsKey(type);
    }
}