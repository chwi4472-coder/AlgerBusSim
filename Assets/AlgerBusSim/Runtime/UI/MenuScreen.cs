using System;
using UnityEngine;
using UnityEngine.UI;
using AlgerBusSim.Core;
using AlgerBusSim.Art;
using AlgerBusSim.Localization;
using AlgerBusSim.World;

namespace AlgerBusSim.UI
{
    public class MenuScreen : MonoBehaviour
    {
        public Canvas Canvas;
        public GameObject Root;
        private Text _title;
        private Text _subtitle;
        private Dropdown _langDrop;
        private Dropdown _weatherDrop;
        private Dropdown _timeDrop;
        private string _selectedLang = "fr";
        private WeatherType _selectedWeather = WeatherType.Clear;
        private int _selectedHour = 7;

        public void Init()
        {
            UiFactory.LoadFonts();
            Build();
        }

        private void Build()
        {
            var canvasGo = new GameObject("MenuCanvas");
            canvasGo.transform.SetParent(transform, false);
            Canvas = canvasGo.AddComponent<Canvas>();
            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Canvas.sortingOrder = 50;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            Root = new GameObject("MenuRoot");
            Root.transform.SetParent(canvasGo.transform, false);
            var rootRt = Root.AddComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero; rootRt.anchorMax = Vector2.one; rootRt.offsetMin = Vector2.zero; rootRt.offsetMax = Vector2.zero;

            // Background gradient image
            var bg = new GameObject("Bg");
            bg.transform.SetParent(Root.transform, false);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.05f, 0.07f, 0.10f, 0.85f);
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one; bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;

            // Logo
            var logoGo = new GameObject("Logo");
            logoGo.transform.SetParent(Root.transform, false);
            var logoImg = logoGo.AddComponent<Image>();
            logoImg.sprite = UiSprites.Logo;
            logoImg.preserveAspect = true;
            var logoRt = logoGo.GetComponent<RectTransform>();
            logoRt.anchorMin = new Vector2(0.5f, 1f); logoRt.anchorMax = new Vector2(0.5f, 1f);
            logoRt.pivot = new Vector2(0.5f, 1f);
            logoRt.sizeDelta = new Vector2(360, 130);
            logoRt.anchoredPosition = new Vector2(0, -40);

            // Title
            _title = UiFactory.Label(Root.transform, "Title", "AlgerBusSim", 52, Color.white, TextAnchor.MiddleCenter, UiFactory.FontBold);
            var trt = _title.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0.5f, 1f); trt.anchorMax = new Vector2(0.5f, 1f);
            trt.pivot = new Vector2(0.5f, 1f);
            trt.sizeDelta = new Vector2(600, 60); trt.anchoredPosition = new Vector2(0, -180);

