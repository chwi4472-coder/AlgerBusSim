using UnityEngine;

namespace AlgerBusSim.Vehicles
{
    [System.Serializable]
    public class FuelTank
    {
        public float TankL = 250f;
        public float FuelL = 150f;
        public float ConsumptionL100km = 28f;

        public void Burn(float distanceM, float load)
        {
            float km = distanceM / 1000f;
            float factor = 0.6f + 0.4f * Mathf.Clamp01(load);
            float used = km * (this.ConsumptionL100km / 100f) * factor;
            this.FuelL = Mathf.Max(0f, this.FuelL - used);
        }

        public float Level01()
        {
            return this.TankL > 0 ? this.FuelL / this.TankL : 0f;
        }

        public void Refuel(float liters)
        {
            this.FuelL = Mathf.Min(this.TankL, this.FuelL + liters);
        }

        public bool IsLow() { return this.FuelL < this.TankL * 0.15f; }
    }
}
