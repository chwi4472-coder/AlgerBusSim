using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace AlgerBusSim.Art
{
    public static class TextureLibrary
    {
        private static readonly Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>();

        public static Texture2D Get(string key, QualityTier tier)
        {
            string high = "art/" + key + ".png";
            string low = "art/low/" + key + "_low.png";
            Texture2D tex = null;
            if (tier == QualityTier.Low)
            {
                tex = Load(low, key);
            }
            else
            {
                tex = Load(high, key);
            }
            return tex;
        }

        private static Texture2D Load(string relative, string key)
        {
            Texture2D cached;
            if (Cache.TryGetValue(key, out cached)) return cached;

            string full = Application.streamingAssetsPath + "/" + relative;
            var request = UnityWebRequestTexture.GetTexture(full, 1024);
            var operation = request.SendWebRequest();
            while (!request.isDone) { }
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("[AlgerBusSim] Failed to load texture " + key + ": " + request.error);
                return null;
            }

            Texture2D tex = DownloadHandlerTexture.GetContent(request.downloadHandler) as Texture2D;
            if (tex == null) tex = request.texture;
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.anisoLevel = 8;
            tex.mipChain = true;
            tex.mipMapPriority = 0;
            Cache[key] = tex;
            return tex;
        }
    }
}
