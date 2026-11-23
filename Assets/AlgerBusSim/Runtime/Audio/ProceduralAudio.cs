using UnityEngine;

namespace AlgerBusSim.Audio
{
    public class ProceduralAudio : MonoBehaviour
    {
        public AudioSource Engine;
        public AudioSource Horn;
        public AudioSource OneShot;
        public AudioSource Rain;

        public AudioClip EngineClip;
        public AudioClip HornClip;
        public AudioClip DoorClip;
        public AudioClip BellClip;
        public AudioClip CrashClip;
        public AudioClip RainClip;

        public void Init()
        {
            EngineClip = MakeEngineClip();
            HornClip = MakeHornClip();
            DoorClip = MakeDoorClip();
            BellClip = MakeBellClip();
            CrashClip = MakeCrashClip();
            RainClip = MakeRainClip();

            if (Engine == null) { var go = new GameObject("EngineAudio"); go.transform.SetParent(transform); Engine = go.AddComponent<AudioSource>(); }
            Engine.loop = true; Engine.clip = EngineClip; Engine.spatialBlend = 0.25f; Engine.volume = 0.55f; Engine.Play();

            if (Horn == null) { var go = new GameObject("HornAudio"); go.transform.SetParent(transform); Horn = go.AddComponent<AudioSource>(); }
            Horn.loop = false; Horn.clip = HornClip; Horn.spatialBlend = 0.4f; Horn.volume = 0.8f;

            if (OneShot == null) { var go = new GameObject("OneShotAudio"); go.transform.SetParent(transform); OneShot = go.AddComponent<AudioSource>(); }
            OneShot.spatialBlend = 0.5f; OneShot.volume = 0.7f;

            if (Rain == null) { var go = new GameObject("RainAudio"); go.transform.SetParent(transform); Rain = go.AddComponent<AudioSource>(); }
            Rain.loop = true; Rain.clip = RainClip; Rain.spatialBlend = 0f; Rain.volume = 0f; Rain.Play();
        }

        public void UpdateEngine(float rpmNorm, float throttle)
        {
            if (Engine == null) return;
            Engine.pitch = 0.55f + rpmNorm * 1.6f + throttle * 0.15f;
            Engine.volume = 0.45f + throttle * 0.2f;
        }

        public void PlayHorn() { if (Horn != null && HornClip != null) Horn.PlayOneShot(HornClip); }
        public void PlayDoor() { if (OneShot != null && DoorClip != null) OneShot.PlayOneShot(DoorClip); }
        public void PlayBell() { if (OneShot != null && BellClip != null) OneShot.PlayOneShot(BellClip); }
        public void PlayCrash() { if (OneShot != null && CrashClip != null) OneShot.PlayOneShot(CrashClip); }
        public void SetRainVolume(float v) { if (Rain != null) Rain.volume = Mathf.Clamp01(v); }

        private static AudioClip MakeEngineClip()
        {
            int sr = 44100; int len = sr;
            var clip = AudioClip.Create("Engine", len, 1, sr, true);
            float[] data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = i / (float)sr;
                float s = 0f;
                s += Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.35f;
                s += Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.2f;
                s += Mathf.Sin(2f * Mathf.PI * 165f * t) * 0.12f;
                s += (Random.value * 2f - 1f) * 0.04f;
                data[i] = s * 0.6f;
            }
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip MakeHornClip()
        {
            int sr = 44100; int len = (int)(sr * 0.7f);
            var clip = AudioClip.Create("Horn", len, 1, sr, false);
            float[] data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = i / (float)sr;
                float env = Mathf.Clamp01(1f - t / 0.7f) * Mathf.Clamp01(t * 20f);
                float s = Mathf.Sin(2f * Mathf.PI * 420f * t) + Mathf.Sin(2f * Mathf.PI * 560f * t) * 0.7f;
                data[i] = s * 0.35f * env;
            }
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip MakeDoorClip()
        {
            int sr = 44100; int len = (int)(sr * 0.5f);
            var clip = AudioClip.Create("Door", len, 1, sr, false);
            float[] data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = i / (float)sr;
                float env = Mathf.Clamp01(1f - t / 0.5f);
                float s = (Random.value * 2f - 1f) * 0.5f * env;
                s += Mathf.Sin(2f * Mathf.PI * 180f * t) * 0.2f * env;
                data[i] = s * 0.5f;
            }
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip MakeBellClip()
        {
            int sr = 44100; int len = (int)(sr * 0.8f);
            var clip = AudioClip.Create("Bell", len, 1, sr, false);
            float[] data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = i / (float)sr;
                float env = Mathf.Exp(-t * 4f);
                float s = Mathf.Sin(2f * Mathf.PI * 880f * t) * env;
                data[i] = s * 0.4f;
            }
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip MakeCrashClip()
        {
            int sr = 44100; int len = (int)(sr * 0.4f);
            var clip = AudioClip.Create("Crash", len, 1, sr, false);
            float[] data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float t = i / (float)sr;
                float env = Mathf.Clamp01(1f - t / 0.4f);
                data[i] = (Random.value * 2f - 1f) * 0.7f * env;
            }
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip MakeRainClip()
        {
            int sr = 44100; int len = sr * 2;
            var clip = AudioClip.Create("Rain", len, 1, sr, true);
            float[] data = new float[len];
            for (int i = 0; i < len; i++)
            {
                float n = Random.value * 2f - 1f;
                data[i] = n * 0.15f;
            }
            clip.SetData(data, 0);
            return clip;
        }
    }
}
