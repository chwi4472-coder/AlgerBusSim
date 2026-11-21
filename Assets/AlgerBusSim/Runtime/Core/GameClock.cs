using System;

namespace AlgerBusSim.Core
{
    public class GameClock
    {
        public float DayLengthSeconds = 1200f;
        public float TimeScale = 1f;
        public float Hour = 7f;
        public bool IsNight;

        public void Tick(float deltaTime)
        {
            this.Hour += (deltaTime * this.TimeScale * 24f) / this.DayLengthSeconds;
            if (this.Hour >= 24f) this.Hour -= 24f;
            this.IsNight = this.Hour < 6f || this.Hour > 19.5f;
        }

        public float SunAngle()
        {
            return (this.Hour / 24f) * 360f;
        }
    }
}
