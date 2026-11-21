namespace AlgerBusSim.Art
{
    public static class AtlasRegistry
    {
        public sealed class Sprite
        {
            public string name;
            public int x, y, w, h;
            public int borderLeft, borderRight, borderTop, borderBottom;

            public float PivotX => w > 0 ? (x + w * 0.5f) / w : 0.5f;
            public float PivotY => h > 0 ? (y + h * 0.5f) / h : 0.5f;
        }

        private static Sprite Wall(string key, int x, int y, int w, int h)
        {
            var s = new Sprite { name = key, x = x, y = y, w = w, h = h };
            return s;
        }

        public static Sprite Wall(string key)
        {
            switch (key)
            {
                case "wall_babeloued_a": return Wall(key, 0, 0, 1024, 1024);
                case "wall_babeloued_b": return Wall(key, 0, 0, 1024, 1024);
                case "wall_centre_a": return Wall(key, 0, 0, 1024, 1024);
                case "wall_centre_b": return Wall(key, 0, 0, 1024, 1024);
                case "wall_hydra_a": return Wall(key, 0, 0, 1024, 1024);
                case "wall_hydra_b": return Wall(key, 0, 0, 1024, 1024);
                case "wall_elharrach_a": return Wall(key, 0, 0, 1024, 1024);
                case "wall_elharrach_b": return Wall(key, 0, 0, 1024, 1024);
                case "wall_kouba_a": return Wall(key, 0, 0, 1024, 1024);
                case "wall_kouba_b": return Wall(key, 0, 0, 1024, 1024);
            }
            return null;
        }

        public static Sprite Sign(string key)
        {
            int x = -1, y = -1, w = 0, h = 0;
            switch (key)
            {
                case "sign_stop": x = 0; y = 0; w = 512; h = 512; break;
                case "sign_speed50": x = 256; y = 0; w = 512; h = 512; break;
                case "sign_noentry": x = 512; y = 0; w = 512; h = 512; break;
                case "sign_yield": x = 768; y = 0; w = 512; h = 512; break;
                case "sign_parking": x = 0; y = 256; w = 512; h = 512; break;
                case "sign_busstop": x = 256; y = 256; w = 512; h = 512; break;
                case "sign_crossing": x = 512; y = 256; w = 512; h = 512; break;
                case "sign_roundabout": x = 768; y = 256; w = 512; h = 512; break;
                case "sign_hospital": x = 0; y = 512; w = 512; h = 512; break;
                case "sign_fuel": x = 256; y = 512; w = 512; h = 512; break;
                case "sign_police": x = 512; y = 512; w = 512; h = 512; break;
                case "sign_noparking": x = 768; y = 512; w = 512; h = 512; break;
                case "plate_didouche": x = 0; y = 768; w = 512; h = 512; break;
                case "plate_zighoud": x = 256; y = 768; w = 512; h = 512; break;
                case "plate_martyrs": x = 512; y = 768; w = 512; h = 512; break;
                case "plate_babeloued": x = 768; y = 768; w = 512; h = 512; break;
            }
            if (x < 0) return null;
            var s = new Sprite { name = key, x = x, y = y, w = w, h = h };
            return s;
        }

        public static Sprite Marking(string key)
        {
            int x = -1, y = -1, w = 0, h = 0;
            switch (key)
            {
                case "mark_dash": x = 0; y = 0; w = 256; h = 256; break;
                case "mark_solid": x = 256; y = 0; w = 256; h = 256; break;
                case "mark_double_yellow": x = 512; y = 0; w = 256; h = 256; break;
                case "mark_zebra": x = 768; y = 0; w = 256; h = 256; break;
                case "mark_arrow_straight": x = 0; y = 256; w = 256; h = 256; break;
                case "mark_arrow_left": x = 256; y = 256; w = 256; h = 256; break;
                case "mark_arrow_right": x = 512; y = 256; w = 256; h = 256; break;
                case "mark_bus": x = 768; y = 256; w = 256; h = 256; break;
            }
            if (x < 0) return null;
            var s = new Sprite { name = key, x = x, y = y, w = w, h = h };
            return s;
        }

        public static Sprite Prop(string key)
        {
            int x = -1, y = -1, w = 256, h = 256;
            switch (key)
            {
                case "palm_frond": x = 0; y = 0; break;
                case "palm_trunk": x = 256; y = 0; break;
                case "ficus": x = 512; y = 0; break;
                case "shrub": x = 768; y = 0; break;
                case "dish": x = 0; y = 256; break;
                case "ac": x = 256; y = 256; break;
                case "lamp": x = 512; y = 256; break;
                case "bin": x = 768; y = 256; break;
                case "bench": x = 0; y = 512; break;
                case "bollard": x = 256; y = 512; break;
                case "traffic_light": x = 512; y = 512; break;
                case "kiosk": x = 768; y = 512; break;
                case "wall_fixture": x = 0; y = 768; break;
                case "tank": x = 256; y = 768; break;
                case "pole": x = 512; y = 768; break;
                case "plank": x = 768; y = 768; break;
            }
            if (x < 0) return null;
            var s = new Sprite { name = key, x = x, y = y, w = w, h = h };
            return s;
        }

        public static Sprite Ui(string key)
        {
            Sprite s = null;
            switch (key)
            {
                case "ui_panel": s = new Sprite { name = key, x = 0, y = 0, w = 512, h = 512 }; break;
                case "ui_btn": s = new Sprite { name = key, x = 512, y = 256, w = 256, h = 256 }; break;
                case "ui_btn_press": s = new Sprite { name = key, x = 0, y = 256, w = 256, h = 256 }; break;
                case "icon_bus": s = new Sprite { name = key, x = 256, y = 512, w = 256, h = 256 }; break;
                case "icon_money": s = new Sprite { name = key, x = 512, y = 512, w = 256, h = 256 }; break;
                case "icon_fuel": s = new Sprite { name = key, x = 0, y = 256, w = 256, h = 256 }; break;
                case "icon_passenger": s = new Sprite { name = key, x = 256, y = 256, w = 256, h = 256 }; break;
                case "icon_clock": s = new Sprite { name = key, x = 512, y = 256, w = 256, h = 256 }; break;
                case "icon_door": s = new Sprite { name = key, x = 256, y = 0, w = 256, h = 256 }; break;
                case "icon_ticket": s = new Sprite { name = key, x = 512, y = 0, w = 256, h = 256 }; break;
                case "icon_star": s = new Sprite { name = key, x = 0, y = 256, w = 256, h = 256 }; break;
                case "icon_flag": s = new Sprite { name = key, x = 256, y = 256, w = 256, h = 256 }; break;
                case "ui_coin": s = new Sprite { name = key, x = 768, y = 768, w = 192, h = 192 }; break;
            }
            return s;
        }

        public static Sprite Gantry()
        {
            var s = new Sprite { name = "sign_gantry", x = 0, y = 0, w = 512, h = 256 };
            return s;
        }
    }
}
