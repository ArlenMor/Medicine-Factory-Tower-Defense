using _Project.Code.Scripts.Audio;
using _Project.Code.Scripts.BattleField;
using _Project.Code.Scripts.Data;
using _Project.Code.Scripts.Game;
using _Project.Code.Scripts.InputResolverService;
using _Project.Code.Scripts.ServiceLocator;
using _Project.Code.Scripts.Stats;
using _Project.Code.Scripts.Tutorial;
using UnityEngine;

namespace _Project.Code.Scripts.BattleField
{
    public class DefenseDragController : MonoBehaviour, IDefenseDragController
    {
        [SerializeField] private Camera _camera;

        private IFieldSystem _fieldSystem;
        private DefenseShopConfig _shopConfig;
        private IInputResolver _inputResolver;
        private IManualUpdateRegistrar _manualUpdateRegistrar;

        private GameObject _dragInstance;
        private IFieldPlaceable _dragPlaceable;
        private DefenseItemData _dragItemData;
        private bool _isDragging;
        private int _snapX;
        private int _snapY;
        private bool _isSnapped;

        public void Initialize(DefenseShopConfig shopConfig, IManualUpdateRegistrar gameController)
        {
            _inputResolver = S.Get<IInputResolver>();
            _fieldSystem = S.Get<IFieldSystem>();
            _shopConfig = shopConfig;
            _manualUpdateRegistrar = gameController;

            _inputResolver.OnPointerDown += OnPointerDown;
            _inputResolver.OnPointerHeld += OnPointerHeld;
            _inputResolver.OnPointerUp += OnPointerUp;
        }

        private void OnDestroy()
        {
            if (_inputResolver == null) return;
            _inputResolver.OnPointerDown -= OnPointerDown;
            _inputResolver.OnPointerHeld -= OnPointerHeld;
            _inputResolver.OnPointerUp -= OnPointerUp;
        }

        private void OnPointerDown(InputEventData data)
        {
            if (data.Button != MouseButton.Left) return;
            if (!data.IsCanvasHit) return;
            if (data.HitObject == null) return;

            if (S.TryGet<ITutorialService>(out var tutorialCheck) && tutorialCheck.IsBuildingDisabled) return;

            var button = data.HitObject.GetComponentInParent<DefenseBuyButtonView>();
            if (button == null) return;

            int credits = GameData.Instance.Resources[ResourceType.Credit];
            
            if (credits < button.Price) 
            {
                AudioManager.Instance.PlayWrongNotify();
                return;
            }

            DefenseItemData? found = null;
            foreach (var item in _shopConfig.Items)
            {
                if (item.Type == button.DefenseType)
                {
                    found = item;
                    break;
                }
            }

            if (!found.HasValue) return;

            _dragItemData = found.Value;
            _fieldSystem.ShowHighlight();
            Vector3 worldPos = ScreenToWorld(data.ScreenPosition);
            _dragInstance = Instantiate(_dragItemData.Prefab, worldPos, Quaternion.identity);
            _dragPlaceable = _dragInstance.GetComponent<IFieldPlaceable>();

            if (_dragInstance.TryGetComponent(out Turret dragTurret))
            {
                dragTurret.Initialize(_dragItemData);
                dragTurret.LockRadius(true);
            }
            else if (_dragInstance.TryGetComponent(out Barricade dragBarricade))
            {
                dragBarricade.Initialize(_dragItemData);
            }

            SetCollidersEnabled(_dragInstance, false);
            _isDragging = true;
            _isSnapped = false;
        }

        private void OnPointerHeld(InputEventData data)
        {
            if (!_isDragging) return;
            if (data.Button != MouseButton.Left) return;

            Vector3 worldPos = ScreenToWorld(data.ScreenPosition);

            if (_fieldSystem.WorldToGrid(worldPos, out int x, out int y)
                && _fieldSystem.CanPlaceMulti(x, y, _dragItemData.SafeWidth, _dragItemData.SafeHeight))
            {
                _dragInstance.transform.position = _fieldSystem.GetMultiCellWorldPosition(
                    x, y, _dragItemData.SafeWidth, _dragItemData.SafeHeight, _dragItemData.PivotOffset);
                _snapX = x;
                _snapY = y;
                _isSnapped = true;
            }
            else
            {
                _dragInstance.transform.position = worldPos;
                _isSnapped = false;
            }
        }

        private void OnPointerUp(InputEventData data)
        {
            if (!_isDragging) return;
            if (data.Button != MouseButton.Left) return;

            if (_isSnapped && _fieldSystem.TryPlaceMulti(
                    _snapX, _snapY,
                    _dragItemData.SafeWidth, _dragItemData.SafeHeight,
                    _dragPlaceable, _dragItemData.PivotOffset))
            {
                SetCollidersEnabled(_dragInstance, true);
                if (_dragInstance.TryGetComponent(out Turret placedTurret))
                {
                    placedTurret.Place();
                    _manualUpdateRegistrar.Register(placedTurret);
                    GameData.Instance.Stats.TurretsBuilt++;
                }
                else if (_dragInstance.TryGetComponent(out Barricade _))
                {
                    GameData.Instance.Stats.BarricadesBuilt++;
                }
                GameData.Instance.AddResource(ResourceType.Credit, -_dragItemData.CreditCost);
                AudioManager.Instance.PlayBuilding();
                if (S.TryGet<GameplayLogger>(out var dLog))
                    dLog.LogPurchase(_dragItemData.Type == DefenseType.Turret ? "Turret" : "Barricade", _dragItemData.CreditCost);
                if (S.TryGet<ITutorialService>(out var tutorial))
                    tutorial.NotifyEvent(TutorialEventType.BuildingPlaced);
            }
            else
            {
                Destroy(_dragInstance);
            }

            _fieldSystem.HideHighlight();
            _dragInstance = null;
            _dragPlaceable = null;
            _isDragging = false;
            _isSnapped = false;
        }

        private static void SetCollidersEnabled(GameObject go, bool enabled)
        {
            foreach (var col in go.GetComponentsInChildren<Collider2D>(true))
                col.enabled = enabled;
            foreach (var col in go.GetComponentsInChildren<Collider>(true))
                col.enabled = enabled;
        }

        private Vector3 ScreenToWorld(Vector2 screenPos)
        {
            Camera cam = _camera ? _camera : Camera.main;
            Vector3 point = new Vector3(screenPos.x, screenPos.y, Mathf.Abs(cam.transform.position.z));
            Vector3 world = cam.ScreenToWorldPoint(point);
            world.z = 0f;
            return world;
        }
    }
}

public interface IDefenseDragController
{
    void Initialize(DefenseShopConfig shopConfig, IManualUpdateRegistrar gameController);
}