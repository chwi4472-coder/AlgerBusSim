using UnityEngine;
using UnityEngine.UI;

namespace AlgerBusSim.UI
{
    public class TouchControls : MonoBehaviour
    {
        public RectTransform JoystickBase;
        public RectTransform JoystickKnob;
        public float Radius = 80f;
        public Vector2 Input { get; private set; }

        private int _touchId = -1;

        void Update()
        {
            if (UnityEngine.Input.touchCount == 0) { this.Input = Vector2.zero; return; }
            foreach (var t in UnityEngine.Input.touches)
            {
                if (t.phase == TouchPhase.Began && this._touchId == -1)
                {
                    this._touchId = t.fingerId;
                }
                if (t.fingerId == this._touchId)
                {
                    if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                    {
                        this._touchId = -1;
                        this.Input = Vector2.zero;
                        if (this.JoystickKnob != null) this.JoystickKnob.anchoredPosition = Vector2.zero;
                        return;
                    }
                    Vector2 local;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(this.JoystickBase, t.position, null, out local);
                    Vector2 clamped = Vector2.ClampMagnitude(local, this.Radius);
                    this.Input = clamped / this.Radius;
                    if (this.JoystickKnob != null) this.JoystickKnob.anchoredPosition = clamped;
                }
            }
        }
    }
}
