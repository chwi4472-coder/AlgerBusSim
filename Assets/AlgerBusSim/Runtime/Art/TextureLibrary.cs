using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace AlgerBusSim.Art
{
    /// <summary>
    /// Loads the authored PNG library from StreamingAssets. On Android the
    /// assets live inside the APK, so every texture is fetched through
    /// UnityWebRequest and preloaded once during the loading screen.
    /// </summary>
    public static class TextureLibrary
    {
        private static readonly Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>();
        private static Texture2D _fallback;

        public static bool IsLoaded(string key)
        {
            return Cache.ContainsKey(key);
        }

        public static IEnumerator Preload(string[] keys, Action<float> progress)
        {
            int total = Mathf.Max(1, keys.Length);
            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                if (!Cache.ContainsKey(key))
                {
                    string relative = "art/" + key + ".png";
                    string full = Application.streamingAssetsPath + "/" + relative;
                    using (var request = UnityWebRequestTexture.GetTexture(full))
                    {
                        yield return request.SendWebRequest();
                        if (request.result == UnityWebRequest.Result.Success)
                        {
                            var handler = request.downloadHandler as DownloadHandlerTexture;
                            Texture2D tex = handler != null ? handler.texture : null;
                            if (tex != null)
                            {
                                tex.filterMode = FilterMode.Bilinear;
                                tex.wrapMode = TextureWrapMode.Repeat;
                                tex.anisoLevel = 8;
                                Cache[key] = tex;
                            }
                        }
                        else
                        {
                            Debug.LogWarning("[AlgerBusSim] Texture missing: " + key + " (" + request.error + ")");
                        }
                    }
                }
                if (progress != null) progress((i + 1f) / total);
            }
        }

        public static Texture2D Get(string key, QualityTier tier)
        {
            Texture2D tex;
            if (Cache.TryGetValue(key, out tex)) return tex;
            tex = LoadBlocking(key);
            if (tex != null)
            {
                Cache[key] = tex;
                return tex;
            }
            return Fallback();
        }

        private static Texture2D LoadBlocking(string key)
        {
            string full = Application.streamingAssetsPath + "/art/" + key + ".png";
            using (var request = UnityWebRequestTexture.GetTexture(full))
            {
                var op = request.SendWebRequest();
                while (!op.isDone) { }
                if (request.result != UnityWebRequest.Result.Success) return null;
                var handler = request.downloadHandler as DownloadHandlerTexture;
                Texture2D tex = handler != null ? handler.texture : null;
                if (tex != null)
                {
                    tex.filterMode = FilterMode.Bilinear;
                    tex.wrapMode = TextureWrapMode.Repeat;
                    tex.anisoLevel = 8;
                }
                return tex;
            }
        }

        public static Texture2D Fallback()
        {
            if (_fallback != null) return _fallback;
            _fallback = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            Color a = new Color(0.85f, 0.80f, 0.70f);
            Color b = new Color(0.55f, 0.50f, 0.45f);
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    _fallback.SetPixel(x, y, ((x + y) & 1) == 0 ? a : b);
                }
            }
            _fallback.Apply();
            return _fallback;
        }
    }
}
