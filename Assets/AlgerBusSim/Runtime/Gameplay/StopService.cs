using UnityEngine;

namespace AlgerBusSim.Gameplay
{
    public class StopService
    {
        public float StopRadiusM = 6f;
        public bool AtStop = false;

        public bool CheckArrival(Vector3 busPos, Vector3 stopPos)
        {
            float d = Vector3.Distance(busPos, stopPos);
            this.AtStop = d <= this.StopRadiusM;
            return this.AtStop;
        }
    }
}
