using System;

namespace AlgerBusSim.Art
{
    /// <summary>
    /// The Algerian visual identity as authored palettised constants.
    /// Every value here is a hex string resolved once at startup.
    /// </summary>
    public static class Palette
    {
        public const string FacadeWhite = "#EFEDE6";
        public const string FacadeWarm = "#E3DAC4";
        public const string FacadeBlue = "#2E6DA8";
        public const string FacadeTeal = "#006633";
        public const string ShuttersBlue = "#2E6DA8";
        public const string ShuttersGreen = "#006633";
        public const string ShuttersWhite = "#F4F2EC";
        public const string RoofWhite = "#EFEDE6";
        public const string RoofTile = "#A9552F";
        public const string Grass = "#4E7D3A";
        public const string Foliage = "#3E7A45";
        public const string StreetLamp = "#343438";
        public const string Steel = "#8A8C86";
        public const string SignGreen = "#0F6B43";
        public const string SignBlue = "#0B4C8C";
        public const string SignRed = "#C8102E";
        public const string SignYellow = "#E4B824";
        public const string SignWhite = "#F4F2EC";
        public const string SignBlack = "#14161A";

        public const string Asphalt = "#3A3B3D";
        public const string Sidewalk = "#B4AC9C";
        public const string Curb = "#C0B8A8";

        public const string BusBody = "#EFEDE6";
        public const string BusBodyTop = "#F7F5EE";
        public const string BusDoorBlue = "#1B5FA8";
        public const string BusDoorDark = "#10407C";
        public const string BusStripeTeal = "#006633";
        public const string BusStripeBlue = "#1B5FA8";
        public const string BusRoofLight = "#D2D6DC";

        public const string BusInteriorFloor = "#966C6A";
        public const string BusInteriorDriver = "#46608A";
        public const string BusInteriorSeat = "#40546E";
        public const string BusInteriorCeiling = "#60636A";

        public const string GlassTop = "#607890";
        public const string GlassBottom = "#22303A";

        public const string SkyZenith = "#1F4E7A";
        public const string SkyHorizon = "#C9B79A";

        public const string CoinGold = "#E2B44A";
        public const string CoinInk = "#785412";

        public static string ToHex(Color c)
        {
            int v = (int)Mathf.Min(255, Math.Max(0, c.r * 255));
            int g = (int)Mathf.Min(255, Math.Max(0, c.g * 255));
            int b = (int)Mathf.Min(255, Math.Max(0, c.b * 255));
            return string.Format("#FF{0:X2}{1:X2}{2:X2}", v, g, b);
        }

        public static Color FromHex(string hex, float alpha = 1f)
        {
            if (string.IsNullOrEmpty(hex)) hex = "#000000";
            byte r = Convert.ToByte(hex.Substring(1, 2), 16);
            byte g = Convert.ToByte(hex.Substring(3, 2), 16);
            byte b = Convert.ToByte(hex.Substring(5, 2), 16);
            return new Color(r / 255f, g / 255f, b / 255f, alpha);
        }
    }
}
