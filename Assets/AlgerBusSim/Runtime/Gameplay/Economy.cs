using System.Collections.Generic;
using UnityEngine;

namespace AlgerBusSim.Gameplay
{
    public class Economy
    {
        public long MoneyDzd = 0;
        public long Reputation = 0;

        public void Add(long amount) { this.MoneyDzd += amount; }
        public bool TrySpend(long amount)
        {
            if (this.MoneyDzd < amount) return false;
            this.MoneyDzd -= amount;
            return true;
        }
    }
}
