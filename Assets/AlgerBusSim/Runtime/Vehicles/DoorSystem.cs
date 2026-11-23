using System.Collections.Generic;
using UnityEngine;

namespace AlgerBusSim.Vehicles
{
    [System.Serializable]
    public class DoorSystem
    {
        public bool DoorsOpen = false;
        public bool Interlock = false;
        public float OpenProgress = 0f;
        public float SlideDistance = 0.72f;

        public List<Transform> LeftPanels = new List<Transform>();
        public List<Transform> RightPanels = new List<Transform>();
        private readonly List<Vector3> _leftClosed = new List<Vector3>();
        private readonly List<Vector3> _rightClosed = new List<Vector3>();
        private bool _bound;

        public void Bind()
        {
            if (_bound) return;
            _bound = true;
            _leftClosed.Clear(); _rightClosed.Clear();
            foreach (var t in LeftPanels) _leftClosed.Add(t.localPosition);
            foreach (var t in RightPanels) _rightClosed.Add(t.localPosition);
        }

        public void Toggle()
        {
            if (this.Interlock) return;
            this.DoorsOpen = !this.DoorsOpen;
        }

        public void SetOpen(bool open)
        {
            if (this.Interlock && open) return;
            this.DoorsOpen = open;
        }

        public void Update(float dt)
        {
            float target = this.DoorsOpen ? 1f : 0f;
            this.OpenProgress = Mathf.MoveTowards(this.OpenProgress, target, dt * 2.4f);
            ApplyVisuals();
        }

        private void ApplyVisuals()
        {
            Bind();
            for (int i = 0; i < LeftPanels.Count; i++)
            {
                if (i >= _leftClosed.Count) break;
                Vector3 c = _leftClosed[i];
                LeftPanels[i].localPosition = c + Vector3.back * (OpenProgress * SlideDistance);
            }
            for (int i = 0; i < RightPanels.Count; i++)
            {
                if (i >= _rightClosed.Count) break;
                Vector3 c = _rightClosed[i];
                RightPanels[i].localPosition = c + Vector3.forward * (OpenProgress * SlideDistance);
            }
        }

        public bool IsClosed()
        {
            return !this.DoorsOpen && this.OpenProgress <= 0.01f;
        }

        public bool IsOpen() { return this.DoorsOpen && this.OpenProgress >= 0.99f; }
    }
}
