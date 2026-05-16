using UnityEngine;
using UnityEngine.UI;
using DesktopPet.MiniGame;

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

        private MiniGameWindowContentController runtimeContent;

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

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(() => uiManager.CloseWindow(windowType));
            }

            runtimeContent = GetComponent<MiniGameWindowContentController>();
            if (runtimeContent != null)
            {
                runtimeContent.InitializeContent(this, manager);
            }
        }

        public override void Open()
        {
            base.Open();
            runtimeContent?.HandleWindowOpened();
        }

        public override void Close()
        {
            runtimeContent?.HandleWindowClosed();
            base.Close();
        }
    }
}
