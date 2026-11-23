using System.Collections.Generic;
using UnityEngine;
using AlgerBusSim.Art;

namespace AlgerBusSim.Traffic
{
    public class TrafficLightSystem : MonoBehaviour
    {
        public class Head
        {
            public Transform Root;
            public Renderer Red;
            public Renderer Yellow;
            public Renderer Green;
            public int Axis;
        }

        public float GreenSeconds = 20f;
        public float YellowSeconds = 3f;
        public float RedSeconds = 22f;

        private readonly List<Head> _heads = new List<Head>();
        private float _timer;
        private int _state0; // 0 green, 1 yellow, 2 red
        private int _state1;

        public void AddHead(Head h) { if (h != null) _heads.Add(h); }

        void Update()
        {
            _timer += Time.deltaTime;
            if (_state0 == 0 && _timer >= GreenSeconds) { _state0 = 1; _timer = 0f; }
            else if (_state0 == 1 && _timer >= YellowSeconds) { _state0 = 2; _timer = 0f; }
            else if (_state0 == 2 && _timer >= RedSeconds) { _state0 = 0; _timer = 0f; }
            _state1 = (_state0 == 0) ? 2 : ((_state0 == 2) ? 0 : 1);

            foreach (var h in _heads)
            {
                int st = h.Axis == 0 ? _state0 : _state1;
                if (h.Red != null) h.Red.sharedMaterial = st == 2 ? WorldMaterials.SignalRed : WorldMaterials.SignalOff;
                if (h.Yellow != null) h.Yellow.sharedMaterial = st == 1 ? WorldMaterials.SignalYellow : WorldMaterials.SignalOff;
                if (h.Green != null) h.Green.sharedMaterial = st == 0 ? WorldMaterials.SignalGreen : WorldMaterials.SignalOff;
            }
        }

        public bool IsRed(int axis)
        {
            int st = axis == 0 ? _state0 : _state1;
            return st == 2;
        }

        public bool IsRedFor(Vector3 position, Vector3 forward)
        {
            Head nearest = null;
            float best = float.MaxValue;
            foreach (var h in _heads)
            {
                float d = Vector3.Distance(h.Root.position, position);
                if (d < best) { best = d; nearest = h; }
            }
            if (nearest == null || best > 22f) return false;
            return IsRed(nearest.Axis);
        }

        public Head NearestHead(Vector3 position)
        {
            Head nearest = null;
            float best = float.MaxValue;
            foreach (var h in _heads)
            {
                float d = Vector3.Distance(h.Root.position, position);
                if (d < best) { best = d; nearest = h; }
            }
            return best < 30f ? nearest : null;
        }
    }
}
