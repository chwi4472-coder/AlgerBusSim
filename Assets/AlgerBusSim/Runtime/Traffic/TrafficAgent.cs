using UnityEngine;

namespace AlgerBusSim.Traffic
{
    public class TrafficAgent : MonoBehaviour
    {
        public float SpeedMs = 8f;
        public float MaxSpeedMs = 13.89f; // 50 km/h
        public float Acceleration = 2f;
        public Vector3 Target;

        void Update()
        {
            Vector3 to = this.Target - this.transform.position;
            if (to.sqrMagnitude < 0.25f) return;
            Vector3 dir = to.normalized;
            this.SpeedMs = Mathf.MoveTowards(this.SpeedMs, this.MaxSpeedMs, this.Acceleration * Time.deltaTime);
            this.transform.position += dir * this.SpeedMs * Time.deltaTime;
            if (dir.sqrMagnitude > 0.0001f)
            {
                this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 4f);
            }
        }
    }
}
