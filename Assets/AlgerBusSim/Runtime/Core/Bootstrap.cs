using UnityEngine;

namespace AlgerBusSim.Core
{
    /// <summary>
    /// Single entry point of the game. Lives on the only authored scene
    /// (Assets/Scenes/Bootstrap.unity) and builds everything else at runtime.
    /// Expanded in Phase 5 with the service hub, clock and state machine.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Bootstrap : MonoBehaviour
    {
        public const string Version = "0.1.0-phase2";

        [Header("Runtime")]
        [Tooltip("Target frame rate on device. 60 is the design target.")]
        public int targetFrameRate = 60;

        [Tooltip("Keep the screen awake while driving.")]
        public bool keepScreenAwake = true;

        static Bootstrap s_instance;

        public static Bootstrap Instance => s_instance;

        void Awake()
        {
            if (s_instance != null && s_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_instance = this;
            DontDestroyOnLoad(gameObject);

            ApplyRuntimeSettings();
            EnsureCameraAndLight();

            Debug.Log($"[AlgerBusSim] Bootstrap {Version} — {SystemInfo.deviceModel} " +
                      $"({SystemInfo.graphicsDeviceType}), {SystemInfo.systemMemorySize} MB RAM");
        }

        void ApplyRuntimeSettings()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = Mathf.Max(30, targetFrameRate);
            Screen.sleepTimeout = keepScreenAwake ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Input.multiTouchEnabled = true;
        }

        /// <summary>
        /// The generated scene is intentionally empty; this guarantees a visible,
        /// correctly configured view even before the world generator exists.
        /// </summary>
        void EnsureCameraAndLight()
        {
            if (Camera.main == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                var cam = camGo.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.55f, 0.72f, 0.88f);
                cam.fieldOfView = 60f;
                cam.nearClipPlane = 0.15f;
                cam.farClipPlane = 900f;
                camGo.transform.SetPositionAndRotation(new Vector3(0f, 8f, -16f), Quaternion.Euler(8f, 0f, 0f));
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

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.72f, 0.86f);
            RenderSettings.ambientEquatorColor = new Color(0.52f, 0.50f, 0.46f);
            RenderSettings.ambientGroundColor = new Color(0.28f, 0.26f, 0.23f);
        }
    }
}
