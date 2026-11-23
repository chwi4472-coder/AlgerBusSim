using UnityEngine;

namespace AlgerBusSim.Vehicles
{
    [System.Serializable]
    public class DamageModel
    {
        public float Damage01 = 0f;

        public void Add(float amount)
        {
            this.Damage01 = Mathf.Clamp01(this.Damage01 + amount);
        }

        public void Repair()
        {
            this.Damage01 = 0f;
        }

        public float RepairCostDzd()
        {
            return this.Damage01 * 50000f;
        }

        public bool IsCritical() { return this.Damage01 > 0.75f; }
    }
}
