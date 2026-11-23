using System;
using System.Collections.Generic;
using UnityEngine;

namespace AlgerBusSim.World
{
    /// <summary>
    /// Authored building rows for every street: Algerian facades from the
    /// texture library, ground-floor shops, balconies, parapets, water tanks
    /// and tiled villa roofs. Geometry is merged per material.
    /// </summary>
    public static class BuildingGenerator
    {
        private sealed class Batches
        {
            public readonly MeshBatch[] Facades = new MeshBatch[14];
            public readonly MeshBatch[] Shops = new MeshBatch[7];
            public readonly MeshBatch Trim = new MeshBatch();
            public readonly MeshBatch RoofFlat = new MeshBatch();
            public readonly MeshBatch RoofTile = new MeshBatch();
            public readonly MeshBatch Metal = new MeshBatch();
            public readonly MeshBatch Glass = new MeshBatch();
            public readonly MeshBatch AwningRed = new MeshBatch();
            public readonly MeshBatch AwningGreen = new MeshBatch();
            public readonly MeshBatch AwningBlue = new MeshBatch();
            public readonly MeshBatch AwningYellow = new MeshBatch();

            public Batches()
            {
                for (int i = 0; i < Facades.Length; i++) Facades[i] = new MeshBatch();
                for (int i = 0; i < Shops.Length; i++) Shops[i] = new MeshBatch();
            }

            public MeshBatch Awning(int i)
            {
                switch (i & 3)
                {
                    case 0: return AwningRed;
                    case 1: return AwningGreen;
                    case 2: return AwningBlue;
                    default: return AwningYellow;
                }
            }
        }

        private static readonly Vector2 FacadeUv = new Vector2(1f / 12f, 1f / 12f);

        public static void Build(WorldLayout layout, GameObject parent)
        {
            var graph = layout.Graph;
            var city = layout.City;
            var batches = new Batches();
            var rng = new System.Random(0x5EED);

            foreach (var e in graph.Edges)
            {
                var pts = graph.SampleEdge(e);
                if (pts.Count < 2) continue;
                float length = PolyLength(pts);
                if (length < 34f) continue;

                float half = e.width * 0.5f;
                float sw = e.sidewalk > 0f ? e.sidewalk : 2f;
                Vector3 midDir = DirectionAt(pts, 0.5f);
                Vector3 midLeft = new Vector3(-midDir.z, 0f, midDir.x);
                bool coastal = IsCoastal(e);
                int seaSide = midLeft.z > 0f ? 1 : -1;

                for (int side = -1; side <= 1; side += 2)
                {
                    if (coastal && side == seaSide) continue;
                    BuildRow(batches, city, graph, rng, pts, length, half, sw, side, 0f, true);
                    if (e.kind == "boulevard" || e.kind == "street")
                    {
                        if (rng.NextDouble() < 0.55)
                        {
                            BuildRow(batches, city, graph, rng, pts, length, half, sw, side, 13f, false);
                        }
                    }
                }
            }

            for (int d = 0; d < 7; d++)
            {
                for (int v = 0; v < 2; v++)
                {
                    var b = batches.Facades[d * 2 + v];
                    if (b.VertexCount > 0)
                    {
                        b.Commit(parent.transform, "Facades_" + d + "_" + v,
                            AlgerBusSim.Art.WorldMaterials.Facade(d, v), true, true);
                    }
                }
                var s = batches.Shops[d];
                if (s.VertexCount > 0)
                {
                    s.Commit(parent.transform, "Shops_" + d,
                        AlgerBusSim.Art.WorldMaterials.Shop(d), true, true);
                }
            }

            Commit(batches.Trim, parent, "BuildingTrim", AlgerBusSim.Art.WorldMaterials.Trim);
            Commit(batches.RoofFlat, parent, "RoofsFlat", AlgerBusSim.Art.WorldMaterials.RoofFlat);
            Commit(batches.RoofTile, parent, "RoofsTile", AlgerBusSim.Art.WorldMaterials.RoofTile);
            Commit(batches.Metal, parent, "RooftopMetal", AlgerBusSim.Art.WorldMaterials.Metal);
            Commit(batches.Glass, parent, "BalconyGlass", AlgerBusSim.Art.WorldMaterials.Glass);
            Commit(batches.AwningRed, parent, "AwningsRed", AlgerBusSim.Art.WorldMaterials.AwningRed);
            Commit(batches.AwningGreen, parent, "AwningsGreen", AlgerBusSim.Art.WorldMaterials.AwningGreen);
            Commit(batches.AwningBlue, parent, "AwningsBlue", AlgerBusSim.Art.WorldMaterials.AwningBlue);
            Commit(batches.AwningYellow, parent, "AwningsYellow", AlgerBusSim.Art.WorldMaterials.AwningYellow);

            Debug.Log("[AlgerBusSim] Buildings generated.");
        }

