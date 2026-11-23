using UnityEngine;
using AlgerBusSim.Art;

namespace AlgerBusSim.World
{
    public enum WeatherType { Clear, Rain, Fog }

    public class WeatherSystem : MonoBehaviour
    {
        public WeatherType Type = WeatherType.Clear;
        public ParticleSystem RainParticles;
        public Light Sun;
        public Material RoadMaterial;

        private Camera _cam;

        public void Init(Camera cam)
        {
            _cam = cam;
            if (RainParticles == null) RainParticles = CreateRain();
            Apply();
        }

        public void SetWeather(WeatherType w) { Type = w; Apply(); }

        private ParticleSystem CreateRain()
        {
            var go = new GameObject("Rain");
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 5f;
            main.loop = true;
            main.startLifetime = 1.2f;
            main.startSpeed = 28f;
            main.startSize = 0.06f;
            main.startColor = new Color(0.8f, 0.85f, 0.95f, 0.45f);
            main.maxParticles = 4000;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(70f, 1f, 70f);
            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            vel.y = -28f;
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 2.5f;
            renderer.material = new Material(Shader.Find("AlgerBusSim/Unlit/Transparent"));
            var tex = new Texture2D(4, 16, TextureFormat.RGBA32, false);
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 4; x++)
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, y < 2 ? 0f : 0.6f));
            tex.Apply();
            renderer.material.mainTexture = tex;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return ps;
        }

        void LateUpdate()
        {
            if (RainParticles != null && _cam != null)
            {
                RainParticles.transform.position = _cam.transform.position + new Vector3(0f, 25f, 0f);
            }
            ApplyRuntime();
        }

        private void ApplyRuntime()
        {
            if (RainParticles == null) return;
            var emission = RainParticles.emission;
            emission.rateOverTime = (Type == WeatherType.Rain) ? 1400f : 0f;
            RainParticles.gameObject.SetActive(Type == WeatherType.Rain);
        }

        public void Apply()
        {
            ApplyRuntime();
            if (Type == WeatherType.Clear)
            {
                RenderSettings.fog = false;
            }
            else if (Type == WeatherType.Rain)
            {
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.Exponential;
                RenderSettings.fogDensity = 0.0022f;
                RenderSettings.fogColor = new Color(0.55f, 0.6f, 0.65f);
            }
            else
            {
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.Exponential;
                RenderSettings.fogDensity = 0.006f;
                RenderSettings.fogColor = new Color(0.7f, 0.72f, 0.75f);
            }
        }
    }
}
