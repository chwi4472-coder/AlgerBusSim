using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using AlgerBusSim.Art;
using AlgerBusSim.Data;
using AlgerBusSim.World;
using AlgerBusSim.Player;
using AlgerBusSim.Vehicles;
using AlgerBusSim.Traffic;
using AlgerBusSim.Audio;
using AlgerBusSim.Gameplay;
using AlgerBusSim.UI;
using AlgerBusSim.Localization;

namespace AlgerBusSim.Core
{
    public enum GameState { Loading, Menu, Driving, Paused, Finished }

    public class GameSession : MonoBehaviour
    {
        public static GameSession Instance;

        [Header("Runtime")]
        public int targetFrameRate = 60;

        public GameState State = GameState.Loading;
        private bool _hornWasHeld;
        public CityData City;
        public RoadGraphData Roads;
        public RouteData Route;
        public EconomyData Economy;
        public WorldLayout Layout;
        public GameObject Bus;
        public BusController Controller;
        public ChaseCamera ChaseCam;
        public GameHud Hud;
        public MenuScreen Menu;
        public DayNightCycle DayNight;
        public WeatherSystem Weather;
        public TrafficSystem Traffic;
        public PedestrianSystem Peds;
        public ProceduralAudio Audio;
        public RouteSession RouteSession;

        private Camera _cam;
        private Light _sun;
        private Canvas _loadingCanvas;
        private Text _loadingText;
        private Text _loadingSub;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Application.targetFrameRate = Mathf.Max(30, targetFrameRate);
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Input.multiTouchEnabled = true;
            Diagnostics.Start();
            QualityTierProvider.Current = (QualityTier)Diagnostics.QualityTier;
            StartCoroutine(Boot());
        }

        IEnumerator Boot()
        {
            // Ensure camera + sun + event system
            EnsureCameraAndLight();
            EnsureEventSystem();

            // Loading UI
            BuildLoadingUI();
            SetLoading("Chargement…", "Initialisation");

            // Load economy
            yield return LoadJson<EconomyData>("data/economy.json", d => Economy = d);
            if (Economy == null) Economy = new EconomyData { currency = "DZD", urbanTicketDzd = 30, fuelDzdPerL = 29f, startingCashDzd = 25000, stopBonusDzd = 350, punctualityBonusDzd = 500, penalties = new PenaltiesData { redLightDzd = 4000, speedingDzd = 2000, missedStopDzd = 1500 } };

            // Load city
            SetLoading("Chargement d'Alger…", "Ville");
            yield return LoadJson<CityData>("data/cities/algiers.json", d => City = d);

            // Load roads
            yield return LoadJson<RoadGraphData>("data/cities/algiers_roads.json", d => Roads = d);

            // Load route
            yield return LoadJson<RouteData>("data/routes/algiers_m0.json", d => Route = d);

            // Preload textures
            SetLoading("Préparation des quartiers…", "Textures");
            string[] keys = new string[] {
                "ground_asphalt","ground_sidewalk","ground_kerb","ground_sand","water",
                "roof_flat","roof_tile","decal_algerbus","bus_plate_dz","bus_dest_m0",
                "stop_panel","flag_dz","plaque_grande_poste","signs",
                "wall_babeloued_a","wall_babeloued_b","wall_centre_a","wall_centre_b",
                "wall_elharrach_a","wall_elharrach_b","wall_hydra_a","wall_hydra_b",
                "wall_kouba_a","wall_kouba_b",
                "shop_babeloued","shop_centre","shop_elharrach","shop_hydra","shop_kouba",
                "ui_round","logo_algerbus","ui_icons" };
            yield return TextureLibrary.Preload(keys, p => { if (_loadingSub != null) _loadingSub.text = Mathf.RoundToInt(p * 100f) + " %"; });
            UiSprites.Init();

            // Localization default
            AlgerBusSim.Localization.Localization.Language = "fr";
            AlgerBusSim.Localization.Localization.Load();

            // World materials
            WorldMaterials.Init();

            // Generate city
            SetLoading("Construction de la ville…", "Géométrie");
            var worldRoot = new GameObject("World");
            Layout = CityGenerator.Generate(City, Roads, worldRoot);

            // Colliders for buildings (facades)
            AddBuildingColliders(worldRoot);

            // Day/night + weather
            var dnGo = new GameObject("DayNight");
            DayNight = dnGo.AddComponent<DayNightCycle>();
            DayNight.Init(_cam);
            DayNight.Sun = _sun;

            var wGo = new GameObject("Weather");
            Weather = wGo.AddComponent<WeatherSystem>();
            Weather.Init(_cam);
            Weather.Sun = _sun;

            // Traffic
            var trGo = new GameObject("Traffic");
            Traffic = trGo.AddComponent<TrafficSystem>();
            Traffic.Init(Layout.Graph, Layout.Traffic);

            // Pedestrians
            var pedGo = new GameObject("Pedestrians");
            Peds = pedGo.AddComponent<PedestrianSystem>();
            Peds.Spawn(Layout, 26);

            // Audio
            var audioGo = new GameObject("Audio");
            Audio = audioGo.AddComponent<ProceduralAudio>();
            Audio.Init();

            // Bus (spawn later on start; create now but inactive)
            SpawnBus();

            // Camera
            var chaseGo = new GameObject("ChaseCamera");
            ChaseCam = chaseGo.AddComponent<ChaseCamera>();
            ChaseCam.Target = Bus.transform;
            ChaseCam.Init(_cam);

            // HUD
            var hudGo = new GameObject("HUD");
            Hud = hudGo.AddComponent<GameHud>();
            Hud.Init();

            // Menu
            var menuGo = new GameObject("Menu");
            Menu = menuGo.AddComponent<MenuScreen>();
            Menu.Init();

            // Route session (created on start)
            State = GameState.Menu;
            HideLoadingUI();
            Menu.Show();

            Debug.Log("[AlgerBusSim] Boot complete — menu shown.");
        }