        private static void Commit(MeshBatch batch, GameObject parent, string name, Material mat)
        {
            if (batch.VertexCount == 0) return;
            batch.Commit(parent.transform, name, mat, true, true);
        }

        private static void BuildRow(Batches batches, AlgerBusSim.Data.CityData city, RoadGraph graph,
                                     System.Random rng,
                                     List<Vector3> pts, float length, float half, float sw,
                                     int side, float setback, bool shops)
        {
            float margin = 6f;
            float dist = margin + (float)rng.NextDouble() * 4f;
            float end = length - margin;

            while (dist < end)
            {
                float width = 9f + (float)rng.NextDouble() * 8f;
                float depth = 10f + (float)rng.NextDouble() * 6f;
                float gap = 1.2f + (float)rng.NextDouble() * 2.2f;
                if (dist + width > end) break;

                Vector3 p = PointAtDistance(pts, dist + width * 0.5f, out Vector3 dir);
                Vector3 left = new Vector3(-dir.z, 0f, dir.x);
                Vector3 outward = left * side;
                Vector3 center = p + outward * (half + sw + setback + depth * 0.5f);
                center.y = 0f;

                if (NearLandmark(city, graph, center, 26f))
                {
                    dist += width + gap;
                    continue;
                }

                int district = DistrictFor(center);
                float height = HeightFor(district, rng);
                Quaternion rot = Quaternion.LookRotation(-outward, Vector3.up);
                int variant = rng.Next(2);

                AddBuilding(batches, center, rot, width, depth, height,
                            district, variant, shops, rng, setback == 0f);

                dist += width + gap;
            }
        }

