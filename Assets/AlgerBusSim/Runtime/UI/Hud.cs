using UnityEngine;
using UnityEngine.UI;

namespace AlgerBusSim.UI
{
    public class Hud : MonoBehaviour
    {
        public Text SpeedText;
        public Text MoneyText;
        public Text FuelText;
        public Text StopText;

        public void SetSpeed(float kmh) { if (this.SpeedText != null) this.SpeedText.text = kmh.ToString("0") + " km/h"; }
        public void SetMoney(long dzd) { if (this.MoneyText != null) this.MoneyText.text = dzd.ToString("N0") + " DA"; }
        public void SetFuel(float level01) { if (this.FuelText != null) this.FuelText.text = (level01 * 100f).ToString("0") + " %"; }
        public void SetStop(string name) { if (this.StopText != null) this.StopText.text = name; }
    }
}
