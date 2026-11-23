using UnityEngine;

namespace AlgerBusSim.Core
{
    public sealed class Bootstrap : MonoBehaviour
    {
        public const string Version = "0.2.0-phase5";
        public int targetFrameRate = 60;
        public bool keepScreenAwake = true;

        void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = Mathf.Max(30, targetFrameRate);
            Screen.sleepTimeout = keepScreenAwake ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Input.multiTouchEnabled = true;

            EnsureCameraAndLight();

            Debug.Log("[AlgerBusSim] Bootstrap " + Version + " — " + SystemInfo.deviceModel +
                      " (" + SystemInfo.graphicsDeviceType + "), " + SystemInfo.systemMemorySize + " MB RAM");

            gameObject.AddComponent<GameSession>();
        }

        void EnsureCameraAndLight()
        {
            if (Camera.main == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                var cam = camGo.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.55f, 0.72f, 0.88f);
                cam.fieldOfView = 62f;
                cam.nearClipPlane = 0.15f;
                cam.farClipPlane = 1800f;
                camGo.transform.position = new Vector3(0f, 8f, -16f);
                camGo.transform.LookAt(new Vector3(0f, 0f, -24f));
                camGo.AddComponent<AudioListener>();
            }
            if (FindAnyObjectByType<Light>() == null)
            {
                var lightGo = new GameObject("Sun");
                var sun = lightGo.AddComponent<Light>();
                sun.type = LightType.Directional;
                sun.color = new Color(1f, 0.96f, 0.87f);
                sun.intensity = 1.15f;
                sun.shadows = LightShadows.Soft;
                lightGo.transform.rotation = Quaternion.Euler(52f, -35f, 0f);
            }
        }
    }
}
