using UnityEngine;

namespace AlgerBusSim.Art
{
    public static class MaterialLibrary
    {
        private static readonly System.Collections.Generic.Dictionary<string, Material> Cache
            = new System.Collections.Generic.Dictionary<string, Material>();

        public static Material Tilable(string key, Texture2D tex, Vector2 tiling, Color tint)
        {
            string shaderName = "AlgerBusSim/Unlit/Tilable";
            Material mat = GetOrCreate(shaderName, key);
            mat.SetTexture("_MainTex", tex);
            mat.SetColor("_Tint", tint);
            mat.SetVector("_TileSize", new Vector4(tiling.x, tiling.y, 0, 0));
            return mat;
        }

        public static Material Alpha(string key, Texture2D tex, Texture2D alphaMap, float smoothness, Color tint)
        {
            string shaderName = "AlgerBusSim/Unlit/Alpha";
            Material mat = GetOrCreate(shaderName, key);
            mat.SetTexture("_MainTex", tex);
            mat.SetTexture("_AlphaTex", alphaMap);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetColor("_Tint", tint);
            return mat;
        }

        public static Material BusBody(string key, Texture2D tex)
        {
            string shaderName = "AlgerBusSim/Unlit/Tilable";
            Material mat = GetOrCreate(shaderName, key);
            mat.SetTexture("_MainTex", tex);
            mat.SetColor("_Tint", Color.white);
            mat.SetVector("_TileSize", new Vector4(1, 1, 0, 0));
            return mat;
        }

        private static Material GetOrCreate(string shaderName, string key)
        {
            Material mat;
            if (!Cache.TryGetValue(key, out mat))
            {
                Shader shader = Shader.Find(shaderName);
                if (shader == null)
                {
                    Debug.LogError("[AlgerBusSim] Shader not found: " + shaderName);
                    shader = Shader.Find("Unlit/Texture");
                }
                mat = new Material(shader);
                Cache[key] = mat;
            }
            return mat;
        }
    }
}