        private static void AddBuilding(Batches batches, Vector3 center, Quaternion rot,
                                        float width, float depth, float height,
                                        int district, int variant, bool shops,
                                        System.Random rng, bool streetFront)
        {
            var facade = batches.Facades[district * 2 + variant];
            facade.AddBox(center + Vector3.up * (height * 0.5f),
                new Vector3(width, height, depth), rot, FacadeUv);

            // roof slab + parapet
            float roofY = height + 0.3f;
            batches.RoofFlat.AddBox(center + Vector3.up * roofY,
                new Vector3(width + 0.5f, 0.6f, depth + 0.5f), rot, new Vector2(0.2f, 0.2f));

            float ph = 0.75f;
            float pT = 0.28f;
            float top = height + 0.6f + ph * 0.5f;
            batches.Trim.AddBox(center + rot * new Vector3(0f, top, depth * 0.5f),
                new Vector3(width, ph, pT), rot, Vector2.one);
            batches.Trim.AddBox(center + rot * new Vector3(0f, top, -depth * 0.5f),
                new Vector3(width, ph, pT), rot, Vector2.one);
            batches.Trim.AddBox(center + rot * new Vector3(width * 0.5f, top, 0f),
                new Vector3(pT, ph, depth), rot, Vector2.one);
            batches.Trim.AddBox(center + rot * new Vector3(-width * 0.5f, top, 0f),
                new Vector3(pT, ph, depth), rot, Vector2.one);

            if (streetFront)
            {
                float frontZ = depth * 0.5f + 0.04f;
                float shopHalf = width * 0.5f - 0.55f;
                if (shops && district <= 4 && width > 8f)
                {
                    float shopH = 3.35f;
                    Vector3 a = center + rot * new Vector3(-shopHalf, 0.16f, frontZ);
                    Vector3 b = center + rot * new Vector3(shopHalf, 0.16f, frontZ);
                    Vector3 c = center + rot * new Vector3(shopHalf, shopH, frontZ);
                    Vector3 d = center + rot * new Vector3(-shopHalf, shopH, frontZ);
                    float u = (shopHalf * 2f) / 6f;
                    batches.Shops[district].AddQuad(a, b, c, d,
                        new Vector2(0f, 0f), new Vector2(u, 0f),
                        new Vector2(u, 0.84f), new Vector2(0f, 0.84f));

                    var awning = batches.Awning(rng.Next(4));
                    awning.AddBox(center + rot * new Vector3(0f, shopH + 0.32f, frontZ + 0.62f),
                        new Vector3(width - 1.1f, 0.12f, 1.25f), rot, Vector2.one);
                    awning.AddBox(center + rot * new Vector3(0f, shopH + 0.06f, frontZ + 1.2f),
                        new Vector3(width - 1.1f, 0.5f, 0.06f), rot, Vector2.one);
                }
                else if (shops && district <= 4)
                {
                    batches.Trim.AddBox(center + rot * new Vector3(0f, 1.6f, frontZ + 0.02f),
                        new Vector3(width - 1f, 3.1f, 0.08f), rot, Vector2.one);
                }

                // balconies on the street facade
                int floors = Mathf.FloorToInt(height / 3.2f);
                for (int f = 2; f < floors; f++)
                {
                    if (rng.NextDouble() > 0.58) continue;
                    float y = f * 3.2f;
                    float bz = frontZ + 0.55f;
                    batches.Trim.AddBox(center + rot * new Vector3(0f, y, bz),
                        new Vector3(2.7f, 0.14f, 1.15f), rot, Vector2.one);
                    batches.Trim.AddBox(center + rot * new Vector3(0f, y + 0.5f, bz + 0.52f),
                        new Vector3(2.7f, 0.95f, 0.06f), rot, Vector2.one);
                    batches.Trim.AddBox(center + rot * new Vector3(1.32f, y + 0.5f, bz),
                        new Vector3(0.06f, 0.95f, 1.15f), rot, Vector2.one);
                    batches.Trim.AddBox(center + rot * new Vector3(-1.32f, y + 0.5f, bz),
                        new Vector3(0.06f, 0.95f, 1.15f), rot, Vector2.one);
                    batches.Glass.AddBox(center + rot * new Vector3(0f, y + 0.55f, bz + 0.5f),
                        new Vector3(2.5f, 0.8f, 0.03f), rot, Vector2.one);
                }
            }

            // rooftop clutter
            if (rng.NextDouble() < 0.45)
            {
                Vector3 local = new Vector3(
                    (float)(rng.NextDouble() - 0.5) * (width - 3f),
                    height + 1.35f,
                    (float)(rng.NextDouble() - 0.5) * (depth - 3f));
                batches.Metal.AddCylinder(center + rot * (local - Vector3.up * 0.7f),
                    0.62f, 1.4f, 8, rot, new Vector2(0.6f, 0.6f), true);
            }
            if (rng.NextDouble() < 0.4)
            {
                Vector3 local = new Vector3(
                    (float)(rng.NextDouble() - 0.5) * (width - 4f),
                    height + 1.0f,
                    (float)(rng.NextDouble() - 0.5) * (depth - 4f));
                batches.Metal.AddBox(center + rot * local,
                    new Vector3(1.5f, 0.9f, 1.1f), rot, Vector2.one);
            }
            if (rng.NextDouble() < 0.3)
            {
                batches.Trim.AddBox(center + rot * new Vector3(0f, height + 1.7f, 0f),
                    new Vector3(2.6f, 2.4f, 2.6f), rot, Vector2.one);
            }

            // Algerian blue/green shutters on street facade
            if (streetFront && district <= 4)
            {
                int floors = Mathf.FloorToInt(height / 3.2f);
                for (int f = 1; f < floors; f++)
                {
                    if (rng.NextDouble() > 0.5) continue;
                    float y = f * 3.2f + 1.6f;
                    float frontZ = depth * 0.5f + 0.06f;
                    int nWin = Mathf.Max(1, Mathf.FloorToInt(width / 3f));
                    for (int wI = 0; wI < nWin; wI++)
                    {
                        if (rng.NextDouble() > 0.6) continue;
                        float wx = -width * 0.5f + 1.5f + wI * 3f;
                        var shutterMat = (rng.NextDouble() < 0.5) ? batches.AwningBlue : batches.AwningGreen;
                        shutterMat.AddBox(center + rot * new Vector3(wx - 0.55f, y, frontZ),
                            new Vector3(0.5f, 1.2f, 0.05f), rot, Vector2.one);
                        shutterMat.AddBox(center + rot * new Vector3(wx + 0.55f, y, frontZ),
                            new Vector3(0.5f, 1.2f, 0.05f), rot, Vector2.one);
                    }
                }
            }

            // satellite dishes on roof (very Algerian)
            if (rng.NextDouble() < 0.55)
            {
                Vector3 dishPos = center + rot * new Vector3(
                    (float)(rng.NextDouble() - 0.5) * (width - 2f),
                    height + 1.0f,
                    (float)(rng.NextDouble() - 0.5) * (depth - 2f));
                batches.Metal.AddCylinder(dishPos, 0.04f, 0.8f, 5, rot, Vector2.one, false);
                batches.Metal.AddDisc(dishPos + rot * Vector3.up * 0.8f, 0.45f, 8,
                    Quaternion.Euler(60f, 0f, 0f) * rot, Vector2.one, true);
            }

            // tiled roofs on the low hillside houses
            if (district >= 5 && height < 14f && rng.NextDouble() < 0.6)
            {
                batches.RoofTile.AddBox(center + Vector3.up * (height + 1.1f),
                    new Vector3(width + 1.2f, 1.2f, depth + 1.2f), rot, new Vector2(0.5f, 0.5f));
                batches.RoofTile.AddBox(center + Vector3.up * (height + 2.0f),
                    new Vector3(width * 0.55f, 0.8f, depth * 0.55f), rot, new Vector2(0.5f, 0.5f));
            }
        }

