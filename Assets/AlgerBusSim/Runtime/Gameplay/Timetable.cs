using System.Collections.Generic;

namespace AlgerBusSim.Gameplay
{
    public class Timetable
    {
        public int HeadwayMin = 12;
        public int FirstDepartureHour = 5;
        public int LastDepartureHour = 22;

        public List<int> DepartureMinutes()
        {
            var list = new List<int>();
            for (int h = this.FirstDepartureHour; h <= this.LastDepartureHour; h++)
            {
                for (int m = 0; m < 60; m += this.HeadwayMin)
                {
                    list.Add(h * 60 + m);
                }
            }
            return list;
        }
    }
}
