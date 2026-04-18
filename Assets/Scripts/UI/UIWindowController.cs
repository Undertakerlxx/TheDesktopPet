using UnityEngine;

namespace DesktopPet.UI
{
    public abstract class UIWindowController : MonoBehaviour
    {
        public UIWindowType windowType;
        public UIWindowLayer windowLayer;

        protected UIManager uiManager;

        public virtual void Initialize(UIManager manager)
        {
            uiManager = manager;

            if (windowLayer == null)
            {
                windowLayer = GetComponent<UIWindowLayer>();
            }
        }

        public virtual void Open()
        {
            if (windowLayer != null)
            {
                windowLayer.Show();
            }
        }

        public virtual void Close()
        {
            if (windowLayer != null)
            {
                windowLayer.Hide();
            }
        }
    }
}
