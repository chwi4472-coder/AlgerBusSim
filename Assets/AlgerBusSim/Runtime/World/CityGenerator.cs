using System.Collections.Generic;
using UnityEngine;

namespace AlgerBusSim.World
{
    public static class CityGenerator
    {
        public static void Generate(AlgerBusSim.Data.CityData city,
                                   AlgerBusSim.Data.RoadGraphData roads,
                                   GameObject parent)
        {
            var graph = AlgerBusSim.World.RoadGraph.FromData(roads);

            var roadGo = new GameObject("Roads");
            roadGo.transform.SetParent(parent.transform);
            EmitRoads(graph, roadGo);

            var propGo = new GameObject("Props");
            propGo.transform.SetParent(parent.transform);
            ScatterProps(graph, propGo);

            var lmGo = new GameObject("Landmarks");
            lmGo.transform.SetParent(parent.transform);
            EmitLandmarks(city, graph, lmGo);
        }

        private static void EmitRoads(AlgerBusSim.World.RoadGraph graph, GameObject go)
        {
            var mesh = new Mesh();
            mesh.name = "RoadsMesh";
            var verts = new List<Vector3>();
            var tris = new List<int>();
            var uvs = new List<Vector2>();

            foreach (var e in graph.Edges)
            {
                var pts = graph.SampleEdge(e);
                if (pts.Count < 2) continue;
                float w = e.width > 0 ? e.width : 7.0f;
                int n = pts.Count;
                Vector3 dir = pts[n - 1] - pts[0];
                if (dir.sqrMagnitude > 0.0001f) dir.Normalize();
                Vector3 left = new Vector3(-dir.z, 0f, dir.x);
                for (int i = 0; i < n; i++)
                {
                    Vector3 l = pts[i] - left * (w * 0.5f);
                    Vector3 r = pts[i] + left * (w * 0.5f);
                    verts.Add(l);
                    verts.Add(r);
                    float u = n > 1 ? (float)i / (n - 1) : 0f;
                    uvs.Add(new Vector2(u, 0f));
                    uvs.Add(new Vector2(u, 1f));
                    if (i > 0)
                    {
                        tris.Add(i * 2 - 2);
                        tris.Add(i * 2 - 1);
                        tris.Add(i * 2 + 1);
                        tris.Add(i * 2 - 2);
                        tris.Add(i * 2 + 1);
                        tris.Add(i * 2);
                    }
                }
            }

            mesh.SetVertices(verts);
            mesh.SetTriangles(tris.ToArray(), 0);
            mesh.SetUVs(0, uvs.ToArray());
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            Shader sh = Shader.Find("AlgerBusSim/Unlit/Tilable");
            if (sh == null) sh = Shader.Find("Unlit/Texture");
            var mat = new Material(sh);
            mat.SetColor("_Tint", new Color(0.36f, 0.34f, 0.30f, 1f));
            mat.SetVector("_TileSize", new Vector4(1f, 1f, 0f, 0f));
            mr.sharedMaterial = mat;
        }

        private class Rng
        {
            private uint state;
            public Rng(uint seed) { this.state = seed; }
            public float Next()
            {
                this.state = this.state * 1664525u + 1013904223u;
                return (this.state - (this.state >> 16)) * (1.0f / 16777216.0f);
            }
        }

        private static void ScatterProps(AlgerBusSim.World.RoadGraph graph, GameObject go)
        {
            var rng = new Rng(0x4C41);
            int placed = 0;
            foreach (var e in graph.Edges)
            {
                if (placed >= 14) break;
                var pts = graph.SampleEdge(e);
                if (pts.Count < 2) continue;
                float t = rng.Next();
                if (t < 0f || t > 0.95f) continue;
                Vector3 p = pts[(int)(t * (pts.Count - 1))];
                float side = (float)(rng.Next() * 2.0f - 1.0f);
                Vector3 pos = new Vector3(p.x, 0f, p.z) + new Vector3(0f, 0f, side * (e.width + 2.5f));
                GameObject prop;
                float r = rng.Next();
                if (r < 0.30f) prop = CreatePrimitive(PrimitiveType.Cylinder, pos, new Vector3(0.12f, 1.6f, 0.12f));
                else if (r < 0.45f) prop = CreatePrimitive(PrimitiveType.Sphere, pos, new Vector3(0.15f, 0.45f, 0.15f));
                else if (r < 0.60f) prop = CreatePrimitive(PrimitiveType.Cube, pos, new Vector3(0.35f, 0.25f, 0.35f));
                else if (r < 0.75f) prop = CreatePrimitive(PrimitiveType.Cylinder, pos, new Vector3(0.06f, 0.9f, 0.06f));
                else prop = CreatePrimitive(PrimitiveType.Sphere, pos, new Vector3(0.35f, 0.5f, 0.35f));
                prop.transform.SetParent(go.transform);
                placed++;
            }
        }

        private static GameObject CreatePrimitive(PrimitiveType type, Vector3 pos, Vector3 scale)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.transform.position = pos;
            go.transform.localScale = scale;
            return go;
        }

        private static void EmitLandmarks(AlgerBusSim.Data.CityData city,
                                        AlgerBusSim.World.RoadGraph graph,
                                        GameObject go)
        {
            foreach (var lm in city.landmarks)
            {
                var node = graph.GetNode(lm.nodeId);
                if (node == null) continue;
                Vector3 pos = new Vector3(node.x, 0f, node.z);
                GameObject marker;
                if (lm.kind == "neo_mauritsian")
                {
                    marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    marker.transform.position = new Vector3(pos.x, pos.y + 30f, pos.z);
                    marker.transform.localScale = new Vector3(8f, 60f, 8f);
                }
                else if (lm.kind == "modern")
                {
                    marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    marker.transform.position = pos;
                    marker.transform.localScale = new Vector3(40f, 0.5f, 30f);
                }
                else
                {
                    marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    marker.transform.position = pos;
                    marker.transform.localScale = new Vector3(12f, 2f, 12f);
                }
                marker.transform.SetParent(go.transform);
            }
        }
    }
}
