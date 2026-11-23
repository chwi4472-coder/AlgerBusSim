using UnityEngine;
using UnityEngine.UI;
using AlgerBusSim.Core;
using AlgerBusSim.Art;
using AlgerBusSim.Player;
using AlgerBusSim.Localization;
using AlgerBusSim.Gameplay;

namespace AlgerBusSim.UI
{
    public class GameHud : MonoBehaviour
    {
        public Canvas Canvas;
        private GameObject _root;
        private Text _speedText;
        private Text _gearText;
        private Text _fuelText;
        private Text _moneyText;
        private Text _paxText;
        private Text _stopText;
        private Text _distText;
        private Text _timeText;
        private Text _routeText;
        private Text _hintText;
        private Image _fuelBar;
        private GameObject _pausePanel;
        private GameObject _summaryPanel;

        public void Init()
        {
            UiFactory.LoadFonts();
            Build();
            Hide();
        }

        private void Build()
        {
            var canvasGo = new GameObject("HudCanvas");
            canvasGo.transform.SetParent(transform, false);
            Canvas = canvasGo.AddComponent<Canvas>();
            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Canvas.sortingOrder = 40;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            _root = new GameObject("HudRoot");
            _root.transform.SetParent(canvasGo.transform, false);
            var rootRt = _root.AddComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero; rootRt.anchorMax = Vector2.one; rootRt.offsetMin = Vector2.zero; rootRt.offsetMax = Vector2.zero;
            var safe = _root.AddComponent<SafeArea>();

            // Top-left route panel
            var routePanel = UiFactory.Panel(_root.transform, "RoutePanel", new Color(0.06f, 0.08f, 0.12f, 0.7f), new Vector2(420, 70), new Vector2(0, 0));
            var rprt = routePanel.GetComponent<RectTransform>();
            rprt.anchorMin = new Vector2(0, 1); rprt.anchorMax = new Vector2(0, 1); rprt.pivot = new Vector2(0, 1);
            rprt.anchoredPosition = new Vector2(20, -20);
            _routeText = UiFactory.Label(routePanel.transform, "RouteText", "Ligne M0", 22, Color.white, TextAnchor.MiddleLeft, UiFactory.FontBold);
            var rtrt = _routeText.GetComponent<RectTransform>(); rtrt.anchorMin = new Vector2(0, 0.5f); rtrt.anchorMax = new Vector2(1, 1); rtrt.offsetMin = new Vector2(12, 0); rtrt.offsetMax = new Vector2(-12, 0);
            _timeText = UiFactory.Label(routePanel.transform, "TimeText", "07:00", 18, new Color(0.75f, 0.78f, 0.85f), TextAnchor.MiddleLeft, UiFactory.FontRegular);
            var ttrt = _timeText.GetComponent<RectTransform>(); ttrt.anchorMin = new Vector2(0, 0); ttrt.anchorMax = new Vector2(1, 0.5f); ttrt.offsetMin = new Vector2(12, 0); ttrt.offsetMax = new Vector2(-12, 0);

            // Top-right money/pax panel
            var moneyPanel = UiFactory.Panel(_root.transform, "MoneyPanel", new Color(0.06f, 0.08f, 0.12f, 0.7f), new Vector2(300, 70), new Vector2(0, 0));
            var mprt = moneyPanel.GetComponent<RectTransform>();
            mprt.anchorMin = new Vector2(1, 1); mprt.anchorMax = new Vector2(1, 1); mprt.pivot = new Vector2(1, 1);
            mprt.anchoredPosition = new Vector2(-20, -20);
            _moneyText = UiFactory.Label(moneyPanel.transform, "MoneyText", "25 000 DA", 22, new Color(0.95f, 0.85f, 0.35f), TextAnchor.MiddleRight, UiFactory.FontBold);
            var mtrt = _moneyText.GetComponent<RectTransform>(); mtrt.anchorMin = new Vector2(0, 0.5f); mtrt.anchorMax = new Vector2(1, 1); mtrt.offsetMin = new Vector2(12, 0); mtrt.offsetMax = new Vector2(-12, 0);
            _paxText = UiFactory.Label(moneyPanel.transform, "PaxText", "0 voyageurs", 16, new Color(0.75f, 0.78f, 0.85f), TextAnchor.MiddleRight, UiFactory.FontRegular);
            var ptrt = _paxText.GetComponent<RectTransform>(); ptrt.anchorMin = new Vector2(0, 0); ptrt.anchorMax = new Vector2(1, 0.5f); ptrt.offsetMin = new Vector2(12, 0); ptrt.offsetMax = new Vector2(-12, 0);

            // Bottom-left speed panel
            var speedPanel = UiFactory.Panel(_root.transform, "SpeedPanel", new Color(0.06f, 0.08f, 0.12f, 0.75f), new Vector2(260, 150), new Vector2(0, 0));
            var sprt = speedPanel.GetComponent<RectTransform>();
            sprt.anchorMin = new Vector2(0, 0); sprt.anchorMax = new Vector2(0, 0); sprt.pivot = new Vector2(0, 0);
            sprt.anchoredPosition = new Vector2(20, 20);
            _speedText = UiFactory.Label(speedPanel.transform, "SpeedText", "0", 64, Color.white, TextAnchor.MiddleCenter, UiFactory.FontBold);
            var str = _speedText.GetComponent<RectTransform>(); str.anchorMin = new Vector2(0, 0.35f); str.anchorMax = new Vector2(1, 1); str.offsetMin = Vector2.zero; str.offsetMax = Vector2.zero;
            var kmh = UiFactory.Label(speedPanel.transform, "Kmh", "km/h", 16, new Color(0.7f, 0.72f, 0.78f), TextAnchor.MiddleCenter, UiFactory.FontRegular);
            var kmhr = kmh.GetComponent<RectTransform>(); kmhr.anchorMin = new Vector2(0, 0.2f); kmhr.anchorMax = new Vector2(1, 0.4f); kmhr.offsetMin = Vector2.zero; kmhr.offsetMax = Vector2.zero;
            _gearText = UiFactory.Label(speedPanel.transform, "GearText", "D1", 22, new Color(0.35f, 0.85f, 0.55f), TextAnchor.MiddleCenter, UiFactory.FontBold);
            var gtr = _gearText.GetComponent<RectTransform>(); gtr.anchorMin = new Vector2(0, 0); gtr.anchorMax = new Vector2(0.5f, 0.22f); gtr.offsetMin = Vector2.zero; gtr.offsetMax = Vector2.zero;
            _fuelText = UiFactory.Label(speedPanel.transform, "FuelText", "55%", 16, new Color(0.85f, 0.7f, 0.3f), TextAnchor.MiddleCenter, UiFactory.FontRegular);
            var ftr = _fuelText.GetComponent<RectTransform>(); ftr.anchorMin = new Vector2(0.5f, 0); ftr.anchorMax = new Vector2(1, 0.22f); ftr.offsetMin = Vector2.zero; ftr.offsetMax = Vector2.zero;

            // fuel bar
            var fuelBarBg = UiFactory.Panel(speedPanel.transform, "FuelBarBg", new Color(0.15f, 0.16f, 0.2f, 1f), new Vector2(220, 8), new Vector2(0, 0));
            var fbbrt = fuelBarBg.GetComponent<RectTransform>(); fbbrt.anchorMin = new Vector2(0.5f, 0.22f); fbbrt.anchorMax = new Vector2(0.5f, 0.22f); fbbrt.pivot = new Vector2(0.5f, 0.5f); fbbrt.anchoredPosition = new Vector2(0, 8);
            var fuelBarGo = new GameObject("FuelBar");
            fuelBarGo.transform.SetParent(fuelBarBg.transform, false);
            _fuelBar = fuelBarGo.AddComponent<Image>();
            _fuelBar.color = new Color(0.35f, 0.85f, 0.45f);
            var fbrt = fuelBarGo.GetComponent<RectTransform>(); fbrt.anchorMin = new Vector2(0, 0); fbrt.anchorMax = new Vector2(1, 1); fbrt.offsetMin = Vector2.zero; fbrt.offsetMax = Vector2.zero;

            // Next stop panel (bottom center)
            var stopPanel = UiFactory.Panel(_root.transform, "StopPanel", new Color(0.06f, 0.08f, 0.12f, 0.75f), new Vector2(460, 90), new Vector2(0, 0));
            var stoprt = stopPanel.GetComponent<RectTransform>();
            stoprt.anchorMin = new Vector2(0.5f, 0); stoprt.anchorMax = new Vector2(0.5f, 0); stoprt.pivot = new Vector2(0.5f, 0);
            stoprt.anchoredPosition = new Vector2(0, 20);
            _stopText = UiFactory.Label(stopPanel.transform, "StopText", "Prochain arrêt", 20, Color.white, TextAnchor.MiddleCenter, UiFactory.FontBold);
            var str2 = _stopText.GetComponent<RectTransform>(); str2.anchorMin = new Vector2(0, 0.5f); str2.anchorMax = new Vector2(1, 1); str2.offsetMin = new Vector2(12, 0); str2.offsetMax = new Vector2(-12, 0);
            _distText = UiFactory.Label(stopPanel.transform, "DistText", "0 m", 16, new Color(0.75f, 0.78f, 0.85f), TextAnchor.MiddleCenter, UiFactory.FontRegular);
            var dtr = _distText.GetComponent<RectTransform>(); dtr.anchorMin = new Vector2(0, 0); dtr.anchorMax = new Vector2(1, 0.5f); dtr.offsetMin = new Vector2(12, 0); dtr.offsetMax = new Vector2(-12, 0);

            // Hint text
            _hintText = UiFactory.Label(_root.transform, "HintText", "", 18, new Color(0.95f, 0.85f, 0.35f), TextAnchor.MiddleCenter, UiFactory.FontRegular);
            var htr = _hintText.GetComponent<RectTransform>(); htr.anchorMin = new Vector2(0.5f, 0.5f); htr.anchorMax = new Vector2(0.5f, 0.5f); htr.sizeDelta = new Vector2(600, 40); htr.anchoredPosition = new Vector2(0, 120);

            // Controls: steering left/right (bottom-left above speed panel)
            CreateHoldButton(_root.transform, "SteerLeft", "◀", new Vector2(90, 90), new Vector2(20, 180), HoldButton.Kind.SteerLeft);
            CreateHoldButton(_root.transform, "SteerRight", "▶", new Vector2(90, 90), new Vector2(120, 180), HoldButton.Kind.SteerRight);

            // Pedals (bottom-right)
            CreateHoldButton(_root.transform, "Brake", "FREIN", new Vector2(160, 160), new Vector2(-20, 20), HoldButton.Kind.Brake, new Color(0.78f, 0.18f, 0.18f, 0.7f));
            CreateHoldButton(_root.transform, "Throttle", "GAZ", new Vector2(160, 160), new Vector2(-190, 20), HoldButton.Kind.Throttle, new Color(0.18f, 0.55f, 0.25f, 0.7f));

            // Door button (tap)
            var doorBtn = UiFactory.Button(_root.transform, "DoorBtn", "PORTES", new Vector2(110, 50), new Vector2(-20, 150), new Color(0.18f, 0.20f, 0.28f, 0.85f), Color.white, 16, () => { if (GameSession.Instance.Controller != null) GameSession.Instance.Controller.DoorSystem.Toggle(); });
            var dbrt = doorBtn.GetComponent<RectTransform>(); dbrt.anchorMin = new Vector2(1, 0); dbrt.anchorMax = new Vector2(1, 0); dbrt.pivot = new Vector2(1, 0);

            // Horn button
            var hornBtn = UiFactory.Button(_root.transform, "HornBtn", "KLAXON", new Vector2(110, 50), new Vector2(-140, 150), new Color(0.18f, 0.20f, 0.28f, 0.85f), Color.white, 16, () => { });
            var hbrt = hornBtn.GetComponent<RectTransform>(); hbrt.anchorMin = new Vector2(1, 0); hbrt.anchorMax = new Vector2(1, 0); hbrt.pivot = new Vector2(1, 0);
            var hb = hornBtn.gameObject.AddComponent<HoldButton>();
            hb.kind = HoldButton.Kind.Horn;
            hb.image = hornBtn.GetComponent<Image>();
            hb.normalColor = new Color(0.18f, 0.20f, 0.28f, 0.85f);
            hb.pressedColor = new Color(0.3f, 0.32f, 0.4f, 0.95f);

            // Camera button
            var camBtn = UiFactory.Button(_root.transform, "CamBtn", "CAM", new Vector2(80, 50), new Vector2(-260, 150), new Color(0.18f, 0.20f, 0.28f, 0.85f), Color.white, 14, () => { if (GameSession.Instance.ChaseCam != null) GameSession.Instance.ChaseCam.Enabled = !GameSession.Instance.ChaseCam.Enabled; });
            var cbrt = camBtn.GetComponent<RectTransform>(); cbrt.anchorMin = new Vector2(1, 0); cbrt.anchorMax = new Vector2(1, 0); cbrt.pivot = new Vector2(1, 0);

            // Pause button (top center)
            var pauseBtn = UiFactory.Button(_root.transform, "PauseBtn", "II", new Vector2(60, 50), new Vector2(0, -20), new Color(0.18f, 0.20f, 0.28f, 0.85f), Color.white, 20, () => GameSession.Instance.PauseService());
            var pbrt = pauseBtn.GetComponent<RectTransform>(); pbrt.anchorMin = new Vector2(0.5f, 1); pbrt.anchorMax = new Vector2(0.5f, 1); pbrt.pivot = new Vector2(0.5f, 1);

            // Pause panel
            _pausePanel = UiFactory.Panel(_root.transform, "PausePanel", new Color(0.04f, 0.05f, 0.08f, 0.9f), new Vector2(400, 300), Vector2.zero);
            var pprt = _pausePanel.GetComponent<RectTransform>(); pprt.anchorMin = new Vector2(0.5f, 0.5f); pprt.anchorMax = new Vector2(0.5f, 0.5f);
            UiFactory.Label(_pausePanel.transform, "PauseTitle", "Service en pause", 28, Color.white, TextAnchor.MiddleCenter, UiFactory.FontBold).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100);
            UiFactory.Button(_pausePanel.transform, "ResumeBtn", "Reprendre", new Vector2(280, 55), new Vector2(0, 20), new Color(0.0f, 0.40f, 0.20f), Color.white, 20, () => GameSession.Instance.ResumeService()).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 20);
            UiFactory.Button(_pausePanel.transform, "RestartBtn", "Retour au menu", new Vector2(280, 55), new Vector2(0, -60), new Color(0.18f, 0.20f, 0.28f), Color.white, 20, () => GameSession.Instance.RestartService()).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -60);
            _pausePanel.SetActive(false);

            // Summary panel
            _summaryPanel = UiFactory.Panel(_root.transform, "SummaryPanel", new Color(0.04f, 0.05f, 0.08f, 0.92f), new Vector2(520, 420), Vector2.zero);
            var sprt2 = _summaryPanel.GetComponent<RectTransform>(); sprt2.anchorMin = new Vector2(0.5f, 0.5f); sprt2.anchorMax = new Vector2(0.5f, 0.5f);
            UiFactory.Label(_summaryPanel.transform, "SummaryTitle", "Service terminé", 30, Color.white, TextAnchor.MiddleCenter, UiFactory.FontBold).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 170);
            UiFactory.Button(_summaryPanel.transform, "RestartSummaryBtn", "Nouveau service", new Vector2(300, 55), new Vector2(0, -160), new Color(0.0f, 0.40f, 0.20f), Color.white, 20, () => GameSession.Instance.RestartService()).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -160);
            _summaryPanel.SetActive(false);
        }

        private void CreateHoldButton(Transform parent, string name, string label, Vector2 size, Vector2 pos, HoldButton.Kind kind, Color? color = null)
        {
            Color bg = color ?? new Color(1f, 1f, 1f, 0.25f);
            var btn = UiFactory.Button(parent, name, label, size, pos, bg, Color.white, 18, null);
            var hb = btn.gameObject.AddComponent<HoldButton>();
            hb.kind = kind;
            hb.image = btn.GetComponent<Image>();
            hb.normalColor = bg;
            hb.pressedColor = new Color(bg.r, bg.g, bg.b, 0.7f);
            btn.GetComponent<Image>().color = bg;
            // position anchors bottom-left
            var rt = btn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(0, 0); rt.pivot = new Vector2(0, 0);
        }

        public void Show() { if (_root != null) _root.SetActive(true); }
        public void Hide() { if (_root != null) _root.SetActive(false); }

        public void SetSpeed(float kmh) { if (_speedText != null) _speedText.text = Mathf.Abs(kmh).ToString("0"); }
        public void SetGear(string g) { if (_gearText != null) _gearText.text = g; }
        public void SetFuel(float l01) { if (_fuelText != null) _fuelText.text = (l01 * 100f).ToString("0") + "%"; if (_fuelBar != null) _fuelBar.fillAmount = l01; }
        public void SetMoney(long dzd) { if (_moneyText != null) _moneyText.text = dzd.ToString("N0") + " DA"; }
        public void SetPassengers(int n) { if (_paxText != null) _paxText.text = n + " voyageurs"; }
        public void SetStop(string name) { if (_stopText != null) _stopText.text = name; }
        public void SetDistance(float m) { if (_distText != null) _distText.text = m.ToString("0") + " m"; }
        public void SetTime(string t) { if (_timeText != null) _timeText.text = t; }
        public void SetRoute(string r) { if (_routeText != null) _routeText.text = r; }
        public void SetHint(string h) { if (_hintText != null) _hintText.text = h; }
        public void Toast(string msg) { SetHint(msg); }

        public void UpdateRoute(string stopName, float dist, int onboard, long earned, int idx, int total)
        {
            SetStop(string.IsNullOrEmpty(stopName) ? "Terminus" : stopName);
            SetDistance(dist);
            SetPassengers(onboard);
            SetMoney(earned + (GameSession.Instance != null ? 25000 : 0));
            SetHint("Arrêt " + (idx + 1) + "/" + total);
        }

        public void ShowPause(bool show) { if (_pausePanel != null) _pausePanel.SetActive(show); }
        public void ShowSummary(RouteSession.Summary s)
        {
            if (_summaryPanel == null) return;
            _summaryPanel.SetActive(true);
            // add summary labels (simple)
            var t = _summaryPanel.transform.Find("SummaryTitle");
            if (t != null) { var txt = t.GetComponent<Text>(); if (txt != null) txt.text = "Service terminé"; }
        }
    }
}
