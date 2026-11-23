using UnityEngine;

namespace AlgerBusSim.Vehicles
{
    public enum DriveMode { Drive, Neutral, Reverse }

    [System.Serializable]
    public class Powertrain
    {
        public float TorqueNm = 1500f;
        public float IdleRpm = 650f;
        public float MaxRpm = 2500f;
        public float[] GearRatios = { 3.4f, 2.0f, 1.45f, 1.05f, 0.78f, 0.62f };
        public float FinalDrive = 4.8f;
        public float WheelRadius = 0.52f;
        public float ShiftCooldown = 0.7f;
        public float AutoUpshiftRpm = 2150f;
        public float AutoDownshiftRpm = 950f;

        public DriveMode Mode = DriveMode.Drive;
        public bool Automatic = true;
        public int GearIndex = 0; // 0-based into GearRatios (Drive only)
        public float Rpm;
        public float EngineLoad;
        public bool EngineOn = true;

        private float _shiftTimer;
        private float _throttlePrev;

        public int DisplayGear
        {
            get
            {
                if (Mode == DriveMode.Reverse) return -1;
                if (Mode == DriveMode.Neutral) return 0;
                return GearIndex + 1;
            }
        }

        public string GearLabel
        {
            get
            {
                if (Mode == DriveMode.Reverse) return "R";
                if (Mode == DriveMode.Neutral) return "N";
                return (GearIndex + 1).ToString();
            }
        }

        public float Step(float dt, float wheelRpmAbs, float throttle01, bool brake, bool shiftUp, bool shiftDown, bool driveModePressed, bool autoManualPressed)
        {
            if (autoManualPressed) Automatic = !Automatic;

            if (driveModePressed)
            {
                if (Mode == DriveMode.Drive) Mode = DriveMode.Neutral;
                else if (Mode == DriveMode.Neutral) Mode = DriveMode.Reverse;
                else Mode = DriveMode.Drive;
                if (Mode == DriveMode.Drive && GearIndex < 0) GearIndex = 0;
            }

            if (shiftUp && !Automatic && Mode == DriveMode.Drive)
            {
                if (GearIndex < GearRatios.Length - 1) { GearIndex++; _shiftTimer = ShiftCooldown; }
            }
            if (shiftDown && !Automatic && Mode == DriveMode.Drive)
            {
                if (GearIndex > 0) { GearIndex--; _shiftTimer = ShiftCooldown; }
                else { Mode = DriveMode.Neutral; }
            }

            if (Mode == DriveMode.Neutral || !EngineOn)
            {
                Rpm = Mathf.MoveTowards(Rpm, IdleRpm, dt * 800f);
                return 0f;
            }

            float ratio = Mode == DriveMode.Reverse ? 3.4f : GearRatios[Mathf.Clamp(GearIndex, 0, GearRatios.Length - 1)];
            float targetRpm = Mathf.Abs(wheelRpmAbs) * FinalDrive * ratio;
            targetRpm = Mathf.Clamp(targetRpm, IdleRpm, MaxRpm);
            Rpm = Mathf.MoveTowards(Rpm, targetRpm, dt * 1200f);

            // auto shifting (drive only)
            if (Automatic && Mode == DriveMode.Drive && _shiftTimer <= 0f)
            {
                if (Rpm > AutoUpshiftRpm && GearIndex < GearRatios.Length - 1)
                {
                    GearIndex++; _shiftTimer = ShiftCooldown;
                }
                else if (Rpm < AutoDownshiftRpm && GearIndex > 0)
                {
                    GearIndex--; _shiftTimer = ShiftCooldown;
                }
            }
            if (_shiftTimer > 0f) _shiftTimer -= dt;

            // torque
            float rpmNorm = (Rpm - IdleRpm) / (MaxRpm - IdleRpm);
            float curve = 1f - 0.35f * Mathf.Pow((rpmNorm - 0.45f) / 0.55f, 2f);
            curve = Mathf.Clamp(curve, 0.4f, 1.05f);
            float thr = Mathf.Clamp01(throttle01);
            float t = thr * curve;
            if (brake) t *= 0.0f;
            float torque = TorqueNm * t * (Mode == DriveMode.Reverse ? 0.6f : 1f);
            EngineLoad = t;
            return torque;
        }
    }
}
