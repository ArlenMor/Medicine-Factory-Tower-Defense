using System;
using System.Collections.Generic;
using _Project.Code.Scripts.Data;
using _Project.Code.Scripts.ServiceLocator;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.Scripts.UI
{
    public class FlyingIconService : MonoBehaviour
    {
        [Serializable]
        private struct ResourceHudTarget
        {
            public ResourceType Type;
            public RectTransform Target;
        }

        [SerializeField] private FlyingIconView _iconPrefab;

        [Tooltip("Canvas с renderMode = Screen Space Overlay и высоким Sort Order. " +
                 "Иконки будут спауниться сюда и рисоваться поверх всего.")]
        [SerializeField] private Canvas _overlayCanvas;

        [Tooltip("Камера для перевода мировых координат в экранные. " +
                 "Если не задана — используется Camera.main.")]
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private RectTransform _parent;

        [Header("Animation")]
        [SerializeField] private float _duration = 0.7f;

        [Tooltip("Высота дуги в пикселях канваса. " +
                 "Положительное значение — дуга идёт вверх по экрану.")]
        [SerializeField] private float _arcHeight = 200f;

        [SerializeField] private Ease _moveEase = Ease.InOutSine;

        [Header("HUD Targets")]
        [Tooltip("Для каждого ResourceType укажи RectTransform иконки в HUD. " +
                 "Используется в FlyResource().")]
        [SerializeField] private ResourceHudTarget[] _resourceHudTargets;

        [Tooltip("RectTransform иконки монет/кредитов в HUD. " +
                 "Используется в FlyCoin().")]
        [SerializeField] private RectTransform _creditHudTarget;

        private RectTransform _canvasRect;
        private Dictionary<ResourceType, RectTransform> _hudTargetLookup;


        public event Action<ResourceType> OnIconArrived;

        private void Awake()
        {
            _canvasRect = (RectTransform)_overlayCanvas.transform;

            _hudTargetLookup = new Dictionary<ResourceType, RectTransform>();
            foreach (var entry in _resourceHudTargets)
                _hudTargetLookup[entry.Type] = entry.Target;
        }

        public void Fly(Transform from, Transform to, Sprite sprite, Action onComplete = null)
        {
            Vector2 startScreen = TransformToScreenPoint(from);
            Vector2 endScreen = TransformToScreenPoint(to);
            SpawnAndAnimate(startScreen, endScreen, sprite, onComplete);
        }

        public void FlyResource(Transform from, ResourceType resourceType, Action onComplete = null)
        {
            var sprite = GameData.Instance.GameConfig.ResourceIconConfig.GetIcon(resourceType);
            if (!_hudTargetLookup.TryGetValue(resourceType, out var target) || target == null)
            {
                Debug.LogWarning($"[FlyingIconService] HUD target not found for ResourceType={resourceType}. "
                    + "Check _resourceHudTargets in Inspector.");
                return;
            }
            Debug.Log($"[FlyingIconService] FlyResource type={resourceType} from={from.position} to={target.name}");
            Fly(from, target, sprite, () => { OnIconArrived?.Invoke(resourceType); onComplete?.Invoke(); });
        }

        public void FlyResource(Vector3 worldFrom, ResourceType resourceType, Action onComplete = null)
        {
            var sprite = GameData.Instance.GameConfig.ResourceIconConfig.GetIcon(resourceType);
            if (!_hudTargetLookup.TryGetValue(resourceType, out var target) || target == null)
            {
                Debug.LogWarning($"[FlyingIconService] HUD target not found for ResourceType={resourceType}. "
                    + "Check _resourceHudTargets in Inspector.");
                return;
            }
            Debug.Log($"[FlyingIconService] FlyResource type={resourceType} worldFrom={worldFrom} to={target.name}");
            Fly(worldFrom, target, sprite, () => { OnIconArrived?.Invoke(resourceType); onComplete?.Invoke(); });
        }

        public void FlyCoin(Transform from, Action onComplete = null)
        {
            if (_creditHudTarget == null)
            {
                Debug.LogWarning("[FlyingIconService] _creditHudTarget is not assigned in Inspector.");
                return;
            }
            Debug.Log($"[FlyingIconService] FlyCoin from={from.name}");
            var sprite = GameData.Instance.GameConfig.ResourceIconConfig.GetIcon(ResourceType.Credit);
            Fly(from, _creditHudTarget, sprite, () => { OnIconArrived?.Invoke(ResourceType.Credit); onComplete?.Invoke(); });
        }

        public void Fly(Vector3 worldFrom, Transform to, Sprite sprite, Action onComplete = null)
        {
            Vector2 startScreen = GetWorldCamera().WorldToScreenPoint(worldFrom);
            Vector2 endScreen = TransformToScreenPoint(to);
            SpawnAndAnimate(startScreen, endScreen, sprite, onComplete);
        }

        private void SpawnAndAnimate(Vector2 startScreen, Vector2 endScreen, Sprite sprite, Action onComplete)
        {
            if (sprite == null)
                Debug.LogWarning("[FlyingIconService] Sprite is null — icon will be invisible.");

            Camera canvasCam = GetCanvasCamera();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect, startScreen, canvasCam, out Vector2 startLocal);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect, endScreen, canvasCam, out Vector2 endLocal);

            Debug.Log($"[FlyingIconService] Spawning icon: startScreen={startScreen} endScreen={endScreen} "
                + $"startLocal={startLocal} endLocal={endLocal}");

            // Контрольная точка: середина отрезка смещена вверх по экрану — формирует дугу
            Vector2 controlPoint = (startLocal + endLocal) * 0.5f + Vector2.up * _arcHeight;

            var icon = Instantiate(_iconPrefab, _parent);
            icon.Initialize(sprite);

            RectTransform rt = icon.RectTransform;
            rt.anchoredPosition = startLocal;
            rt.localScale = Vector3.zero;

            float t = 0f;

            DOTween.Sequence()
                // Небольшой pop-in в точке старта
                .Append(rt.DOScale(Vector3.one, 0.12f).SetEase(Ease.OutBack))
                // Полёт по дуге (квадратичный безье)
                .Append(DOTween.To(
                    () => t,
                    x =>
                    {
                        t = x;
                        rt.anchoredPosition = QuadraticBezier(startLocal, controlPoint, endLocal, x);
                    },
                    1f,
                    _duration).SetEase(_moveEase))
                .OnComplete(() =>
                {
                    Destroy(icon.gameObject);
                    onComplete?.Invoke();
                })
                .SetAutoKill(true);
        }

        private Vector2 TransformToScreenPoint(Transform t)
        {
            // RectTransform — UI-элемент, мировой Transform — объект сцены
            if (t is RectTransform)
                return RectTransformUtility.WorldToScreenPoint(GetCanvasCamera(), t.position);

            return GetWorldCamera().WorldToScreenPoint(t.position);
        }

        private Camera GetWorldCamera() =>
            _worldCamera != null ? _worldCamera : Camera.main;

        private Camera GetCanvasCamera() =>
            _overlayCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : _overlayCanvas.worldCamera;

        private static Vector2 QuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            float u = 1f - t;
            return u * u * p0 + 2f * u * t * p1 + t * t * p2;
        }
    }
}
