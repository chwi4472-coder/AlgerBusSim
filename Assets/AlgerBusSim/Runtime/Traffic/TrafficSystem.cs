using AlgerBusSim.Art;
using System.Collections.Generic;
using UnityEngine;
using AlgerBusSim.World;
using AlgerBusSim.Data;

namespace AlgerBusSim.Traffic
{
    public class TrafficCar : MonoBehaviour
    {
        public RoadGraph Graph;
        public RoadEdge CurrentEdge;
        public float T;
        public float Speed;
        public float TargetSpeed;
        public float LaneOffset; // +1 right side
        public Color BodyColor;
        public TrafficLightSystem Lights;

        private List<Vector3> _sampled;

        public void Init(RoadGraph graph, RoadEdge edge, TrafficLightSystem lights)
        {
            Graph = graph;
            CurrentEdge = edge;
            Lights = lights;
            T = Random.Range(0.05f, 0.85f);
            TargetSpeed = Random.Range(8f, 13f);
            Speed = TargetSpeed * 0.5f;
            LaneOffset = Random.value < 0.5f ? 1f : -1f;
            _sampled = graph.SampleEdge(edge);
        }

        void Update()
        {
            if (Graph == null || CurrentEdge == null) return;
            if (_sampled == null || _sampled.Count < 2) _sampled = Graph.SampleEdge(CurrentEdge);

            // speed control
            TargetSpeed = Mathf.Clamp(TargetSpeed, 6f, 14f);
            // red light check near junction
            Vector3 pos = Graph.PointAt(CurrentEdge, T, _sampled);
            Vector3 dir = Graph.DirectionAt(CurrentEdge, T, _sampled);
            Vector3 right = new Vector3(dir.z, 0f, -dir.x);
            // raycast ahead for obstacles
            float ahead = 12f;
            Vector3 origin = pos + Vector3.up * 1.2f;
            if (Physics.SphereCast(origin, 1.0f, dir, out RaycastHit hit, ahead))
            {
                if (hit.distance < ahead) TargetSpeed = 0f;
            }
            // red light
            if (Lights != null && T > 0.85f)
            {
                if (Lights.IsRedFor(pos, dir)) TargetSpeed = 0f;
            }

            Speed = Mathf.MoveTowards(Speed, TargetSpeed, 3f * Time.deltaTime);
            if (Speed < 0.5f) Speed = 0.5f;

            float edgeLen = Graph.EdgeLength(CurrentEdge);
            if (edgeLen < 0.1f) edgeLen = 1f;
            T += (Speed * Time.deltaTime) / edgeLen;

            if (T >= 1f)
            {
                T = 0f;
                // pick next edge
                var node = Graph.GetNode(CurrentEdge.to);
                var outs = Graph.From(CurrentEdge.to);
                RoadEdge next = null;
                foreach (var e in outs)
                {
                    if (e.id != CurrentEdge.id) { next = e; break; }
                }
                if (next == null && outs.Count > 0) next = outs[0];
                if (next == null)
                {
                    // reverse
                    T = 1f;
                    // swap from/to by reusing same edge backwards: simulate by flipping T
                    var tmp = CurrentEdge.from; CurrentEdge.from = CurrentEdge.to; CurrentEdge.to = tmp;
                    _sampled = Graph.SampleEdge(CurrentEdge);
                    T = 0.99f;
                    return;
                }
                CurrentEdge = next;
                _sampled = Graph.SampleEdge(next);
            }

            pos = Graph.PointAt(CurrentEdge, T, _sampled);
            dir = Graph.DirectionAt(CurrentEdge, T, _sampled);
            right = new Vector3(dir.z, 0f, -dir.x);
            Vector3 lanePos = pos + right * (LaneOffset * CurrentEdge.width * 0.22f);
            lanePos.y = 0.0f;
            transform.position = Vector3.Lerp(transform.position, lanePos, Time.deltaTime * 8f);
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 6f);
        }
    }

    public class TrafficSystem : MonoBehaviour
    {
        public RoadGraph Graph;
        public TrafficLightSystem Lights;
        public int CarCount = 16;
        public GameObject CarPrefabRoot;

        private readonly List<TrafficCar> _cars = new List<TrafficCar>();

        public void Init(RoadGraph graph, TrafficLightSystem lights)
        {
            Graph = graph;
            Lights = lights;
            SpawnCars();
        }

        private void SpawnCars()
        {
            if (Graph == null) return;
            var edges = Graph.Edges;
            for (int i = 0; i < CarCount; i++)
            {
                RoadEdge e = edges[Random.Range(0, edges.Count)];
                if (e.kind != "street" && e.kind != "boulevard") continue;
                var car = CreateCar();
                car.transform.SetParent(transform, false);
                var tc = car.AddComponent<TrafficCar>();
                tc.Init(Graph, e, Lights);
                _cars.Add(tc);
            }
        }

        private GameObject CreateCar()
        {
            var go = new GameObject("TrafficCar");
            // body
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.SetParent(go.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            body.transform.localScale = new Vector3(1.7f, 1.0f, 4.0f);
            UnityEngine.Object.Destroy(body.GetComponent<Collider>());
            Color[] colors = { Color.white, new Color(0.7f,0.7f,0.72f), new Color(0.85f,0.15f,0.15f), new Color(0.15f,0.3f,0.7f), new Color(0.95f,0.8f,0.1f) };
            body.GetComponent<Renderer>().sharedMaterial = WorldMaterials.Solid(colors[Random.Range(0, colors.Length)], 0.3f);

            var cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabin.transform.SetParent(go.transform, false);
            cabin.transform.localPosition = new Vector3(0f, 1.35f, -0.3f);
            cabin.transform.localScale = new Vector3(1.55f, 0.7f, 2.4f);
            UnityEngine.Object.Destroy(cabin.GetComponent<Collider>());
            cabin.GetComponent<Renderer>().sharedMaterial = WorldMaterials.Glass;

            // wheels
            for (int w = 0; w < 4; w++)
            {
                var wh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wh.transform.SetParent(go.transform, false);
                float x = (w % 2 == 0) ? -0.75f : 0.75f;
                float z = (w < 2) ? 1.2f : -1.2f;
                wh.transform.localPosition = new Vector3(x, 0.28f, z);
                wh.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                wh.transform.localScale = new Vector3(0.28f, 0.28f, 0.28f);
                UnityEngine.Object.Destroy(wh.GetComponent<Collider>());
                wh.GetComponent<Renderer>().sharedMaterial = WorldMaterials.Dark;
            }

            // collider
            var col = go.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.6f, 0f);
            col.size = new Vector3(1.7f, 1.2f, 4.0f);
            var rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            return go;
        }
    }
}
