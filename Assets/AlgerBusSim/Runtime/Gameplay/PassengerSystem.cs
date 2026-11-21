using System.Collections.Generic;
using UnityEngine;

namespace AlgerBusSim.Gameplay
{
    public class Passenger
    {
        public string Id;
        public string DestinationStopId;
        public bool Boarded;
        public bool Paid;
    }

    public class PassengerSystem
    {
        public int Seats = 28;
        public int Standing = 45;
        public List<Passenger> OnBoard = new List<Passenger>();
        public List<Passenger> Waiting = new List<Passenger>();

        public int Capacity() { return this.Seats + this.Standing; }

        public bool Board(Passenger p)
        {
            if (this.OnBoard.Count >= this.Capacity()) return false;
            p.Boarded = true;
            this.OnBoard.Add(p);
            return true;
        }

        public int AlightAt(string stopId)
        {
            int count = 0;
            for (int i = this.OnBoard.Count - 1; i >= 0; i--)
            {
                if (this.OnBoard[i].DestinationStopId == stopId)
                {
                    this.OnBoard.RemoveAt(i);
                    count++;
                }
            }
            return count;
        }
    }
}
