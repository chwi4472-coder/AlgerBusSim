using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace AlgerBusSim.Core
{
    /// <summary>
    /// Single entry point of the game. Lives on the only authored scene
    /// (Assets/Scenes/Bootstrap.unity) and builds everything else at runtime.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Bootstrap : MonoBehaviour
    {
        public const string Version = "0.1.0-phase5";

        [Header("Runtime")]
        public int targetFrameRate = 60;
        public bool keepScreenAwake = true;

        static Bootstrap s_instance;
        public static Bootstrap Instance => s_instance;

        private GameObject _worldRoot;

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

            Debug.Log("[AlgerBusSim] Bootstrap " + Version + " — " + SystemInfo.deviceModel +
                      " (" + SystemInfo.graphicsDeviceType + "), " + SystemInfo.systemMemorySize + " MB RAM");

            StartCoroutine(BootSequence());
        }

        void ApplyRuntimeSettings()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = Mathf.Max(30, targetFrameRate);
            Screen.sleepTimeout = keepScreenAwake ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Input.multiTouchEnabled = true;
        }

        void EnsureCameraAndLight()
        {
            if (Camera.main == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                var cam = camGo.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.Skybox;
                cam.backgroundColor = new Color(0.55f, 0.72f, 0.88f);
                cam.fieldOfView = 60f;
                cam.nearClipPlane = 0.1f;
                cam.farClipPlane = 2000f;
                camGo.transform.position = new Vector3(0f, 80f, 200f);
                camGo.transform.LookAt(new Vector3(0f, 0f, -240f));
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

        IEnumerator BootSequence()
        {
            Debug.Log("[AlgerBusSim] Boot: loading city data...");
            AlgerBusSim.Data.CityData city = null;
            AlgerBusSim.Data.RoadGraphData roads = null;

            // Load city JSON
            string cityPath = Application.streamingAssetsPath + "/data/cities/algiers.json";
            Debug.Log("[AlgerBusSim] Loading " + cityPath);
            using (var req = UnityWebRequest.Get(cityPath))
            {
                yield return req.SendWebRequest();
                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("[AlgerBusSim] City JSON load failed: " + req.error);
                }
                else
                {
                    try
                    {
                        city = JsonUtility.FromJson<AlgerBusSim.Data.CityData>(req.downloadHandler.text);
                        Debug.Log("[AlgerBusSim] City loaded: " + (city != null ? city.id : "null"));
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("[AlgerBusSim] City JSON parse error: " + e);
                    }
                }
            }

            // Load roads JSON
            string roadsPath = Application.streamingAssetsPath + "/data/cities/algiers_roads.json";
            Debug.Log("[AlgerBusSim] Loading " + roadsPath);
            using (var req = UnityWebRequest.Get(roadsPath))
            {
                yield return req.SendWebRequest();
                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("[AlgerBusSim] Roads JSON load failed: " + req.error);
                }
                else
                {
                    try
                    {
                        roads = JsonUtility.FromJson<AlgerBusSim.Data.RoadGraphData>(req.downloadHandler.text);
                        Debug.Log("[AlgerBusSim] Roads loaded: " + (roads != null && roads.nodes != null ? roads.nodes.Count + " nodes" : "null"));
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("[AlgerBusSim] Roads JSON parse error: " + e);
                    }
                }
            }

            _worldRoot = new GameObject("World");

            bool ok = false;
            if (city != null && roads != null)
            {
                try
                {
                    AlgerBusSim.World.CityGenerator.Generate(city, roads, _worldRoot);
                    ok = true;
                    Debug.Log("[AlgerBusSim] City generated successfully.");
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[AlgerBusSim] City generation threw: " + e);
                }
            }

            if (!ok)
            {
                Debug.LogWarning("[AlgerBusSim] Falling back to minimal environment.");
                BuildFallback(_worldRoot);
            }

            // Frame the camera on the generated world
            FrameCamera();

            // Minimal HUD
            CreateHud();

            Debug.Log("[AlgerBusSim] Boot complete.");
        }

        private void BuildFallback(GameObject parent)
        {
            // Ground
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "FallbackGround";
            ground.transform.SetParent(parent.transform);
            ground.transform.localScale = new Vector3(50f, 1f, 50f);
            ground.transform.position = Vector3.zero;

            // A road (long box)
            var road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name = "FallbackRoad";
            road.transform.SetParent(parent.transform);
            road.transform.position = new Vector3(0f, 0.05f, 0f);
            road.transform.localScale = new Vector3(8f, 0.1f, 200f);

            // A bus (box)
            var bus = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bus.name = "FallbackBus";
            bus.transform.SetParent(parent.transform);
            bus.transform.position = new Vector3(0f, 1.5f, 0f);
            bus.transform.localScale = new Vector3(2.5f, 3f, 12f);

            // A few buildings
            for (int i = 0; i < 6; i++)
            {
                var b = GameObject.CreatePrimitive(PrimitiveType.Cube);
                b.name = "FallbackBuilding" + i;
                b.transform.SetParent(parent.transform);
                float x = (i % 2 == 0 ? -15f : 15f);
                float z = -30f + i * 12f;
                b.transform.position = new Vector3(x, 5f, z);
                b.transform.localScale = new Vector3(8f, 10f, 8f);
            }
        }

        private void FrameCamera()
        {
            var cam = Camera.main;
            if (cam == null) return;
            // Place camera above and behind the city center, looking at the road spine
            cam.transform.position = new Vector3(-200f, 150f, 100f);
            cam.transform.LookAt(new Vector3(-200f, 0f, -240f));
        }

        private void CreateHud()
        {
            var canvasGo = new GameObject("HUD");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            var textGo = new GameObject("Title");
            textGo.transform.SetParent(canvasGo.transform);
            var text = textGo.AddComponent<UnityEngine.UI.Text>();
            text.text = "AlgerBusSim — حافلة الجزائر";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperLeft;
            var rt = textGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(10f, -10f);
            rt.sizeDelta = new Vector2(400f, 40f);
        }
    }
}
