using System.Collections.Generic;
using UnityEngine;

namespace AlgerBusSim.Gameplay
{
    public class RouteService
    {
        public string RouteId;
        public List<string> StopIds = new List<string>();
        public int CurrentStopIndex = 0;

        public string CurrentStopId()
        {
            if (this.StopIds.Count == 0) return null;
            if (this.CurrentStopIndex < 0 || this.CurrentStopIndex >= this.StopIds.Count) return null;
            return this.StopIds[this.CurrentStopIndex];
        }

        public bool Advance()
        {
            if (this.CurrentStopIndex + 1 >= this.StopIds.Count) return false;
            this.CurrentStopIndex++;
            return true;
        }

        public bool IsLastStop()
        {
            return this.CurrentStopIndex >= this.StopIds.Count - 1;
        }
    }
}