            _subtitle = UiFactory.Label(Root.transform, "Subtitle", "Simulateur de bus — Alger", 24, new Color(0.75f, 0.78f, 0.85f), TextAnchor.MiddleCenter, UiFactory.FontRegular);
            var srt = _subtitle.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.5f, 1f); srt.anchorMax = new Vector2(0.5f, 1f);
            srt.pivot = new Vector2(0.5f, 1f);
            srt.sizeDelta = new Vector2(600, 40); srt.anchoredPosition = new Vector2(0, -230);

            // Language selector
            var langLabel = UiFactory.Label(Root.transform, "LangLabel", "Langue", 20, Color.white, TextAnchor.MiddleLeft, UiFactory.FontBold);
            var llrt = langLabel.GetComponent<RectTransform>();
            llrt.anchorMin = new Vector2(0.5f, 0.5f); llrt.anchorMax = new Vector2(0.5f, 0.5f);
            llrt.sizeDelta = new Vector2(200, 30); llrt.anchoredPosition = new Vector2(-120, 60);

            // Use simple buttons for language instead of dropdown for reliability
            string[] langs = { "fr", "ar", "dz" };
            string[] langNames = { "Français", "العربية", "Darja" };
            for (int i = 0; i < langs.Length; i++)
            {
                int idx = i;
                var btn = UiFactory.Button(Root.transform, "Lang_" + langs[i], langNames[i], new Vector2(110, 44), new Vector2(-120 + i * 120, 0), new Color(0.18f, 0.20f, 0.28f), Color.white, 16, () => SelectLang(langs[idx]));
                var brt = btn.GetComponent<RectTransform>();
                brt.anchorMin = new Vector2(0.5f, 0.5f); brt.anchorMax = new Vector2(0.5f, 0.5f);
            }

            // Weather selector
            var wLabel = UiFactory.Label(Root.transform, "WLabel", "Météo", 20, Color.white, TextAnchor.MiddleLeft, UiFactory.FontBold);
            var wlrt = wLabel.GetComponent<RectTransform>();
            wlrt.anchorMin = new Vector2(0.5f, 0.5f); wlrt.anchorMax = new Vector2(0.5f, 0.5f);
            wlrt.sizeDelta = new Vector2(200, 30); wlrt.anchoredPosition = new Vector2(-120, -80);

            string[] wNames = { "Dégagé", "Pluie", "Brume" };
            for (int i = 0; i < 3; i++)
            {
                int idx = i;
                WeatherType wt = (WeatherType)idx;
                var btn = UiFactory.Button(Root.transform, "Weather_" + i, wNames[i], new Vector2(110, 44), new Vector2(-120 + i * 120, -140), new Color(0.18f, 0.20f, 0.28f), Color.white, 16, () => SelectWeather(wt));
                var brt = btn.GetComponent<RectTransform>();
                brt.anchorMin = new Vector2(0.5f, 0.5f); brt.anchorMax = new Vector2(0.5f, 0.5f);
            }

            // Time selector
            var tLabel = UiFactory.Label(Root.transform, "TLabel", "Heure", 20, Color.white, TextAnchor.MiddleLeft, UiFactory.FontBold);
            var tlrt = tLabel.GetComponent<RectTransform>();
            tlrt.anchorMin = new Vector2(0.5f, 0.5f); tlrt.anchorMax = new Vector2(0.5f, 0.5f);
            tlrt.sizeDelta = new Vector2(200, 30); tlrt.anchoredPosition = new Vector2(-120, -220);

            int[] hours = { 7, 15, 19, 22 };
            string[] hourNames = { "Matin", "Après-midi", "Soir", "Nuit" };
            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                int h = hours[idx];
                var btn = UiFactory.Button(Root.transform, "Time_" + i, hourNames[i], new Vector2(110, 44), new Vector2(-180 + i * 120, -280), new Color(0.18f, 0.20f, 0.28f), Color.white, 16, () => SelectHour(h));
                var brt = btn.GetComponent<RectTransform>();
                brt.anchorMin = new Vector2(0.5f, 0.5f); brt.anchorMax = new Vector2(0.5f, 0.5f);
            }

            // Start button
            var startBtn = UiFactory.Button(Root.transform, "Start", "Démarrer le service", new Vector2(320, 70), new Vector2(0, -400), new Color(0.0f, 0.40f, 0.20f), Color.white, 24, () => OnStart());
            var sbrt = startBtn.GetComponent<RectTransform>();
            sbrt.anchorMin = new Vector2(0.5f, 0.5f); sbrt.anchorMax = new Vector2(0.5f, 0.5f);

            // Credits
            var credits = UiFactory.Label(Root.transform, "Credits", "Assets procéduraux originaux — aucune ressource propriétaire", 14, new Color(0.5f, 0.52f, 0.58f), TextAnchor.MiddleCenter, UiFactory.FontRegular);
            var crt = credits.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.5f, 0f); crt.anchorMax = new Vector2(0.5f, 0f);
            crt.pivot = new Vector2(0.5f, 0f);
            crt.sizeDelta = new Vector2(800, 30); crt.anchoredPosition = new Vector2(0, 20);
        }

        private void SelectLang(string lang)
        {
            _selectedLang = lang;
            AlgerBusSim.Localization.Localization.Language = lang;
            AlgerBusSim.Localization.Localization.Load();
            if (_title != null) _title.text = AlgerBusSim.Localization.Localization.Get("app_title");
            if (_subtitle != null) _subtitle.text = AlgerBusSim.Localization.Localization.Get("app_subtitle");
        }

        private void SelectWeather(WeatherType w) { _selectedWeather = w; }
        private void SelectHour(int h) { _selectedHour = h; }

        private void OnStart()
        {
            GameSession.Instance.StartService(_selectedHour, _selectedWeather, _selectedLang);
        }

        public void Show() { if (Root != null) Root.SetActive(true); }
        public void Hide() { if (Root != null) Root.SetActive(false); }
    }
}
