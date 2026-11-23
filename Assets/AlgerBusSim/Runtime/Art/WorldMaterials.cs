using UnityEngine;

namespace AlgerBusSim.Art
{
    /// <summary>
    /// Every material in the city, created once from the authored textures.
    /// Standard shader is used for lit geometry (sun, shadows, fog); the
    /// custom unlit shaders are only for decals and signage.
    /// </summary>
    public static class WorldMaterials
    {
        public static Material Road;
        public static Material Sidewalk;
        public static Material Kerb;
        public static Material Marking;
        public static Material MarkingYellow;
        public static Material Sand;
        public static Material Water;
        public static Material RoofFlat;
        public static Material RoofTile;
        public static Material Trim;
        public static Material Glass;
        public static Material Metal;
        public static Material Dark;
        public static Material White;
        public static Material Concrete;
        public static Material LampOff;
        public static Material LampOn;
        public static Material Headlight;
        public static Material Taillight;
        public static Material SignalRed;
        public static Material SignalYellow;
        public static Material SignalGreen;
        public static Material SignalOff;
        public static Material DecalBus;
        public static Material DecalPlate;
        public static Material Destination;
        public static Material StopPanel;
        public static Material Signs;
        public static Material Flag;
        public static Material PlaqueGrandePoste;
        public static Material Foliage;
        public static Material Trunk;
        public static Material Wood;
        public static Material Terracotta;
        public static Material AwningRed;
        public static Material AwningGreen;
        public static Material AwningBlue;
        public static Material AwningYellow;

        private static Material[] _facades;
        private static Material[] _shops;
        private static bool _ready;

        public static Material Facade(int district, int variant)
        {
            int d = Mathf.Clamp(district, 0, 6);
            int v = Mathf.Clamp(variant, 0, 1);
            return _facades[d * 2 + v];
        }

        public static Material Shop(int district)
        {
            return _shops[Mathf.Clamp(district, 0, 6)];
        }

        public static void Init()
        {
            if (_ready) return;
            _ready = true;

            Texture2D asphalt = Tex("ground_asphalt");
            Texture2D sidewalk = Tex("ground_sidewalk");
            Texture2D kerb = Tex("ground_kerb");
            Texture2D sand = Tex("ground_sand");
            Texture2D water = Tex("water");
            Texture2D roof = Tex("roof_flat");
            Texture2D roofTile = Tex("roof_tile");

            Road = Lit(asphalt, Color.white, 0.12f, 0f);
            Road.mainTextureScale = Vector2.one;
            Sidewalk = Lit(sidewalk, Color.white, 0.08f, 0f);
            Kerb = Lit(kerb, Color.white, 0.08f, 0f);
            Marking = Lit(null, new Color(0.93f, 0.93f, 0.88f), 0.05f, 0f);
            MarkingYellow = Lit(null, new Color(0.89f, 0.72f, 0.14f), 0.05f, 0f);
            Sand = Lit(sand, Color.white, 0.05f, 0f);
            Water = Lit(water, Color.white, 0.86f, 0.15f);
            RoofFlat = Lit(roof, Color.white, 0.10f, 0f);
            RoofTile = Lit(roofTile, Color.white, 0.08f, 0f);

            Trim = Solid(Palette.FromHex(Palette.FacadeWhite), 0.15f);
            Glass = Lit(null, Palette.FromHex(Palette.GlassBottom), 0.78f, 0.35f);
            Metal = Solid(Palette.FromHex(Palette.Steel), 0.55f, 0.4f);
            Dark = Solid(new Color(0.10f, 0.11f, 0.12f), 0.2f);
            White = Solid(new Color(0.95f, 0.94f, 0.90f), 0.1f);
            Concrete = Solid(new Color(0.66f, 0.64f, 0.60f), 0.1f);
            LampOff = Solid(new Color(0.22f, 0.22f, 0.24f), 0.3f);
            LampOn = Emissive(new Color(0.22f, 0.22f, 0.24f), new Color(1f, 0.82f, 0.45f) * 2.2f);
            Headlight = Emissive(new Color(0.9f, 0.9f, 0.85f), new Color(1f, 0.95f, 0.8f) * 2.5f);
            Taillight = Emissive(new Color(0.5f, 0.05f, 0.05f), new Color(0.9f, 0.08f, 0.05f) * 1.8f);
            SignalRed = Emissive(new Color(0.3f, 0.05f, 0.05f), new Color(1f, 0.08f, 0.05f) * 2.4f);
            SignalYellow = Emissive(new Color(0.3f, 0.25f, 0.05f), new Color(1f, 0.7f, 0.08f) * 2.4f);
            SignalGreen = Emissive(new Color(0.05f, 0.25f, 0.1f), new Color(0.1f, 1f, 0.35f) * 2.4f);
            SignalOff = Solid(new Color(0.09f, 0.09f, 0.10f), 0.2f);

            DecalBus = Transparent(Tex("decal_algerbus"));
            DecalPlate = Transparent(Tex("bus_plate_dz"));
            Destination = Transparent(Tex("bus_dest_m0"));
            StopPanel = Transparent(Tex("stop_panel"));
            Signs = Transparent(Tex("signs"));
            Flag = Transparent(Tex("flag_dz"));
            PlaqueGrandePoste = Transparent(Tex("plaque_grande_poste"));

            Foliage = Solid(new Color(0.20f, 0.42f, 0.20f), 0.05f);
            Trunk = Solid(new Color(0.38f, 0.28f, 0.18f), 0.05f);
            Wood = Solid(new Color(0.52f, 0.36f, 0.20f), 0.08f);
            Terracotta = Solid(new Color(0.66f, 0.33f, 0.18f), 0.06f);
            AwningRed = Solid(new Color(0.72f, 0.12f, 0.14f), 0.06f);
            AwningGreen = Solid(new Color(0.05f, 0.40f, 0.20f), 0.06f);
            AwningBlue = Solid(new Color(0.09f, 0.28f, 0.55f), 0.06f);
            AwningYellow = Solid(new Color(0.85f, 0.62f, 0.10f), 0.06f);

            _facades = new Material[14];
            _shops = new Material[7];
            for (int d = 0; d < 7; d++)
            {
                string[] variants = FacadeKeys(d);
                _facades[d * 2 + 0] = Lit(Tex(variants[0]), Color.white, 0.08f, 0f);
                _facades[d * 2 + 1] = Lit(Tex(variants[1]), Color.white, 0.08f, 0f);
                _shops[d] = Lit(Tex(ShopKey(d)), Color.white, 0.10f, 0f);
            }

            Debug.Log("[AlgerBusSim] WorldMaterials ready.");
        }

