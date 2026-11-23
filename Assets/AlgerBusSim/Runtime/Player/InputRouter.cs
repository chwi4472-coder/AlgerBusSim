using UnityEngine;
using UnityEngine.EventSystems;

namespace AlgerBusSim.Player
{
    public static class InputRouter
    {
        public static float SteerTarget;
        public static float ThrottleTarget;
        public static float BrakeTarget;
        public static float Steer;
        public static float Throttle;
        public static float Brake;
        public static bool Handbrake;
        public static bool DoorsPressed;
        public static bool HornHeld;
        public static bool HeadlightsPressed;
        public static bool GearUpPressed;
        public static bool GearDownPressed;
        public static bool DriveModePressed;
        public static bool AutoManualPressed;
        public static bool CameraPressed;
        public static bool PausePressed;

        public static void ResetFrame()
        {
            DoorsPressed = false;
            HeadlightsPressed = false;
            GearUpPressed = false;
            GearDownPressed = false;
            DriveModePressed = false;
            AutoManualPressed = false;
            CameraPressed = false;
            PausePressed = false;
        }

        public static void Tick(float dt)
        {
            // keyboard
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h = -1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h = 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v = 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v = -1f;

            float steerKey = h;
            float thrKey = Mathf.Clamp01(v);
            float brkKey = v < 0 ? -v : 0f;
            if (Input.GetKey(KeyCode.Space)) brkKey = 1f;

            SteerTarget = Mathf.Clamp(steerKey + TouchSteer, -1f, 1f);
            ThrottleTarget = Mathf.Clamp01(thrKey + TouchThrottle);
            BrakeTarget = Mathf.Clamp01(brkKey + TouchBrake);

            Steer = Mathf.MoveTowards(Steer, SteerTarget, dt * 4f);
            Throttle = Mathf.MoveTowards(Throttle, ThrottleTarget, dt * 6f);
            Brake = Mathf.MoveTowards(Brake, BrakeTarget, dt * 8f);

            Handbrake = Input.GetKey(KeyCode.LeftShift) || TouchHandbrake;

            if (Input.GetKeyDown(KeyCode.O)) DoorsPressed = true;
            if (Input.GetKeyDown(KeyCode.H)) HornHeld = true;
            if (Input.GetKeyUp(KeyCode.H)) HornHeld = false;
            if (Input.GetKeyDown(KeyCode.L)) HeadlightsPressed = true;
            if (Input.GetKeyDown(KeyCode.C)) CameraPressed = true;
            if (Input.GetKeyDown(KeyCode.R)) DriveModePressed = true;
            if (Input.GetKeyDown(KeyCode.M)) AutoManualPressed = true;
            if (Input.GetKeyDown(KeyCode.PageUp)) GearUpPressed = true;
            if (Input.GetKeyDown(KeyCode.PageDown)) GearDownPressed = true;
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) PausePressed = true;

            // touch horn handled by HoldButton setting TouchHorn
            if (TouchHorn) HornHeld = true;
            else if (!Input.GetKey(KeyCode.H)) HornHeld = false;

            // reset touch pulses
            TouchDoorsPulse = false;
        }

        // Touch state set by UI HoldButtons
        public static float TouchSteer;
        public static float TouchThrottle;
        public static float TouchBrake;
        public static bool TouchHandbrake;
        public static bool TouchHorn;
        public static bool TouchDoorsPulse;
    }

    public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public enum Kind { SteerLeft, SteerRight, Throttle, Brake, Handbrake, Horn }
        public Kind kind;
        public UnityEngine.UI.Image image;
        public Color normalColor = new Color(1f, 1f, 1f, 0.35f);
        public Color pressedColor = new Color(1f, 1f, 1f, 0.75f);

        public void OnPointerDown(PointerEventData e)
        {
            Set(true);
            if (image != null) image.color = pressedColor;
        }
        public void OnPointerUp(PointerEventData e)
        {
            Set(false);
            if (image != null) image.color = normalColor;
        }
        public void OnPointerExit(PointerEventData e)
        {
            Set(false);
            if (image != null) image.color = normalColor;
        }

        private void Set(bool down)
        {
            switch (kind)
            {
                case Kind.SteerLeft: InputRouter.TouchSteer = down ? -1f : 0f; break;
                case Kind.SteerRight: InputRouter.TouchSteer = down ? 1f : 0f; break;
                case Kind.Throttle: InputRouter.TouchThrottle = down ? 1f : 0f; break;
                case Kind.Brake: InputRouter.TouchBrake = down ? 1f : 0f; break;
                case Kind.Handbrake: InputRouter.TouchHandbrake = down; break;
                case Kind.Horn: InputRouter.TouchHorn = down; break;
            }
        }
    }
}
