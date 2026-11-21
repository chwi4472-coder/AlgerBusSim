using System.Collections.Generic;
using UnityEngine;

namespace AlgerBusSim.Vehicles
{
    public class Powertrain
    {
        public float PowerKw = 184f;
        public float TorqueNm = 1100f;
        public float IdleRpm = 600f;
        public float MaxRpm = 2400f;
        public int Gears = 6;
        public float FinalDrive = 5.13f;
        public float CurrentRpm = 600f;
        public int CurrentGear = 1;

        public float EngineTorque(float throttle01, float load)
        {
            float t = Mathf.Clamp01(throttle01);
            float rpmFactor = Mathf.Clamp01((this.CurrentRpm - this.IdleRpm) / (this.MaxRpm - this.IdleRpm));
            float torque = this.TorqueNm * t * (1.0f - 0.3f * rpmFactor);
            return torque;
        }

        public void UpdateRpm(float wheelRpm)
        {
            this.CurrentRpm = Mathf.Clamp(this.IdleRpm + wheelRpm * this.FinalDrive * 0.5f, this.IdleRpm, this.MaxRpm);
        }
    }
}