        private static string[] FacadeKeys(int district)
        {
            switch (district)
            {
                case 0: return new[] { "wall_babeloued_a", "wall_babeloued_b" };
                case 1: return new[] { "wall_babeloued_b", "wall_centre_b" };
                case 2: return new[] { "wall_centre_a", "wall_centre_b" };
                case 3: return new[] { "wall_elharrach_a", "wall_elharrach_b" };
                case 4: return new[] { "wall_elharrach_b", "wall_kouba_b" };
                case 5: return new[] { "wall_hydra_a", "wall_hydra_b" };
                default: return new[] { "wall_kouba_a", "wall_kouba_b" };
            }
        }

        private static string ShopKey(int district)
        {
            switch (district)
            {
                case 0: return "shop_babeloued";
                case 1: return "shop_babeloued";
                case 2: return "shop_centre";
                case 3: return "shop_elharrach";
                case 4: return "shop_elharrach";
                case 5: return "shop_hydra";
                default: return "shop_kouba";
            }
        }

        private static Texture2D Tex(string key)
        {
            return TextureLibrary.Get(key, QualityTierProvider.Current);
        }

        public static Material Lit(Texture2D tex, Color tint, float gloss, float metallic)
        {
            Shader shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("AlgerBusSim/Unlit/Tilable");
            if (shader == null) shader = Shader.Find("Unlit/Texture");
            var mat = new Material(shader);
            if (tex != null) mat.mainTexture = tex;
            mat.color = tint;
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", gloss);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            return mat;
        }

        public static Material Solid(Color color, float gloss = 0.1f, float metallic = 0f)
        {
            return Lit(null, color, gloss, metallic);
        }

        public static Material Emissive(Color color, Color emission)
        {
            var mat = Solid(color, 0.2f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emission);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            return mat;
        }

        public static Material Transparent(Texture2D tex)
        {
            Shader shader = Shader.Find("AlgerBusSim/Unlit/Transparent"); if (shader == null) shader = Shader.Find("Unlit/Transparent");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            var mat = new Material(shader);
            mat.mainTexture = tex;
            mat.color = Color.white;
            mat.renderQueue = 3000;
            return mat;
        }
    }
}
