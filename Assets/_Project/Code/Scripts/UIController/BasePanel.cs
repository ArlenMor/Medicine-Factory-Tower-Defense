using System;
using UnityEngine;

namespace _Project.Code.Scripts.UIService
{
    public abstract class BasePanel : MonoBehaviour
    {
        public virtual void Initialize(PanelSettings settings) { }
        public Action OnClose { get; set; }

        public virtual void Close()
        {
            OnClose?.Invoke();
            Destroy(gameObject);
        }
    }
}
