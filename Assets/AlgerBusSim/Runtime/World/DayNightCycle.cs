using UnityEngine;
using AlgerBusSim.Art;

namespace AlgerBusSim.World
{
    public class DayNightCycle : MonoBehaviour
    {
        public Light Sun;
        public float Hour = 7f;
        public float TimeScale = 1f; // game hours per real second? set by caller
        public bool UseExternalHour = false;

        public Color SkyTopDay = Palette.FromHex(Palette.SkyZenith);
        public Color SkyHorizonDay = Palette.FromHex(Palette.SkyHorizon);
        public Color SkyTopNight = new Color(0.02f, 0.04f, 0.10f);
        public Color SkyHorizonNight = new Color(0.06f, 0.08f, 0.16f);
        public Color GroundNight = new Color(0.05f, 0.05f, 0.07f);

        private Material _skyMat;
        private MeshRenderer _skyRenderer;
        private GameObject _skyDome;

        public void Init(Camera cam)
        {
            // sky dome
            var dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dome.name = "SkyDome";
            dome.transform.SetParent(transform, false);
            dome.transform.localScale = new Vector3(2500f, 2500f, 2500f);
            UnityEngine.Object.Destroy(dome.GetComponent<Collider>());
            _skyRenderer = dome.GetComponent<MeshRenderer>();
            _skyMat = new Material(Shader.Find("AlgerBusSim/Sky/Dome"));
            _skyRenderer.sharedMaterial = _skyMat;
            _skyRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _skyRenderer.receiveShadows = false;
            _skyDome = dome;

            if (Sun == null)
            {
                var sunGo = new GameObject("Sun");
                sunGo.transform.SetParent(transform, false);
                Sun = sunGo.AddComponent<Light>();
                Sun.type = LightType.Directional;
                Sun.shadows = LightShadows.Soft;
                Sun.shadowStrength = 0.85f;
            }
        }

        void LateUpdate()
        {
            if (_skyDome != null && Camera.main != null)
            {
                _skyDome.transform.position = Camera.main.transform.position;
            }
            if (!UseExternalHour)
            {
                Hour += Time.deltaTime * TimeScale;
                if (Hour >= 24f) Hour -= 24f;
            }
            Apply();
        }

        public void SetHour(float h) { Hour = h; UseExternalHour = true; }

        private void Apply()
        {
            float elev = Mathf.Sin((Hour - 6f) / 12f * Mathf.PI) * 70f;
            float az = (Hour - 6f) / 12f * 180f - 90f;
            if (Sun != null)
            {
                Sun.transform.rotation = Quaternion.Euler(elev, az, 0f);
                float intensity = Mathf.Clamp01(Mathf.Sin((Hour - 6f) / 12f * Mathf.PI));
                Sun.intensity = Mathf.Lerp(0.05f, 1.25f, intensity);
                Sun.color = Color.Lerp(new Color(1f, 0.55f, 0.35f), new Color(1f, 0.96f, 0.87f),
                    Mathf.Clamp01(intensity * 1.4f));
            }

            bool night = Hour < 6f || Hour > 19.5f;
            float nightFactor = night ? 1f : 0f;
            // smooth transition
            float t = Mathf.SmoothStep(0f, 1f, night ? 1f : 0f);

            Color skyTop = Color.Lerp(SkyTopDay, SkyTopNight, t);
            Color skyHor = Color.Lerp(SkyHorizonDay, SkyHorizonNight, t);
            Color ground = Color.Lerp(Palette.FromHex(Palette.Sidewalk), GroundNight, t);

            if (_skyMat != null)
            {
                _skyMat.SetColor("_TopColor", skyTop);
                _skyMat.SetColor("_HorizonColor", skyHor);
                _skyMat.SetColor("_GroundColor", ground);
                Vector3 sunDir = Sun != null ? Sun.transform.forward : Vector3.up;
                _skyMat.SetVector("_SunDir", sunDir);
                _skyMat.SetColor("_SunColor", Sun != null ? Sun.color : Color.white);
                _skyMat.SetFloat("_SunSize", 0.035f);
                _skyMat.SetFloat("_Exposure", night ? 0.7f : 1.05f);
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Color.Lerp(new Color(0.62f, 0.72f, 0.86f), new Color(0.05f, 0.07f, 0.12f), t);
            RenderSettings.ambientEquatorColor = Color.Lerp(new Color(0.52f, 0.50f, 0.46f), new Color(0.04f, 0.05f, 0.08f), t);
            RenderSettings.ambientGroundColor = Color.Lerp(new Color(0.28f, 0.26f, 0.23f), new Color(0.02f, 0.02f, 0.03f), t);
            RenderSettings.ambientIntensity = night ? 0.35f : 1f;
        }

        public bool IsNight() { return Hour < 6f || Hour > 19.5f; }
    }
}