        private static bool NearLandmark(AlgerBusSim.Data.CityData city, RoadGraph graph,
                                         Vector3 p, float margin)
        {
            if (city == null || city.landmarks == null) return false;
            foreach (var lm in city.landmarks)
            {
                float r = 26f;
                if (lm.kind == "neo_mauritsian") r = 85f;
                else if (lm.kind == "martyrs") r = 42f;
                else if (lm.kind == "market") r = 32f;
                else if (lm.kind == "port") r = 95f;
                else if (lm.kind == "casbah") r = 0f;
                else if (lm.kind == "hills" || lm.kind == "residential") r = 0f;
                if (r <= 0f) continue;
                var node = graph.GetNode(lm.nodeId);
                if (node == null) continue;
                Vector3 np = new Vector3(node.x, 0f, node.z);
                if ((np - p).sqrMagnitude < (r + margin) * (r + margin)) return true;
            }
            return false;
        }

        private static int DistrictFor(Vector3 p)
        {
            if (p.z < -820f) return p.x > 0f ? 5 : 6;
            if (p.x < -820f) return 0;
            if (p.x < -300f) return 1;
            if (p.x < 700f) return 2;
            if (p.x < 1100f) return 3;
            return 4;
        }

        private static float HeightFor(int district, System.Random rng)
        {
            switch (district)
            {
                case 0: return 12f + (float)rng.NextDouble() * 10f;
                case 1: return 9f + (float)rng.NextDouble() * 7f;
                case 2:
                    if (rng.NextDouble() < 0.18) return 30f + (float)rng.NextDouble() * 8f;
                    return 16f + (float)rng.NextDouble() * 12f;
                case 3: return 10f + (float)rng.NextDouble() * 8f;
                case 4: return 9f + (float)rng.NextDouble() * 7f;
                case 5: return 9f + (float)rng.NextDouble() * 6f;
                default: return 12f + (float)rng.NextDouble() * 10f;
            }
        }

        private static bool IsCoastal(AlgerBusSim.Data.RoadEdge e)
        {
            return e.kind == "boulevard" && (e.id == "e01" || e.id == "e02" || e.id == "e03" ||
                e.id == "e04" || e.id == "e05" || e.id == "e06" || e.id == "e07" || e.id == "e08");
        }

        private static float PolyLength(List<Vector3> pts)
        {
            float len = 0f;
            for (int i = 1; i < pts.Count; i++) len += Vector3.Distance(pts[i - 1], pts[i]);
            return len;
        }

        private static Vector3 DirectionAt(List<Vector3> pts, float t)
        {
            int i = Mathf.Clamp(Mathf.RoundToInt(t * (pts.Count - 1)), 0, pts.Count - 2);
            Vector3 d = pts[i + 1] - pts[i];
            d.y = 0f;
            return d.sqrMagnitude > 0.0001f ? d.normalized : Vector3.forward;
        }

        private static Vector3 PointAtDistance(List<Vector3> pts, float dist, out Vector3 dir)
        {
            float acc = 0f;
            for (int i = 1; i < pts.Count; i++)
            {
                Vector3 a = pts[i - 1];
                Vector3 b = pts[i];
                float seg = Vector3.Distance(a, b);
                if (acc + seg >= dist || i == pts.Count - 1)
                {
                    float t = seg > 0.0001f ? Mathf.Clamp01((dist - acc) / seg) : 0f;
                    dir = (b - a);
                    dir.y = 0f;
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
