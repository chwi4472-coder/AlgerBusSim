using System.Collections.Generic;
using UnityEngine;

namespace AlgerBusSim.World
{
    public class RoadGraph
    {
        public Dictionary<string, AlgerBusSim.Data.RoadNode> Nodes =
            new Dictionary<string, AlgerBusSim.Data.RoadNode>();

        public List<AlgerBusSim.Data.RoadEdge> Edges =
            new List<AlgerBusSim.Data.RoadEdge>();

        public List<AlgerBusSim.Data.Stop> Stops =
            new List<AlgerBusSim.Data.Stop>();

        public Dictionary<string, AlgerBusSim.Data.RoadEdge> EdgeById =
            new Dictionary<string, AlgerBusSim.Data.RoadEdge>();

        public Dictionary<string, List<AlgerBusSim.Data.RoadEdge>> Outbound =
            new Dictionary<string, List<AlgerBusSim.Data.RoadEdge>>();

        public static RoadGraph FromData(AlgerBusSim.Data.RoadGraphData data)
        {
            var graph = new RoadGraph();
            if (data == null) return graph;
            if (data.nodes != null)
            {
                foreach (var n in data.nodes)
                {
                    if (n == null || n.id == null) continue;
                    graph.Nodes[n.id] = n;
                }
            }
            if (data.edges != null)
            {
                foreach (var e in data.edges)
                {
                    if (e == null || e.id == null) continue;
                    graph.Edges.Add(e);
                    graph.EdgeById[e.id] = e;
                    if (!graph.Outbound.ContainsKey(e.from))
                        graph.Outbound[e.from] = new List<AlgerBusSim.Data.RoadEdge>();
                    graph.Outbound[e.from].Add(e);
                }
            }
            if (data.stops != null) graph.Stops.AddRange(data.stops);
            return graph;
        }

        public AlgerBusSim.Data.RoadNode GetNode(string id)
        {
            AlgerBusSim.Data.RoadNode n;
            if (id == null || !this.Nodes.TryGetValue(id, out n)) return null;
            return n;
        }

        public AlgerBusSim.Data.RoadEdge GetEdge(string id)
        {
            AlgerBusSim.Data.RoadEdge e;
            if (id == null || !this.EdgeById.TryGetValue(id, out e)) return null;
            return e;
        }

        public List<AlgerBusSim.Data.RoadEdge> From(string id)
        {
            List<AlgerBusSim.Data.RoadEdge> list;
            if (this.Outbound.TryGetValue(id, out list)) return list;
            return new List<AlgerBusSim.Data.RoadEdge>();
        }

        public List<AlgerBusSim.Data.RoadEdge> To(string id)
        {
            var result = new List<AlgerBusSim.Data.RoadEdge>();
            foreach (var e in this.Edges)
            {
                if (e.to == id) result.Add(e);
            }
            return result;
        }

        public Vector3 NodePosition(AlgerBusSim.Data.RoadNode n)
        {
            if (n == null) return Vector3.zero;
            return new Vector3(n.x, n.y, n.z);
        }

        public Vector3 NodePosition(string id)
        {
            return this.NodePosition(this.GetNode(id));
        }

        /// <summary>
        /// Samples an edge as a polyline. Edges may carry authored control
        /// points; those are interpolated with a uniform Catmull-Rom spline so
        /// the coastal boulevard curves with the bay instead of running as a
        /// ruler-straight line.
        /// </summary>
        public List<Vector3> SampleEdge(AlgerBusSim.Data.RoadEdge edge)
        {
            var pts = new List<Vector3>();
            var a = this.GetNode(edge.from);
            var b = this.GetNode(edge.to);
            if (a == null || b == null) return pts;

            var control = new List<Vector3>();
            control.Add(this.NodePosition(a));
            if (edge.control != null)
            {
                for (int i = 0; i < edge.control.Count; i++)
                {
                    var c = edge.control[i];
                    if (c != null) control.Add(new Vector3(c.x, a.y, c.z));
                }
            }
            control.Add(this.NodePosition(b));

            if (control.Count == 2)
            {
                pts.Add(control[0]);
                pts.Add(control[1]);
                return pts;
            }

            int n = control.Count;
            var p = new Vector3[n + 2];
            p[0] = control[0] - (control[1] - control[0]);
            for (int i = 0; i < n; i++) p[i + 1] = control[i];
            p[n + 1] = control[n - 1] + (control[n - 1] - control[n - 2]);

            pts.Add(control[0]);
            for (int i = 0; i < n - 1; i++)
            {
                Vector3 p0 = p[i], p1 = p[i + 1], p2 = p[i + 2], p3 = p[i + 3];
                for (int s = 1; s <= 8; s++)
                {
                    float t = s / 8f;
                    float t2 = t * t;
                    float t3 = t2 * t;
                    Vector3 c = 0.5f * ((2f * p1) + (-p0 + p2) * t +
                        (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                        (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
                    pts.Add(c);
                }
            }
            return pts;
        }

        public float EdgeLength(AlgerBusSim.Data.RoadEdge edge)
        {
            var pts = this.SampleEdge(edge);
            float len = 0f;
            for (int i = 1; i < pts.Count; i++) len += Vector3.Distance(pts[i - 1], pts[i]);
            return len;
        }

        /// <summary>Position at normalized distance t (0..1) along the edge.</summary>
        public Vector3 PointAt(AlgerBusSim.Data.RoadEdge edge, float t, List<Vector3> sampled = null)
        {
            var pts = sampled ?? this.SampleEdge(edge);
            if (pts.Count == 0) return Vector3.zero;
            if (pts.Count == 1) return pts[0];
            float f = Mathf.Clamp01(t) * (pts.Count - 1);
            int i0 = Mathf.FloorToInt(f);
            int i1 = Mathf.Min(i0 + 1, pts.Count - 1);
            return Vector3.Lerp(pts[i0], pts[i1], f - i0);
        }

        public Vector3 DirectionAt(AlgerBusSim.Data.RoadEdge edge, float t, List<Vector3> sampled = null)
        {
            var pts = sampled ?? this.SampleEdge(edge);
            if (pts.Count < 2) return Vector3.forward;
            int i = Mathf.Clamp(Mathf.RoundToInt(t * (pts.Count - 1)), 0, pts.Count - 2);
            Vector3 d = pts[i + 1] - pts[i];
            d.y = 0f;
            return d.sqrMagnitude > 0.0001f ? d.normalized : Vector3.forward;
        }

        public AlgerBusSim.Data.Stop StopAt(string edgeId, float t)
        {
            foreach (var s in this.Stops)
            {
                if (s != null && s.edge == edgeId) return s;
            }
            return null;
        }
    }
}
