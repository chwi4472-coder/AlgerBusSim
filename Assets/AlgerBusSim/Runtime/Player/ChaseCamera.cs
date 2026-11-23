using UnityEngine;
using AlgerBusSim.Vehicles;

namespace AlgerBusSim.Player
{
    public class ChaseCamera : MonoBehaviour
    {
        public Transform Target;
        public Camera Camera;
        public bool Enabled = true;
        public float Distance = 13f;
        public float Height = 5.2f;
        public float LookAhead = 4f;
        public float Smooth = 6f;
        public float FovBase = 62f;
        public float FovSpeed = 72f;
        public float CollisionRadius = 0.6f;

        private Vector3 _vel;
        private float _mode; // 0 chase, 1 close, 2 far
        private float _modeTimer;

        public void Init(Camera cam) { Camera = cam; }

        void LateUpdate()
        {
            if (!Enabled || Target == null || Camera == null) return;
            // mode toggle via InputRouter.CameraPressed handled by GameSession? We handle here via a key
            if (InputRouter.CameraPressed)
            {
                _mode = (_mode + 1f) % 3f;
            }

            float dist = Distance;
            float height = Height;
            if (_mode == 1) { dist = 8f; height = 3.4f; }
            else if (_mode == 2) { dist = 20f; height = 9f; }

            Vector3 forward = Target.forward;
            Vector3 desired = Target.position - forward * dist + Vector3.up * height;
            // collision avoidance
            Vector3 origin = Target.position + Vector3.up * 1.2f;
            Vector3 toDesired = desired - origin;
            float len = toDesired.magnitude;
            if (Physics.SphereCast(origin, CollisionRadius, toDesired.normalized, out RaycastHit hit, len, ~ (1 << 8)))
            {
                desired = origin + toDesired.normalized * (hit.distance - 0.5f);
            }

            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _vel, 1f / Smooth);

            Vector3 lookTarget = Target.position + forward * LookAhead + Vector3.up * 1.6f;
            Quaternion lookRot = Quaternion.LookRotation(lookTarget - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * Smooth);

            // FOV
            float speedKmh = 0f;
            var bc = Target.GetComponent<BusController>();
            if (bc != null) speedKmh = Mathf.Abs(bc.SpeedKmh());
            Camera.fieldOfView = Mathf.Lerp(Camera.fieldOfView, Mathf.Lerp(FovBase, FovSpeed, Mathf.Clamp01(speedKmh / 90f)), Time.deltaTime * 4f);
        }
    }
}
