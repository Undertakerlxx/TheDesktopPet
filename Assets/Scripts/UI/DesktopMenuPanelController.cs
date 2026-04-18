using UnityEngine.UI;

namespace DesktopPet.UI
{
    public class DesktopMenuPanelController : UIPanelController
    {
        public Button skinButton;
        public Button timerButton;
        public Button miniGameButton;
        public Button farmButton;
        public Button kitchenButton;
        public Button achievementButton;

        public override void Initialize(UIManager manager, UIPanelLayer layer)
        {
            base.Initialize(manager, layer);
            BindButton(skinButton, UIWindowType.Skin);
            BindButton(timerButton, UIWindowType.Timer);
            BindButton(miniGameButton, UIWindowType.MiniGame);
            BindButton(farmButton, UIWindowType.Farm);
            BindButton(kitchenButton, UIWindowType.Kitchen);
            BindButton(achievementButton, UIWindowType.Achievement);
        }

        private void BindButton(Button button, UIWindowType windowType)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => uiManager.OpenWindow(windowType));
        }
    }
}
