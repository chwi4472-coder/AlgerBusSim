using UnityEngine;

namespace AlgerBusSim.Rendering
{
    [RequireComponent(typeof(Camera))]
    public class PostEffects : MonoBehaviour
    {
        public float bloomThreshold = 0.72f;
        public float bloomIntensity = 0.4f;
        public int bloomIterations = 3;
        public float contrast = 1.12f;
        public float saturation = 1.18f;
        public float temperature = 0.06f;
        public float vignette = 0.3f;
        public float exposure = 1.05f;

        private Material _compositeMat;
        private Material _brightMat;
        private Material _blurMat;

        void Awake()
        {
            _compositeMat = new Material(Shader.Find("AlgerBusSim/Post/Composite"));
            _brightMat = new Material(Shader.Find("AlgerBusSim/Post/BrightPass"));
            _blurMat = new Material(Shader.Find("AlgerBusSim/Post/Blur"));
        }

        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (_compositeMat == null || _brightMat == null || _blurMat == null)
            {
                Graphics.Blit(src, dest);
                return;
            }

            int w = src.width / 2;
            int h = src.height / 2;
            RenderTexture bright = RenderTexture.GetTemporary(w, h, 0, src.format);
            RenderTexture blurA = RenderTexture.GetTemporary(w, h, 0, src.format);
            RenderTexture blurB = RenderTexture.GetTemporary(w, h, 0, src.format);

            // bright pass
            _brightMat.SetFloat("_Threshold", bloomThreshold);
            Graphics.Blit(src, bright, _brightMat);

            // blur iterations
            Graphics.Blit(bright, blurA);
            for (int i = 0; i < bloomIterations; i++)
            {
                float scale = 1f + i * 1.5f;
                _blurMat.SetVector("_Direction", new Vector4(1f, 0f, 0f, 0f));
                _blurMat.SetVector("_TexelSize", new Vector4(scale / w, scale / h, 0, 0));
                Graphics.Blit(blurA, blurB, _blurMat);
                _blurMat.SetVector("_Direction", new Vector4(0f, 1f, 0f, 0f));
                Graphics.Blit(blurB, blurA, _blurMat);
            }

            // composite
            _compositeMat.SetTexture("_BloomTex", blurA);
            _compositeMat.SetFloat("_BloomIntensity", bloomIntensity);
            _compositeMat.SetFloat("_Contrast", contrast);
            _compositeMat.SetFloat("_Saturation", saturation);
            _compositeMat.SetFloat("_Temperature", temperature);
            _compositeMat.SetFloat("_VignetteIntensity", vignette);
            _compositeMat.SetFloat("_VignetteSmooth", 0.45f);
            _compositeMat.SetFloat("_Exposure", exposure);
            Graphics.Blit(src, dest, _compositeMat);

            RenderTexture.ReleaseTemporary(bright);
            RenderTexture.ReleaseTemporary(blurA);
            RenderTexture.ReleaseTemporary(blurB);
        }

        void OnDestroy()
        {
            if (_compositeMat != null) Destroy(_compositeMat);
            if (_brightMat != null) Destroy(_brightMat);
            if (_blurMat != null) Destroy(_blurMat);
        }
    }
}
