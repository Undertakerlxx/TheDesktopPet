using UnityEngine;

namespace DesktopPet.UI
{
    public abstract class UIPanelController : MonoBehaviour
    {
        protected UIManager uiManager;
        protected UIPanelLayer panelLayer;

        public virtual void Initialize(UIManager manager, UIPanelLayer layer)
        {
            uiManager = manager;
            panelLayer = layer;
        }
    }
}
