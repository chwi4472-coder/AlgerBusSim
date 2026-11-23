using UnityEngine;

namespace AlgerBusSim.World
{
    public class WaterAnimator : MonoBehaviour
    {
        public Material WaterMaterial;
        public float ScrollSpeed = 0.015f;
        public float WaveSpeed = 0.4f;
        private Vector2 _offset;

        void Update()
        {
            if (WaterMaterial == null) return;
            _offset += new Vector2(ScrollSpeed, ScrollSpeed * 0.6f) * Time.deltaTime;
            WaterMaterial.mainTextureOffset = _offset;
        }
    }
}
