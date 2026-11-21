namespace AlgerBusSim.Gameplay
{
    public class FareSystem
    {
        public int UrbanTicketDzd = 30;

        public long Collect(Passenger p)
        {
            if (p == null || p.Paid) return 0;
            p.Paid = true;
            return this.UrbanTicketDzd;
        }
    }
}
