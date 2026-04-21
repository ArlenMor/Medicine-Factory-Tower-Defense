using UnityEngine;

namespace _Project.Code.Scripts.UIService
{
    public abstract class BasePanel : MonoBehaviour
    {
        public virtual void Initialize(PanelSettings settings) { }

        public virtual void Close()
        {
            Destroy(gameObject);
        }
    }
}
