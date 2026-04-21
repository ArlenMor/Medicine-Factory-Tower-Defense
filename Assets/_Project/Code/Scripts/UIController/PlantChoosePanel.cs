using System.Collections.Generic;
using _Project.Code.Scripts.UI;
using UnityEngine;

namespace _Project.Code.Scripts.UIService
{
    public class PlantChoosePanel: BasePanel
    {
        [SerializeField] private List<PlantChooseView> _plantChooseViews;
        
        public override void Initialize(PanelSettings settings)
        {
            base.Initialize(settings);

            if (settings is not PlantChoosePanelSettings castedSettings)
            {
                Debug.LogError("[PlantChoosePanel] - castedSettings is null");
                return;
            }
            
            foreach (var plantChooseView in _plantChooseViews)
            {
                plantChooseView.Initialize(castedSettings.Callback);
            }
            
            transform.position = castedSettings.Position;
        }
    }
}