using DG.Tweening;
using TMPro;
using _Project.Code.Scripts.ServiceLocator;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.Scripts.Tutorial
{

    public class TutorialOverlayView : MonoBehaviour
    {
        [Header("Overlay")]
        [SerializeField] private Image _overlayImage;

        [Header("Text panel")]
        [SerializeField] private RectTransform _textPanelRect;
        [SerializeField] private TextMeshProUGUI _instructionText;
        [SerializeField] private Button _nextButton;
        [SerializeField] private CanvasGroup _textPanelGroup;

        [Header("World Camera")]
        [SerializeField] private Camera _worldCamera;

        [Header("Animation")]
        [SerializeField] private float _fadeDuration = 0.3f;

        [Header("Other")]
        [SerializeField] private Shader _overlayShader;
        [SerializeField] private TutorialRaycastBlocker _inputBlocker;


        private static readonly string[] HolePropertyNames =
        {
            "_Hole0", "_Hole1", "_Hole2", "_Hole3"
        };

        private static readonly int OverlayColorId = Shader.PropertyToID("_OverlayColor");
        private static readonly int[] HoleIds = new int[4];

        private ITutorialService _tutorialService;
        private ITutorialTargetRegistry _targetRegistry;
        private TutorialStepData _currentStep;
        private Tween _fadeTween;
        private Tween _textTween;
        private Material _overlayMaterial;

        private void Awake()
        {
            for (int i = 0; i < 4; i++)
                HoleIds[i] = Shader.PropertyToID(HolePropertyNames[i]);

            _overlayMaterial = new Material(_overlayShader);
            _overlayImage.material = _overlayMaterial;
            _overlayImage.raycastTarget = false;
            _textPanelGroup.alpha = 0f;

            SetOverlayAlpha(0f);
            _textPanelRect.gameObject.SetActive(false);
            _nextButton.gameObject.SetActive(false);
        }

        public void Initialize(ITutorialService tutorialService)
        {
            _tutorialService = tutorialService;
            _tutorialService.OnStepStarted       += OnStepStarted;
            _tutorialService.OnStepHidden        += HideOverlay;
            _tutorialService.OnSequenceCompleted += OnSequenceCompleted;

            _nextButton.onClick.AddListener(OnNextClicked);
        }

        private void OnDestroy()
        {
            if (_tutorialService != null)
            {
                _tutorialService.OnStepStarted       -= OnStepStarted;
                _tutorialService.OnStepHidden        -= HideOverlay;
                _tutorialService.OnSequenceCompleted -= OnSequenceCompleted;
            }

            _nextButton.onClick.RemoveListener(OnNextClicked);
            _fadeTween?.Kill();
            _textTween?.Kill();

            if (_overlayMaterial != null)
                Destroy(_overlayMaterial);
        }

        private void Update()
        {
            if (_currentStep != null && _tutorialService is { IsActive: true })
                RefreshHoles(_currentStep);
        }

        // ── Events ──────────────────────────────────────────────────────────────

        private void OnStepStarted(TutorialStepData step)
        {
            _currentStep = step;
            bool wasVisible = _textPanelGroup.alpha > 0.01f;

            RefreshHoles(step);

            bool hasHighlights = step.Highlights.Count > 0;
            bool shouldBlock = step.BlockInput switch
            {
                InputBlockMode.AlwaysBlock => true,
                InputBlockMode.NeverBlock  => false,
                _                          => hasHighlights,
            };
            _overlayImage.gameObject.SetActive(true);
            _overlayImage.raycastTarget = shouldBlock;
            _inputBlocker.SetBlocking(shouldBlock);

            _fadeTween?.Kill();
            _fadeTween = DOVirtual.Float(
                GetOverlayAlpha(), 1f, _fadeDuration,
                v => SetOverlayAlpha(v));

            _textTween?.Kill();
            if (wasVisible)
            {
                _textTween = DOTween.Sequence()
                    .Append(_textPanelGroup.DOFade(0f, _fadeDuration * 0.4f))
                    .AppendCallback(() => PopulateStep(step))
                    .Append(_textPanelGroup.DOFade(1f, _fadeDuration * 0.6f));
            }
            else
            {
                _textPanelRect.gameObject.SetActive(true);
                _textPanelGroup.alpha = 0f;
                PopulateStep(step);
                _textTween = _textPanelGroup.DOFade(1f, _fadeDuration);
            }
        }

        private void PopulateStep(TutorialStepData step)
        {
            _instructionText.text = step.InstructionText;
            _textPanelRect.gameObject.SetActive(true);
            ApplyTextPosition(step);
            _nextButton.gameObject.SetActive(step.Trigger == StepTrigger.ManualButton);
        }

        private void OnSequenceCompleted()
        {
            HideOverlay();
        }

        private void HideOverlay()
        {
            _currentStep = null;

            _overlayImage.raycastTarget = false;
            _inputBlocker.SetBlocking(false);

            ClearHoles();

            _fadeTween?.Kill();
            _fadeTween = DOVirtual.Float(
                GetOverlayAlpha(), 0f, _fadeDuration,
                v => SetOverlayAlpha(v));

            _textTween?.Kill();
            _textTween = _textPanelGroup.DOFade(0f, _fadeDuration)
                .OnComplete(() =>
                {
                    _textPanelRect.gameObject.SetActive(false);
                    _nextButton.gameObject.SetActive(false);
                });
        }

        private void OnNextClicked()
        {
            _tutorialService?.AdvanceManually();
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        private void RefreshHoles(TutorialStepData step)
        {
            // Ленивая инициализация registry — после Bootstrap гарантированно зарегистрирован
            _targetRegistry ??= S.TryGet<ITutorialTargetRegistry>(out var reg) ? reg : null;

            var highlights = step.Highlights;
            var holeRects = new Vector4[4];
            for (int i = 0; i < 4; i++)
            {
                Vector4 rect;
                if (i < highlights.Count && IsHighlightValid(highlights[i], _targetRegistry))
                {
                    rect = CalculateHoleRect(highlights[i], _worldCamera, _targetRegistry);
                }
                else
                {
                    rect = new Vector4(-1, -1, 0, 0);
                }
                _overlayMaterial.SetVector(HoleIds[i], rect);
                holeRects[i] = rect;
            }
            _inputBlocker.SetHoles(holeRects);
        }

        private void ClearHoles()
        {
            for (int i = 0; i < 4; i++)
                _overlayMaterial.SetVector(HoleIds[i], new Vector4(-1, -1, 0, 0));
        }

        private static Vector4 CalculateHoleRect(TutorialHighlightTarget highlight, Camera worldCam, ITutorialTargetRegistry registry)
        {
            return highlight.Type == HighlightTargetType.World
                ? CalculateWorldHoleRect(highlight, worldCam, registry)
                : CalculateUIHoleRect(highlight, registry);
        }

        private static Vector4 CalculateUIHoleRect(TutorialHighlightTarget highlight, ITutorialTargetRegistry registry)
        {
            var rt = ResolveRect(highlight, registry);
            if (rt == null) return new Vector4(-1, -1, 0, 0);
            var corners = new Vector3[4];
            rt.GetWorldCorners(corners);

            var rootCanvas = rt.GetComponentInParent<Canvas>()?.rootCanvas;
            Camera cam = rootCanvas?.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas?.worldCamera;

            Vector2 min = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);

            return BuildNormalizedRect(min, max, highlight.Padding);
        }

        private static Vector4 CalculateWorldHoleRect(TutorialHighlightTarget highlight, Camera worldCam, ITutorialTargetRegistry registry)
        {
            var target = ResolveTransform(highlight, registry);
            if (target == null) return new Vector4(-1, -1, 0, 0);

            worldCam ??= Camera.main;
            if (worldCam == null) return new Vector4(-1, -1, 0, 0);

            Bounds bounds;
            var renderer = target.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                bounds = renderer.bounds;
            }
            else
            {
                float half = highlight.WorldSize * 0.5f;
                bounds = new Bounds(target.position, Vector3.one * half * 2f);
            }

            Vector3 bMin = bounds.min;
            Vector3 bMax = bounds.max;
            Vector3[] corners =
            {
                new(bMin.x, bMin.y, bMin.z), new(bMax.x, bMin.y, bMin.z),
                new(bMin.x, bMax.y, bMin.z), new(bMax.x, bMax.y, bMin.z),
                new(bMin.x, bMin.y, bMax.z), new(bMax.x, bMin.y, bMax.z),
                new(bMin.x, bMax.y, bMax.z), new(bMax.x, bMax.y, bMax.z),
            };

            Vector2 screenMin = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 screenMax = new Vector2(float.MinValue, float.MinValue);
            foreach (var c in corners)
            {
                Vector2 sp = worldCam.WorldToScreenPoint(c);
                screenMin = Vector2.Min(screenMin, sp);
                screenMax = Vector2.Max(screenMax, sp);
            }

            return BuildNormalizedRect(screenMin, screenMax, highlight.Padding);
        }

        private static Vector4 BuildNormalizedRect(Vector2 min, Vector2 max, float padding)
        {
            min -= Vector2.one * padding;
            max += Vector2.one * padding;

            float sw = Screen.width;
            float sh = Screen.height;
            return new Vector4(min.x / sw, min.y / sh, (max.x - min.x) / sw, (max.y - min.y) / sh);
        }

        private static bool IsHighlightValid(TutorialHighlightTarget h, ITutorialTargetRegistry registry)
        {
            if (!string.IsNullOrEmpty(h.TargetId))
                return registry != null && registry.TryGet(h.TargetId, out _);
            return h.Type == HighlightTargetType.UI ? h.UITarget != null : h.WorldTarget != null;
        }

        private static RectTransform ResolveRect(TutorialHighlightTarget h, ITutorialTargetRegistry registry)
        {
            if (!string.IsNullOrEmpty(h.TargetId) && registry != null)
                if (registry.TryGetRect(h.TargetId, out var rt)) return rt;
            return h.UITarget;
        }

        private static Transform ResolveTransform(TutorialHighlightTarget h, ITutorialTargetRegistry registry)
        {
            if (!string.IsNullOrEmpty(h.TargetId) && registry != null)
                if (registry.TryGet(h.TargetId, out var t)) return t;
            return h.WorldTarget;
        }

        // ── Text positioning ─────────────────────────────────────────────────────

        private void ApplyTextPosition(TutorialStepData step)
        {
            Vector2 anchor = ResolveTextAnchor(step);
            _textPanelRect.anchorMin = anchor;
            _textPanelRect.anchorMax = anchor;
            _textPanelRect.pivot     = anchor;
            _textPanelRect.anchoredPosition = Vector2.zero;
        }

        private static Vector2 ResolveTextAnchor(TutorialStepData step) => step.TextPosition switch
        {
            TextPanelPosition.TopLeft      => new Vector2(0.1f, 0.9f),
            TextPanelPosition.TopCenter    => new Vector2(0.5f, 0.9f),
            TextPanelPosition.TopRight     => new Vector2(0.9f, 0.9f),
            TextPanelPosition.MiddleLeft   => new Vector2(0.1f, 0.5f),
            TextPanelPosition.MiddleCenter => new Vector2(0.5f, 0.5f),
            TextPanelPosition.MiddleRight  => new Vector2(0.9f, 0.5f),
            TextPanelPosition.BottomLeft   => new Vector2(0.1f, 0.1f),
            TextPanelPosition.BottomCenter => new Vector2(0.5f, 0.1f),
            TextPanelPosition.BottomRight  => new Vector2(0.9f, 0.1f),
            TextPanelPosition.Custom       => step.CustomTextAnchor,
            _                              => new Vector2(0.5f, 0.1f),
        };

        private void SetOverlayAlpha(float alpha)
        {
            var col = _overlayMaterial.GetColor(OverlayColorId);
            col.a = alpha;
            _overlayMaterial.SetColor(OverlayColorId, col);
        }

        private float GetOverlayAlpha()
        {
            return _overlayMaterial.GetColor(OverlayColorId).a;
        }
    }
}
