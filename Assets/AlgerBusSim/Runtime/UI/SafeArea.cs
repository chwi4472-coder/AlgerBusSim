using UnityEngine;

namespace AlgerBusSim.UI
{
    public class SafeArea : MonoBehaviour
    {
        public RectTransform Target;

        void Start()
        {
            if (this.Target == null) this.Target = this.GetComponent<RectTransform>();
            if (this.Target == null) return;
            Rect safe = Screen.safeArea;
            Vector2 anchorMin = safe.position;
            Vector2 anchorMax = safe.position + safe.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            this.Target.anchorMin = anchorMin;
            this.Target.anchorMax = anchorMax;
        }
    }
}
