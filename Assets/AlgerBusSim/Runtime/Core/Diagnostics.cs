using UnityEngine;

namespace AlgerBusSim.Core
{
    public static class Diagnostics
    {
        public static int QualityTier = 1; // 0 low, 1 medium, 2 high

        public static void Start()
        {
            int ram = SystemInfo.systemMemorySize;
            string gpu = SystemInfo.graphicsDeviceName;
            if (ram < 3000) QualityTier = 0;
            else if (ram < 6000) QualityTier = 1;
            else QualityTier = 2;
            UnityEngine.Debug.Log("[AlgerBusSim] Diagnostics: RAM=" + ram + "MB GPU=" + gpu + " tier=" + QualityTier);
        }
    }
}
