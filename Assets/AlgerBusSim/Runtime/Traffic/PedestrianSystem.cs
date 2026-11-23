using System.Collections.Generic;
using UnityEngine;
using AlgerBusSim.Art;
using AlgerBusSim.World;

namespace AlgerBusSim.Traffic
{
    public class PedestrianAgent
    {
        public Transform Transform;
        public Vector3 A;
        public Vector3 B;
        public float Speed;
        public float Phase;
        public bool ToB;
    }

    public class PedestrianSystem : MonoBehaviour
    {
        public List<PedestrianAgent> Agents = new List<PedestrianAgent>();
        public float WalkSpeed = 1.3f;

        public void Spawn(WorldLayout layout, int count)
        {
            if (layout == null || layout.Graph == null) return;
            var edges = layout.Graph.Edges;
            var rng = new System.Random(0x7A11);
            int attempts = 0;
            while (Agents.Count < count && attempts < count * 8)
            {
                attempts++;
                var e = edges[rng.Next(edges.Count)];
                var pts = layout.Graph.SampleEdge(e);
                if (pts.Count < 2) continue;
                float total = 0f;
                for (int i = 1; i < pts.Count; i++) total += Vector3.Distance(pts[i - 1], pts[i]);
                if (total < 20f) continue;
                float s0 = (float)rng.NextDouble() * (total - 8f) + 4f;
                float s1 = s0 + (rng.NextDouble() < 0.5 ? -1f : 1f) * (6f + (float)rng.NextDouble() * 10f);
                s1 = Mathf.Clamp(s1, 4f, total - 4f);
                Vector3 p0 = PointAt(pts, s0, out Vector3 d0);
                Vector3 p1 = PointAt(pts, s1, out Vector3 d1);
                Vector3 left = new Vector3(-d0.z, 0f, d0.x);
                int side = rng.NextDouble() < 0.5 ? 1 : -1;
                if (e.id.StartsWith("e0")) side = -1; // land side (sea is +z for coastal? pick -1)
                Vector3 off = left * (side * (e.width * 0.5f + e.sidewalk * 0.55f));
                p0 += off + Vector3.up * 0.16f;
                p1 += off + Vector3.up * 0.16f;

                // build person
                var root = new GameObject("Pedestrian");
                root.transform.SetParent(transform, false);
                root.transform.position = p0;
                var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.transform.SetParent(root.transform, false);
                body.transform.localPosition = new Vector3(0f, 0.85f, 0f);
                body.transform.localScale = new Vector3(0.32f, 0.85f, 0.32f);
                UnityEngine.Object.Destroy(body.GetComponent<Collider>());
                Color[] palette = { new Color(0.18f,0.30f,0.50f), new Color(0.55f,0.18f,0.20f), new Color(0.20f,0.42f,0.30f), new Color(0.60f,0.55f,0.45f), new Color(0.35f,0.25f,0.18f) };
                body.GetComponent<Renderer>().sharedMaterial = WorldMaterials.Solid(palette[rng.Next(palette.Length)], 0.1f);
                var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                head.transform.SetParent(root.transform, false);
                head.transform.localPosition = new Vector3(0f, 1.85f, 0f);
                head.transform.localScale = new Vector3(0.24f, 0.26f, 0.24f);
                UnityEngine.Object.Destroy(head.GetComponent<Collider>());
                head.GetComponent<Renderer>().sharedMaterial = WorldMaterials.Solid(new Color(0.62f,0.46f,0.34f), 0.1f);

                var agent = new PedestrianAgent
                {
                    Transform = root.transform,
                    A = p0,
                    B = p1,
                    Speed = WalkSpeed * (0.85f + (float)rng.NextDouble() * 0.3f),
                    Phase = (float)rng.NextDouble(),
                    ToB = true
                };
                Agents.Add(agent);
            }
        }

        void Update()
        {
            foreach (var a in Agents)
            {
                if (a.Transform == null) continue;
                Vector3 target = a.ToB ? a.B : a.A;
                Vector3 to = target - a.Transform.position;
                to.y = 0f;
                if (to.sqrMagnitude < 0.04f)
                {
                    a.ToB = !a.ToB;
                    continue;
                }
                Vector3 dir = to.normalized;
                a.Transform.position += dir * a.Speed * Time.deltaTime;
                if (dir.sqrMagnitude > 0.001f)
                    a.Transform.rotation = Quaternion.Slerp(a.Transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 6f);
                // bob
                float bob = Mathf.Sin(Time.time * 8f + a.Phase) * 0.025f;
                a.Transform.position = new Vector3(a.Transform.position.x, (a.ToB ? a.B.y : a.A.y) + bob, a.Transform.position.z);
            }
        }

        private static Vector3 PointAt(List<Vector3> pts, float dist, out Vector3 dir)
        {
            float acc = 0f;
            for (int i = 1; i < pts.Count; i++)
            {
                Vector3 a = pts[i - 1]; Vector3 b = pts[i];
                float seg = Vector3.Distance(a, b);
                if (acc + seg >= dist || i == pts.Count - 1)
                {
                    float t = seg > 0.0001f ? Mathf.Clamp01((dist - acc) / seg) : 0f;
                    dir = (b - a); dir.y = 0f;
                    dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector3.forward;
                    return Vector3.Lerp(a, b, t);
                }
                acc += seg;
            }
            dir = Vector3.forward;
            return pts[pts.Count - 1];
        }
    }
}
