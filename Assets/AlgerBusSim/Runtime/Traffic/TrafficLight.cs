using UnityEngine;

namespace AlgerBusSim.Traffic
{
    public class TrafficLight : MonoBehaviour
    {
        public float GreenSeconds = 25f;
        public float RedSeconds = 30f;
        public float YellowSeconds = 3f;
        public int State = 0; // 0 green, 1 yellow, 2 red
        private float _timer;

        void Update()
        {
            this._timer += Time.deltaTime;
            if (this.State == 0 && this._timer >= this.GreenSeconds) { this.State = 1; this._timer = 0f; }
            else if (this.State == 1 && this._timer >= this.YellowSeconds) { this.State = 2; this._timer = 0f; }
            else if (this.State == 2 && this._timer >= this.RedSeconds) { this.State = 0; this._timer = 0f; }
        }

        public bool IsRed() { return this.State == 2; }
        public bool IsGreen() { return this.State == 0; }
    }
}
