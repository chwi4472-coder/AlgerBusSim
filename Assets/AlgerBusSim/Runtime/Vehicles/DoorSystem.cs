using UnityEngine;

namespace AlgerBusSim.Vehicles
{
    public class DoorSystem
    {
        public bool DoorsOpen = false;
        public bool Interlock = false;
        public float OpenProgress = 0f;

        public void Toggle()
        {
            if (this.Interlock) return;
            this.DoorsOpen = !this.DoorsOpen;
        }

        public void Update(float dt)
        {
            float target = this.DoorsOpen ? 1f : 0f;
            this.OpenProgress = Mathf.MoveTowards(this.OpenProgress, target, dt * 2f);
        }

        public bool IsClosed()
        {
            return !this.DoorsOpen && this.OpenProgress <= 0.01f;
        }
    }
}
