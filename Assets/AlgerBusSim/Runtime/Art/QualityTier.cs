namespace AlgerBusSim.Art
{
    public enum QualityTier
    {
        Low = 0,
        Medium = 1,
        High = 2,
    }

    public static class QualityTierProvider
    {
        private static QualityTier _current = QualityTier.Medium;
        private static readonly string[] OverrideKeys = { "quality_low", "quality_medium", "quality_high" };

        public static QualityTier Current
        {
            get { return _current; }
            set { _current = value; }
        }

        public static QualityTier Detect()
        {
            // Full device-tier detection lands in Phase 6 (Android runtime probe).
            // Until then the Medium tier is the design default.
            return QualityTier.Medium;
        }
    }
}
