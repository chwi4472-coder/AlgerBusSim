using System.Collections.Generic;
using UnityEngine;
using AlgerBusSim.Player;
using AlgerBusSim.Art;

namespace AlgerBusSim.Vehicles
{
    public class BusController : MonoBehaviour
    {
        public WheelCollider FrontLeft;
        public WheelCollider FrontRight;
        public WheelCollider RearLeft;
        public WheelCollider RearRight;

        public Powertrain Powertrain = new Powertrain();
        public FuelTank FuelTank = new FuelTank();
        public DamageModel Damage = new DamageModel();
        public DoorSystem DoorSystem = new DoorSystem();

        public float MaxSteerAngle = 42f;
        public float MaxBrakeTorque = 6000f;
        public float HandbrakeTorque = 8000f;

        public List<Light> HeadlightSpots = new List<Light>();

        private Rigidbody _rb;
        private float _speedMs;
        private float _distanceAccum;
        private Vector3 _prevPos;
        private bool _headlightsOn = false;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _prevPos = transform.position;
        }

        void Update()
        {
            InputRouter.Tick(Time.deltaTime);
            bool doorsPressed = InputRouter.DoorsPressed;
            bool headlightsPressed = InputRouter.HeadlightsPressed;
            bool gearUp = InputRouter.GearUpPressed;
            bool gearDown = InputRouter.GearDownPressed;

            DoorSystem.Update(Time.deltaTime);

            // door interlock
            DoorSystem.Interlock = _speedMs > 0.8f;

            if (doorsPressed) DoorSystem.Toggle();
            if (headlightsPressed) _headlightsOn = !_headlightsOn;
            if (InputRouter.CameraPressed) { /* handled by GameSession */ }
            if (gearUp) Powertrain.GearIndex = Mathf.Min(Powertrain.GearRatios.Length - 1, Powertrain.GearIndex + 1);
            if (gearDown) Powertrain.GearIndex = Mathf.Max(0, Powertrain.GearIndex - 1);

            Powertrain.Step(Time.deltaTime,
                WheelRpmAverage(),
                InputRouter.Throttle,
                InputRouter.Brake > 0.5f,
                InputRouter.GearUpPressed,
                InputRouter.GearDownPressed,
                InputRouter.DriveModePressed,
                InputRouter.AutoManualPressed);

            // Horn
            // (audio handled externally; flag available via InputRouter.HornHeld)

            // Headlights
            foreach (var l in HeadlightSpots) if (l != null) l.enabled = _headlightsOn || (UnityEngine.RenderSettings.ambientIntensity < 0.5f);
            // emissive headlight material toggle not needed (always emissive)

            InputRouter.ResetFrame();

            _distanceAccum += Vector3.Distance(transform.position, _prevPos);
            _prevPos = transform.position;
        }

        void FixedUpdate()
        {
            _speedMs = _rb != null ? Vector3.Dot(_rb.linearVelocity, transform.forward) : 0f;

            float speedKmh = Mathf.Abs(_speedMs) * 3.6f;
            float steerAngle = InputRouter.Steer * MaxSteerAngle * Mathf.Clamp01(1f - speedKmh / 120f);
            steerAngle = Mathf.Clamp(steerAngle, -MaxSteerAngle, MaxSteerAngle);
            if (FrontLeft != null) FrontLeft.steerAngle = steerAngle;
            if (FrontRight != null) FrontRight.steerAngle = steerAngle;

            // fuel gate
            bool hasFuel = FuelTank.FuelL > 0.1f;
            float torque = hasFuel ? PowertrainTorque() : 0f;

            // door interlock: no throttle if doors open
            if (!DoorSystem.IsClosed()) InputRouter.Throttle = 0f;

            float motor = torque;
            if (RearLeft != null) RearLeft.motorTorque = motor * 0.5f;
            if (RearRight != null) RearRight.motorTorque = motor * 0.5f;

            // braking
            float brake = InputRouter.Brake * MaxBrakeTorque;
            if (InputRouter.Handbrake) brake = HandbrakeTorque;
            if (FrontLeft != null) FrontLeft.brakeTorque = brake * 0.4f;
            if (FrontRight != null) FrontRight.brakeTorque = brake * 0.4f;
            if (RearLeft != null) RearLeft.brakeTorque = brake * 0.6f;
            if (RearRight != null) RearRight.brakeTorque = brake * 0.6f;

            // engine braking when no throttle
            if (InputRouter.Throttle < 0.05f && Mathf.Abs(_speedMs) > 1f)
            {
                float eb = 800f;
                if (RearLeft != null) RearLeft.brakeTorque += eb;
                if (RearRight != null) RearRight.brakeTorque += eb;
            }

            FuelTank.Burn(Mathf.Abs(_speedMs) * Time.fixedDeltaTime, Powertrain.EngineLoad);

            // update rpm from wheels (already in Step via WheelRpmAverage, but Step called with 0 dt above; call a light update)
            Powertrain.Rpm = Mathf.Clamp(Powertrain.Rpm, Powertrain.IdleRpm, Powertrain.MaxRpm);
        }

        private float PowertrainTorque()
        {
            // Recompute torque using current throttle without advancing rpm again
            float rpmNorm = (Powertrain.Rpm - Powertrain.IdleRpm) / (Powertrain.MaxRpm - Powertrain.IdleRpm);
            float curve = 1f - 0.35f * Mathf.Pow((rpmNorm - 0.45f) / 0.55f, 2f);
            curve = Mathf.Clamp(curve, 0.4f, 1.05f);
            float thr = InputRouter.Throttle;
            if (InputRouter.Brake > 0.5f) thr = 0f;
            if (!DoorSystem.IsClosed()) thr = 0f;
            float t = thr * curve * (1f - Damage.Damage01 * 0.5f);
            Powertrain.EngineLoad = t;
            float ratio = Powertrain.Mode == DriveMode.Reverse ? 3.4f
                : Powertrain.GearRatios[Mathf.Clamp(Powertrain.GearIndex, 0, Powertrain.GearRatios.Length - 1)];
            if (Powertrain.Mode == DriveMode.Neutral) return 0f;
            float gearRatio = Powertrain.Mode == DriveMode.Reverse ? 3.4f
                : Powertrain.GearRatios[Mathf.Clamp(Powertrain.GearIndex, 0, Powertrain.GearRatios.Length - 1)];
            float wheelTorque = Powertrain.TorqueNm * t * gearRatio * Powertrain.FinalDrive;
            return wheelTorque * (Powertrain.Mode == DriveMode.Reverse ? 0.6f : 1f);
        }

        private float WheelRpmAverage()
        {
            float sum = 0f; int n = 0;
            if (RearLeft != null) { sum += RearLeft.rpm; n++; }
            if (RearRight != null) { sum += RearRight.rpm; n++; }
            if (n == 0) return 0f;
            return sum / n;
        }

        public float SpeedKmh() { return _speedMs * 3.6f; }
        public float SpeedMs() { return _speedMs; }

        void OnCollisionEnter(Collision c)
        {
            if (c.relativeVelocity.magnitude > 6f)
            {
                float dmg = (c.relativeVelocity.magnitude - 6f) * 0.02f;
                Damage.Add(dmg);
            }
        }

        public float DistanceSinceStart() { return _distanceAccum; }
    }
}
