using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DesktopPet.MiniGame
{
    public static class MiniGameUiFactory
    {
        private static Font cachedFont;

        public static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject gameObject = new(name, typeof(RectTransform));
            gameObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            rectTransform.localScale = Vector3.one;
            return rectTransform;
        }

        public static Image CreatePanel(string name, Transform parent, Color color)
        {
            RectTransform rectTransform = CreateRect(name, parent);
            Image image = rectTransform.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        public static Text CreateText(string name, Transform parent, int fontSize, TextAnchor alignment, Color color)
        {
            RectTransform rectTransform = CreateRect(name, parent);
            Text text = rectTransform.gameObject.AddComponent<Text>();
            text.font = GetFont();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        public static Button CreateButton(string name, Transform parent, string label, Color backgroundColor, Color textColor)
        {
            Image image = CreatePanel(name, parent, backgroundColor);
            Button button = image.gameObject.AddComponent<Button>();
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            ColorBlock colors = button.colors;
            colors.normalColor = backgroundColor;
            colors.highlightedColor = backgroundColor * 1.05f;
            colors.pressedColor = backgroundColor * 0.92f;
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 0.55f);
            button.colors = colors;
            button.onClick.AddListener(ClearSelection);

            Text labelText = CreateText("Label", image.transform, 24, TextAnchor.MiddleCenter, textColor);
            Stretch(labelText.rectTransform);
            labelText.text = label;
            return button;
        }

        public static void StyleSymbolText(Text text, int fontSize, Color textColor, Color outlineColor)
        {
            if (text == null)
            {
                return;
            }

            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = textColor;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            Outline outline = text.GetComponent<Outline>();
            if (outline == null)
            {
                outline = text.gameObject.AddComponent<Outline>();
            }

            outline.effectColor = outlineColor;
            outline.effectDistance = new Vector2(1.5f, -1.5f);
        }

        public static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        public static void SetAnchors(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }

        public static void SetHeight(LayoutElement element, float preferredHeight)
        {
            element.minHeight = preferredHeight;
            element.preferredHeight = preferredHeight;
        }

        private static Font GetFont()
        {
            if (cachedFont == null)
            {
                cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (cachedFont == null)
                {
                    cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
            }

            return cachedFont;
        }

        private static void ClearSelection()
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }
}
