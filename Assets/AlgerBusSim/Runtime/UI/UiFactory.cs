using UnityEngine;
using UnityEngine.UI;
using AlgerBusSim.Art;

namespace AlgerBusSim.UI
{
    public static class UiFactory
    {
        public static Font FontRegular;
        public static Font FontBold;
        public static Font FontArabic;

        public static void LoadFonts()
        {
            if (FontRegular == null) FontRegular = Resources.Load<Font>("Fonts/BarlowSemiCondensed-Regular");
            if (FontBold == null) FontBold = Resources.Load<Font>("Fonts/BarlowSemiCondensed-Bold");
            if (FontArabic == null) FontArabic = Resources.Load<Font>("Fonts/NotoNaskhArabic");
            if (FontRegular == null) FontRegular = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (FontBold == null) FontBold = FontRegular;
            if (FontArabic == null) FontArabic = FontRegular;
        }

        public static Font FontForLanguage(string lang)
        {
            LoadFonts();
            if (lang == "ar" || lang == "dz") return FontArabic;
            return FontBold;
        }

        public static GameObject Panel(Transform parent, string name, Color color, Vector2 size, Vector2 pos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            img.sprite = UiSprites.Round;
            img.type = UnityEngine.UI.Image.Type.Sliced;
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return go;
        }

        public static Text Label(Transform parent, string name, string text, int size, Color color, TextAnchor align, Font font = null)
        {
            LoadFonts();
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.font = font != null ? font : FontRegular;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        public static Button Button(Transform parent, string name, string text, Vector2 size, Vector2 pos, Color bg, Color fg, int fontSize, System.Action onClick)
        {
            LoadFonts();
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = bg;
            img.sprite = UiSprites.Round;
            img.type = UnityEngine.UI.Image.Type.Sliced;
            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = bg * 1.15f;
            colors.pressedColor = bg * 0.85f;
            btn.colors = colors;
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            var label = labelGo.AddComponent<Text>();
            label.text = text;
            label.fontSize = fontSize;
            label.color = fg;
            label.alignment = TextAnchor.MiddleCenter;
            label.font = FontBold;
            label.raycastTarget = false;
            var lrt = labelGo.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
            return btn;
        }

        public static Image Image(Transform parent, string name, Sprite sprite, Vector2 size, Vector2 pos, Color tint)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.color = tint;
            img.raycastTarget = false;
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return img;
        }
    }
}
