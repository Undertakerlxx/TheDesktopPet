using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public class FeatureWindowController : UIWindowController
    {
        public Text titleText;
        public Text bodyText;
        public Button closeButton;

        [TextArea]
        public string title;

        [TextArea]
        public string description;

        public override void Initialize(UIManager manager)
        {
            base.Initialize(manager);

            if (titleText != null)
            {
                titleText.text = title;
            }

            if (bodyText != null)
            {
                bodyText.text = description;
            }

            if (closeButton == null)
            {
                return;
            }

            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => uiManager.CloseWindow(windowType));
        }
    }
}
