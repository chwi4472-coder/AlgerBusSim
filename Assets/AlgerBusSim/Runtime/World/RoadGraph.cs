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
                    if (!graph.Outbound.ContainsKey(e.from)) graph.Outbound[e.from] = new List<AlgerBusSim.Data.RoadEdge>();
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

        public List<AlgerBusSim.Data.RoadEdge> From(string id)
        {
            List<AlgerBusSim.Data.RoadEdge> list = new List<AlgerBusSim.Data.RoadEdge>();
            List<AlgerBusSim.Data.RoadEdge> empty;
            if (this.Outbound.TryGetValue(id, out empty)) list = empty;
            return list;
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

        public AlgerBusSim.Data.Stop StopAt(string edgeId, float t)
        {
            foreach (var s in this.Stops)
            {
                if (s != null && s.edge == edgeId) return s;
            }
            return null;
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

        public List<Vector3> SampleEdge(AlgerBusSim.Data.RoadEdge edge)
        {
            var pts = new List<Vector3>();
            var a = this.GetNode(edge.from);
            var b = this.GetNode(edge.to);
            if (a == null || b == null) return pts;
            Vector3[] control = new Vector3[] { this.NodePosition(a), this.NodePosition(b) };
            if (control.Length == 2)
            {
                pts.Add(control[0]);
                pts.Add(control[1]);
                return pts;
            }
            // uniform Catmull-Rom over control polyline
            int n = control.Length;
            Vector3[] p = new Vector3[n];
            for (int i = 0; i < n; i++) p[i] = control[i];
            Vector3[] q = new Vector3[n * 9 + 1];
            int count = 0;
            for (int i = 1; i < n - 1; i++)
            {
                Vector3 p0 = p[i - 1], p1 = p[i], p2 = p[i + 1], p3 = p[i + 2];
                for (int s = 0; s <= 8; s++)
                {
                    float t = s / 8f;
                    float t2 = t * t;
                    float t3 = t2 * t;
                    Vector3 c = 0.5f * ((2.0f * p1) + (-p0 + p2) * t + (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * t2 + (-p0 + 3.0f * p1 - 3.0f * p2 + p3) * t3);
                    q[count] = c;
                    count++;
                }
            }
            q[count] = p[n - 1];
            for (int i = 0; i <= count; i++) pts.Add(q[i]);
            return pts;
        }
    }
}