        private void SpawnBus()
        {
            Vector3 pos = Layout.RouteStart;
            Quaternion rot = Layout.RouteStartRotation;
            Bus = BusBuilder.Build("player", pos, rot);
            Controller = Bus.GetComponent<BusController>();
            // engine audio follow
            if (Audio != null) Audio.transform.SetParent(Bus.transform, false);
        }

        private void EnsureCameraAndLight()
        {
            _cam = Camera.main;
            if (_cam == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                _cam = camGo.AddComponent<Camera>();
                _cam.clearFlags = CameraClearFlags.SolidColor;
                _cam.backgroundColor = new Color(0.55f, 0.72f, 0.88f);
                _cam.fieldOfView = 62f;
                _cam.nearClipPlane = 0.15f;
                _cam.farClipPlane = 1800f;
                camGo.AddComponent<AudioListener>();
            }
            _cam.transform.position = new Vector3(-200f, 150f, 100f);

            _sun = FindAnyObjectByType<Light>();
            if (_sun == null)
            {
                var sunGo = new GameObject("Sun");
                _sun = sunGo.AddComponent<Light>();
                _sun.type = LightType.Directional;
                _sun.color = new Color(1f, 0.96f, 0.87f);
                _sun.intensity = 1.15f;
                _sun.shadows = LightShadows.Soft;
                sunGo.transform.rotation = Quaternion.Euler(52f, -35f, 0f);
            }
        }

        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
        }

        private void BuildLoadingUI()
        {
            var canvasGo = new GameObject("LoadingCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var panel = new GameObject("Panel");
            panel.transform.SetParent(canvasGo.transform, false);
            var img = panel.AddComponent<Image>();
            img.color = new Color(0.06f, 0.07f, 0.09f);
            var rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(panel.transform, false);
            _loadingText = titleGo.AddComponent<Text>();
            _loadingText.font = Resources.Load<Font>("Fonts/BarlowSemiCondensed-Bold");
            _loadingText.fontSize = 42;
            _loadingText.alignment = TextAnchor.MiddleCenter;
            _loadingText.color = Color.white;
            var trt = titleGo.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0.5f, 0.5f); trt.anchorMax = new Vector2(0.5f, 0.5f);
            trt.sizeDelta = new Vector2(600, 60); trt.anchoredPosition = new Vector2(0, 40);

            var subGo = new GameObject("Sub");
            subGo.transform.SetParent(panel.transform, false);
            _loadingSub = subGo.AddComponent<Text>();
            _loadingSub.font = Resources.Load<Font>("Fonts/BarlowSemiCondensed-Regular");
            _loadingSub.fontSize = 26;
            _loadingSub.alignment = TextAnchor.MiddleCenter;
            _loadingSub.color = new Color(0.7f, 0.72f, 0.78f);
            var srt = subGo.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.5f, 0.5f); srt.anchorMax = new Vector2(0.5f, 0.5f);
            srt.sizeDelta = new Vector2(600, 40); srt.anchoredPosition = new Vector2(0, -20);

            _loadingCanvas = canvas;
        }

        private void SetLoading(string title, string sub)
        {
            if (_loadingText != null) _loadingText.text = title;
            if (_loadingSub != null) _loadingSub.text = sub;
        }

        private void HideLoadingUI()
        {
            if (_loadingCanvas != null) _loadingCanvas.gameObject.SetActive(false);
        }

        private IEnumerator LoadJson<T>(string relative, System.Action<T> onLoaded) where T : class
        {
            string path = Application.streamingAssetsPath + "/" + relative;
            using (var req = UnityWebRequest.Get(path))
            {
                yield return req.SendWebRequest();
                if (req.result == UnityWebRequest.Result.Success)
                {
                    try { onLoaded(JsonUtility.FromJson<T>(req.downloadHandler.text)); }
                    catch (System.Exception e) { Debug.LogError("[AlgerBusSim] JSON parse error " + relative + ": " + e); }
                }
                else
                {
                    Debug.LogError("[AlgerBusSim] JSON load failed " + relative + ": " + req.error);
                }
            }
        }

        private void AddBuildingColliders(GameObject worldRoot)
        {
            // Add MeshColliders to facade meshes (children with name starting Facades_)
            foreach (var mf in worldRoot.GetComponentsInChildren<MeshFilter>())
            {
                if (mf.sharedMesh == null) continue;
                if (!mf.name.StartsWith("Facades_")) continue;
                var mc = mf.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;
            }
        }

        // Public control methods called by Menu/HUD
        public void StartService(int startHour, WeatherType weatherType, string language)
        {
            AlgerBusSim.Localization.Localization.Language = language;
            AlgerBusSim.Localization.Localization.Load();
            DayNight.SetHour(startHour);
            DayNight.UseExternalHour = false;
            DayNight.TimeScale = 0f; // time driven by RouteSession? keep static hour for visuals
            Weather.SetWeather(weatherType);

            Menu.Hide();
            State = GameState.Driving;

            // reset bus
            Controller.FuelTank.FuelL = Controller.FuelTank.TankL * 0.55f;
            Controller.Damage.Repair();
            Bus.transform.SetPositionAndRotation(Layout.RouteStart, Layout.RouteStartRotation);
            var rb = Bus.GetComponent<Rigidbody>();
            if (rb != null) { rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }

            ChaseCam.Enabled = true;
            Hud.Show();

            // Route session
            if (RouteSession != null) Destroy(RouteSession.gameObject);
            var rsGo = new GameObject("RouteSession");
            RouteSession = rsGo.AddComponent<RouteSession>();
            RouteSession.Init(this);
        }

        public void PauseService()
        {
            if (State != GameState.Driving) return;
            State = GameState.Paused;
            Time.timeScale = 0f;
            Hud.ShowPause(true);
        }

        public void ResumeService()
        {
            if (State != GameState.Paused) return;
            State = GameState.Driving;
            Time.timeScale = 1f;
            Hud.ShowPause(false);
        }

        public void RestartService()
        {
            Time.timeScale = 1f;
            State = GameState.Menu;
            Hud.Hide();
            if (RouteSession != null) { Destroy(RouteSession.gameObject); RouteSession = null; }
            Menu.Show();
        }

        public void EndService(RouteSession.Summary summary)
        {
            State = GameState.Finished;
            Time.timeScale = 0f;
            Hud.ShowSummary(summary);
        }

        void Update()
        {
            if (State == GameState.Driving)
            {
                if (InputRouter.PausePressed) PauseService();
                // audio
                if (Audio != null && Controller != null)
                {
                    float rpmNorm = (Controller.Powertrain.Rpm - Controller.Powertrain.IdleRpm) / (Controller.Powertrain.MaxRpm - Controller.Powertrain.IdleRpm);
                    Audio.UpdateEngine(rpmNorm, InputRouter.Throttle);
                    Audio.SetRainVolume(Weather != null && Weather.Type == WeatherType.Rain ? 0.6f : 0f);
                }
                if (Controller != null && InputRouter.HornHeld && !_hornWasHeld) { Audio.PlayHorn(); _hornWasHeld = true; }
                if (!InputRouter.HornHeld) _hornWasHeld = false;
            }
            else if (State == GameState.Paused)
            {
                if (InputRouter.PausePressed) ResumeService();
            }
        }
    }
}
