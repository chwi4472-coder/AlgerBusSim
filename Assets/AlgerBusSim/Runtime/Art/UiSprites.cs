using UnityEngine;

namespace AlgerBusSim.Art
{
    /// <summary>UI sprites sliced from the authored texture sheet.</summary>
    public static class UiSprites
    {
        public static Sprite Round;
        public static Sprite Logo;
        public static Sprite StopPanel;
        public static Sprite[] Icons;
        public static Sprite White;
        private static bool _ready;

        public static void Init()
        {
            if (_ready) return;
            _ready = true;

            Texture2D round = TextureLibrary.Get("ui_round", QualityTierProvider.Current);
            Round = Sprite.Create(round, new Rect(0, 0, round.width, round.height),
                new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
                new Vector4(30, 30, 30, 30));

            Texture2D logo = TextureLibrary.Get("logo_algerbus", QualityTierProvider.Current);
            Logo = Sprite.Create(logo, new Rect(0, 0, logo.width, logo.height),
                new Vector2(0.5f, 0.5f), 100f);

            Texture2D stop = TextureLibrary.Get("stop_panel", QualityTierProvider.Current);
            StopPanel = Sprite.Create(stop, new Rect(0, 0, stop.width, stop.height),
                new Vector2(0.5f, 0.5f), 100f);

            Texture2D icons = TextureLibrary.Get("ui_icons", QualityTierProvider.Current);
            Icons = new Sprite[8];
            for (int i = 0; i < 8; i++)
            {
                Icons[i] = Sprite.Create(icons, new Rect(i * 128, 0, 128, 128),
                    new Vector2(0.5f, 0.5f), 100f);
            }

            var white = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                    white.SetPixel(x, y, Color.white);
            white.Apply();
            White = Sprite.Create(white, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
