using UnityEngine;

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
        public DoorSystem Doors = new DoorSystem();

        public float MaxSteerAngle = 35f;
        public float MaxMotorTorque = 3000f;
        public float MaxBrakeTorque = 4000f;

        private float _throttle;
        private float _steer;
        private bool _brake;

        void Update()
        {
            _throttle = Input.GetAxis("Vertical");
            _steer = Input.GetAxis("Horizontal");
            _brake = Input.GetKey(KeyCode.Space);
            if (Input.GetKeyDown(KeyCode.O)) this.Doors.Toggle();
            this.Doors.Update(Time.deltaTime);
        }

        void FixedUpdate()
        {
            float steer = _steer * this.MaxSteerAngle;
            if (this.FrontLeft != null) this.FrontLeft.steerAngle = steer;
            if (this.FrontRight != null) this.FrontRight.steerAngle = steer;

            float motor = _throttle * this.MaxMotorTorque;
            if (this.RearLeft != null) this.RearLeft.motorTorque = motor;
            if (this.RearRight != null) this.RearRight.motorTorque = motor;

            float brake = _brake ? this.MaxBrakeTorque : 0f;
            if (this.FrontLeft != null) this.FrontLeft.brakeTorque = brake;
            if (this.FrontRight != null) this.FrontRight.brakeTorque = brake;
            if (this.RearLeft != null) this.RearLeft.brakeTorque = brake;
            if (this.RearRight != null) this.RearRight.brakeTorque = brake;

            Rigidbody rb = this.GetComponent<Rigidbody>();
            if (rb != null)
            {
                float speedMs = rb.velocity.magnitude;
                float speedKmh = speedMs * 3.6f;
                this.FuelTank.Burn(speedMs * Time.fixedDeltaTime, speedKmh);
            }
        }

        public float SpeedKmh()
        {
            Rigidbody rb = this.GetComponent<Rigidbody>();
            if (rb == null) return 0f;
            return rb.velocity.magnitude * 3.6f;
        }
    }
}
