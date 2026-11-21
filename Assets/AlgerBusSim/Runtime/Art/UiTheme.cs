using UnityEngine;

namespace AlgerBusSim.Art
{
    public static class UiTheme
    {
        public static Color Background = new Color(0.055f, 0.06f, 0.075f);
        public static Color Panel = new Color(0.965f, 0.960f, 0.937f);
        public static Color PanelEdge = new Color(0.227f, 0.259f, 0.314f);
        public static Color Button = new Color(0.173f, 0.196f, 0.271f);
        public static Color ButtonPressed = new Color(0.176f, 0.22f, 0.294f);
        public static Color Accent = new Color(0.125f, 0.353f, 0.627f);
        public static Color AccentTeal = new Color(0f, 0.4f, 0.2f);
        public static Color Ink = new Color(0.118f, 0.141f, 0.173f);
        public static Color Muted = new Color(0.47f, 0.494f, 0.518f);
        public static Color CoinGold = new Color(0.89f, 0.69f, 0.26f);
        public static Color Danger = new Color(0.78f, 0.18f, 0.18f);

        public static Texture2D PanelSprite(string key)
        {
            // 9-slice panel from the UI atlas.
            return null;
        }

        public static Texture2D ButtonSprite(string key)
        {
            var tex = TextureLibrary.Get("ui", QualityTierProvider.Current);
            if (tex == null) return null;
            var rt = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(tex, rt);
            RenderTexture render = rt;
            Texture2D result = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
            RenderTexture.active = render;
            result.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            result.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseRenderTexture(render);
            return result;
        }
    }
}
