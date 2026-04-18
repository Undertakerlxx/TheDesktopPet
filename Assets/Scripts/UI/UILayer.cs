using UnityEngine;

namespace DesktopPet.UI
{
    public abstract class UILayer : MonoBehaviour
    {
        public CanvasGroup canvasGroup;

        public bool IsVisible => gameObject.activeSelf;

        protected virtual void Awake()
        {
            EnsureCanvasGroup();
        }

        public virtual void Show()
        {
            EnsureCanvasGroup();
            gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public virtual void Hide()
        {
            EnsureCanvasGroup();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        private void EnsureCanvasGroup()
        {
            if (canvasGroup != null)
            {
                return;
            }

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
    }
}
