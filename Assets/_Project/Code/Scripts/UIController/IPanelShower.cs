using UnityEngine;

namespace _Project.Code.Scripts.UIService
{
    public interface IPanelShower
    {
        BasePanel ShowView(PanelType type, PanelSettings settings = null, Transform parentOverride = null);
        void HideView(PanelType type);
        bool IsShown(PanelType type);
    }
}
