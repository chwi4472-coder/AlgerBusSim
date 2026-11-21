using UnityEngine;

namespace AlgerBusSim.Vehicles
{
    public class FuelTank
    {
        public float TankL = 250f;
        public float FuelL = 250f;
        public float ConsumptionL100km = 28f;

        public void Burn(float distanceM, float speedKmh)
        {
            float km = distanceM / 1000f;
            float used = km * (this.ConsumptionL100km / 100f);
            this.FuelL = Mathf.Max(0f, this.FuelL - used);
        }

        public float Level01()
        {
            return this.TankL > 0 ? this.FuelL / this.TankL : 0f;
        }
    }
}
