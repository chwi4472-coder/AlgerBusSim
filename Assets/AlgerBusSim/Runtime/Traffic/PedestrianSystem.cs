using System.Collections.Generic;
using UnityEngine;

namespace AlgerBusSim.Traffic
{
    public class PedestrianSystem : MonoBehaviour
    {
        public float WalkSpeedMs = 1.4f;
        public List<Transform> Pedestrians = new List<Transform>();
        public List<Vector3> Targets = new List<Vector3>();

        void Update()
        {
            for (int i = 0; i < this.Pedestrians.Count; i++)
            {
                if (i >= this.Targets.Count) break;
                Transform p = this.Pedestrians[i];
                if (p == null) continue;
                Vector3 to = this.Targets[i] - p.position;
                if (to.sqrMagnitude < 0.04f) continue;
                p.position += to.normalized * this.WalkSpeedMs * Time.deltaTime;
            }
        }
    }
}
