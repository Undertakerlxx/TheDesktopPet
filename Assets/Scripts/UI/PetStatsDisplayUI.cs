using UnityEngine;
using UnityEngine.UI;

namespace DesktopPet.UI
{
    public class PetStatsDisplayUI : MonoBehaviour
    {
        public ThePet pet;
        public ThePetStatsManager statsManager;
        public RectTransform panel;
        public Text statsText;
        public Canvas targetCanvas;
        public Vector3 worldOffset = new(0f, 1.15f, 0f);
        public Vector2 screenOffset = new(0f, 12f);

        private RectTransform canvasRectTransform;

        private void Awake()
        {
            AutoWireReferences();
        }

        private void LateUpdate()
        {
            AutoWireReferences();
            UpdatePosition();
            UpdateStatsText();
        }

        private void AutoWireReferences()
        {
            if (pet == null)
            {
                pet = FindFirstObjectByType<ThePet>();
            }

            if (statsManager == null && pet != null)
            {
                statsManager = pet.GetComponent<ThePetStatsManager>();
            }

            if (panel == null)
            {
                panel = GetComponent<RectTransform>();
            }

            if (statsText == null)
            {
                statsText = GetComponentInChildren<Text>(true);
            }

            if (targetCanvas == null)
            {
                targetCanvas = GetComponentInParent<Canvas>();
            }

            if (canvasRectTransform == null && targetCanvas != null)
            {
                canvasRectTransform = targetCanvas.GetComponent<RectTransform>();
            }
        }

        private void UpdatePosition()
        {
            if (pet == null || pet.cam == null || panel == null)
            {
                return;
            }

            Vector3 worldPosition = pet.transform.position + worldOffset;
            Vector2 screenPosition = pet.cam.WorldToScreenPoint(worldPosition);
            screenPosition += screenOffset;

            if (targetCanvas == null || targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                panel.position = screenPosition;
                return;
            }

            if (canvasRectTransform == null)
            {
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform,
                screenPosition,
                targetCanvas.worldCamera,
                out Vector2 localPoint);
            panel.anchoredPosition = localPoint;
        }

        private void UpdateStatsText()
        {
            if (statsText == null || statsManager == null)
            {
                return;
            }

            ThePetStats stats = statsManager.current_stats;
            if (stats == null)
            {
                if (statsManager.stats != null && statsManager.stats.Length > 0)
                {
                    statsManager.Change(0);
                    stats = statsManager.current_stats;
                }

                if (stats == null)
                {
                    statsText.text = "属性未配置";
                    return;
                }
            }

            statsText.text =
                $"亲密度: {stats.intimacy:0}\n" +
                $"开心值: {stats.happiness:0}\n" +
                $"活力值: {stats.energy:0}/{stats.energy_max:0}\n" +
                $"专注值: {stats.focus:0}\n" +
                $"饱食度: {stats.satiety:0}";
        }
    }
}
