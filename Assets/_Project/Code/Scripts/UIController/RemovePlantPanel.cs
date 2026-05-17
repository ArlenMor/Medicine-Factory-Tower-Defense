using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.Scripts.UIService
{
    public class RemovePlantPanel : BasePanel
    {
        [SerializeField] private Button _removeButton;

        public override void Initialize(PanelSettings settings)
        {
            base.Initialize(settings);

            if (settings is not RemovePlantPanelSettings castedSettings)
            {
                Debug.LogError("[RemovePlantPanel] - castedSettings is null");
                return;
            }

            _removeButton.onClick.RemoveAllListeners();
            _removeButton.onClick.AddListener(() => castedSettings.Callback?.Invoke());

            transform.position = castedSettings.Position;
        }
    }
}
