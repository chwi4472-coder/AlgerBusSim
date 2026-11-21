using UnityEngine;

namespace AlgerBusSim.Vehicles
{
    public class WheelVisuals : MonoBehaviour
    {
        public WheelCollider Collider;
        public Transform Visual;

        void Update()
        {
            if (this.Collider == null || this.Visual == null) return;
            Vector3 pos;
            Quaternion rot;
            this.Collider.GetWorldPose(out pos, out rot);
            this.Visual.position = pos;
            this.Visual.rotation = rot;
        }
    }
}
