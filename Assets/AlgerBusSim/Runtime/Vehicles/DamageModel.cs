namespace AlgerBusSim.Vehicles
{
    public class DamageModel
    {
        public float Damage01 = 0f;

        public void Add(float amount)
        {
            this.Damage01 = Mathf.Clamp01(this.Damage01 + amount);
        }

        public float RepairCostDzd()
        {
            return this.Damage01 * 50000f;
        }
    }
}
