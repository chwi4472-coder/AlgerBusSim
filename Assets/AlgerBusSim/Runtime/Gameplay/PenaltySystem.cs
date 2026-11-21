namespace AlgerBusSim.Gameplay
{
    public class PenaltySystem
    {
        public int RedLightDzd = 4000;
        public int SpeedingDzd = 2000;
        public int MissedStopDzd = 1500;

        public long RedLight() { return this.RedLightDzd; }
        public long Speeding() { return this.SpeedingDzd; }
        public long MissedStop() { return this.MissedStopDzd; }
    }
}
