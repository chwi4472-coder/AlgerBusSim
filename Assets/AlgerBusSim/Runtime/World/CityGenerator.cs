using System;
using System.Collections.Generic;
using UnityEngine;
using AlgerBusSim.Art;
using AlgerBusSim.Data;

namespace AlgerBusSim.World
{
    /// <summary>
    /// Builds the playable slice of Algiers: bay, corniche, boulevard, dense
    /// quarters, landmarks, bus shelters and street furniture. All geometry is
    /// procedural and merged per material.
    /// </summary>
    public static class CityGenerator
    {
        public static WorldLayout Generate(CityData city, RoadGraphData roads, GameObject parent)
        {
            WorldMaterials.Init();
            var layout = new WorldLayout
            {
                City = city,
                Graph = RoadGraph.FromData(roads),
                Root = parent,
                WaterMaterial = WorldMaterials.Water
            };

            BuildGround(parent);
            BuildSeaAndQuay(layout, parent);
            BuildHills(layout, parent);
            BuildRoadNetwork(layout, parent);
            BuildingGenerator.Build(layout, parent);
            BuildWaterfront(layout, parent);
            BuildStops(layout, parent);
            BuildLandmarks(layout, parent);
            BuildStreetFurniture(layout, parent);
            BuildTrafficLights(layout, parent);
            ComputeRouteStart(layout);

            Debug.Log("[AlgerBusSim] City generated: " + layout.Graph.Edges.Count + " edges, " +
                      layout.Graph.Nodes.Count + " nodes, " + layout.Stops.Count + " stops.");
            return layout;
        }

        // ---------------------------------------------------------------- ground

        private static void BuildGround(GameObject parent)
        {
            var batch = new MeshBatch();
            Vector3 a = new Vector3(-2300f, -0.06f, -2100f);
            Vector3 b = new Vector3(-2300f, -0.06f, -60f);
            Vector3 c = new Vector3(2300f, -0.06f, -60f);
            Vector3 d = new Vector3(2300f, -0.06f, -2100f);
            batch.AddGroundQuad(a, b, c, d, 12f);
            batch.Commit(parent.transform, "GroundSand", WorldMaterials.Sand, false, true);
        }

        private static void BuildSeaAndQuay(WorldLayout layout, GameObject parent)
        {
            var sea = new MeshBatch();
            Vector3 a = new Vector3(-2600f, -0.55f, -58f);
            Vector3 b = new Vector3(-2600f, -0.55f, 1600f);
            Vector3 c = new Vector3(2600f, -0.55f, 1600f);
            Vector3 d = new Vector3(2600f, -0.55f, -58f);
            sea.AddGroundQuad(a, b, c, d, 26f);
            sea.Commit(parent.transform, "Sea", WorldMaterials.Water, false, false);

            var quay = new MeshBatch();
            quay.AddBox(new Vector3(0f, -0.15f, -59.5f), new Vector3(3600f, 1.1f, 2.2f),
                Quaternion.identity, new Vector2(0.2f, 0.2f));
            quay.Commit(parent.transform, "QuayWall", WorldMaterials.Concrete, true, true);

            var rail = new MeshBatch();
            for (float x = -1580f; x <= 1580f; x += 7f)
            {
                rail.AddCylinder(new Vector3(x, 0.15f, -61.4f), 0.045f, 1.05f, 6,
                    Quaternion.identity, new Vector2(1f, 1f), false);
            }
            rail.AddBox(new Vector3(0f, 1.18f, -61.4f), new Vector3(3200f, 0.07f, 0.07f),
                Quaternion.identity, Vector2.one);
            rail.AddBox(new Vector3(0f, 0.75f, -61.4f), new Vector3(3200f, 0.05f, 0.05f),
                Quaternion.identity, Vector2.one);
            rail.Commit(parent.transform, "SeaRailing", WorldMaterials.Metal, true, false);
        }

        private static void BuildHills(WorldLayout layout, GameObject parent)
        {
            var batch = new MeshBatch();
            int cols = 18;
            float x0 = -2300f;
            float x1 = 2300f;
            float[] zs = { -980f, -1160f, -1400f, -1720f, -2100f };
            float[] ys = { 0f, 26f, 64f, 108f, 140f };
            var rng = new System.Random(0x41A1);

            for (int r = 0; r < zs.Length - 1; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    float xa = Mathf.Lerp(x0, x1, (float)c / cols);
                    float xb = Mathf.Lerp(x0, x1, (float)(c + 1) / cols);
                    float ha = ys[r] + (float)rng.NextDouble() * 9f;
                    float hb = ys[r] + (float)rng.NextDouble() * 9f;
                    float hc = ys[r + 1] + (float)rng.NextDouble() * 12f;
                    float hd = ys[r + 1] + (float)rng.NextDouble() * 12f;
                    Vector3 a = new Vector3(xa, ha, zs[r]);
                    Vector3 b = new Vector3(xb, hb, zs[r]);
                    Vector3 c2 = new Vector3(xb, hc, zs[r + 1]);
                    Vector3 d = new Vector3(xa, hd, zs[r + 1]);
                    float u = Mathf.Abs(xb - xa) / 34f;
                    float v = Mathf.Abs(zs[r + 1] - zs[r]) / 34f;
                    batch.AddQuad(a, d, c2, b,
                        new Vector2(0f, 0f), new Vector2(0f, v),
                        new Vector2(u, v), new Vector2(u, 0f));
                }
            }
            batch.Commit(parent.transform, "Hills", WorldMaterials.Sand, false, true);

            // hillside villas with tiled roofs
            var villaWall = new MeshBatch();
            var villaRoof = new MeshBatch();
            var rng2 = new System.Random(0x77C1);
            for (int i = 0; i < 34; i++)
            {
                float x = -1900f + (float)rng2.NextDouble() * 3800f;
                float z = -1010f - (float)rng2.NextDouble() * 120f;
                float y = 12f + (float)rng2.NextDouble() * 18f;
                float w = 8f + (float)rng2.NextDouble() * 8f;
                float d = 7f + (float)rng2.NextDouble() * 6f;
                float h = 4f + (float)rng2.NextDouble() * 4f;
                Quaternion rot = Quaternion.Euler(0f, (float)rng2.NextDouble() * 360f, 0f);
                villaWall.AddBox(new Vector3(x, y + h * 0.5f, z), new Vector3(w, h, d), rot,
                    new Vector2(1f / 3.6f, 1f / 3.2f));
                villaRoof.AddBox(new Vector3(x, y + h + 0.7f, z), new Vector3(w + 1.2f, 1.4f, d + 1.2f),
                    rot, new Vector2(0.5f, 0.5f));
            }
            villaWall.Commit(parent.transform, "HillVillas", WorldMaterials.Trim, true, true);
            villaRoof.Commit(parent.transform, "HillVillaRoofs", WorldMaterials.RoofTile, true, true);

            // Maqam Echahid silhouette on the ridge
            var maqam = new MeshBatch();
            Vector3 basePos = new Vector3(0f, 74f, -1460f);
            maqam.AddBox(basePos + Vector3.up * 2.5f, new Vector3(46f, 5f, 34f),
                Quaternion.identity, new Vector2(0.2f, 0.2f));
            for (int i = -1; i <= 1; i++)
            {
                Quaternion lean = Quaternion.Euler(0f, 0f, i * 17f);
                Vector3 p = basePos + new Vector3(i * 10f, 32f, 0f);
                maqam.AddBox(p, new Vector3(5.2f, 58f, 5.2f), lean, new Vector2(0.15f, 0.15f));
            }
            maqam.Commit(parent.transform, "MaqamEchahid", WorldMaterials.Concrete, true, true);

            var flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flame.name = "MaqamFlame";
            flame.transform.SetParent(parent.transform, false);
            flame.transform.position = basePos + new Vector3(0f, 64f, 0f);
            flame.transform.localScale = new Vector3(4.5f, 6.5f, 4.5f);
            flame.GetComponent<Renderer>().sharedMaterial = WorldMaterials.Headlight;
            UnityEngine.Object.Destroy(flame.GetComponent<Collider>());
        }

        // ---------------------------------------------------------------- roads

        private static void BuildRoadNetwork(WorldLayout layout, GameObject parent)
        {
            var roads = new MeshBatch();
            var sidewalks = new MeshBatch();
            var curbs = new MeshBatch();
            var markings = new MeshBatch();
            var yellow = new MeshBatch();

            foreach (var e in layout.Graph.Edges)
            {
                var pts = layout.Graph.SampleEdge(e);
                if (pts.Count < 2) continue;
                float width = e.width > 0f ? e.width : 7f;
                float half = width * 0.5f;
                float sw = e.sidewalk > 0f ? e.sidewalk : 2f;
                float total = PolyLength(pts);
                if (total < 8f) continue;
                float dist = 0f;

                for (int i = 1; i < pts.Count; i++)
                {
                    Vector3 p0 = pts[i - 1];
                    Vector3 p1 = pts[i];
                    Vector3 d = p1 - p0;
                    float len = d.magnitude;
                    if (len < 0.01f) continue;
                    d /= len;
                    Vector3 left = new Vector3(-d.z, 0f, d.x);

                    Vector3 l0 = p0 + left * half;
                    Vector3 r0 = p0 - left * half;
                    Vector3 l1 = p1 + left * half;
                    Vector3 r1 = p1 - left * half;
                    roads.AddQuad(l0, l1, r1, r0,
                        new Vector2(0f, dist / 8f),
                        new Vector2(0f, (dist + len) / 8f),
                        new Vector2(width / 8f, (dist + len) / 8f),
                        new Vector2(width / 8f, dist / 8f));

                    bool nearEnd = dist < 11f || dist + len > total - 11f;
                    if (!nearEnd)
                    {
                        AddSidewalk(sidewalks, curbs, p0, p1, d, left, half, sw, 1);
                        AddSidewalk(sidewalks, curbs, p0, p1, d, left, half, sw, -1);
                    }
                    dist += len;
                }

                AddCenterLine(markings, pts, total);
                AddEdgeLines(markings, pts, half);
                AddCrossings(markings, pts, total, half, e);
                AddStopBoxes(yellow, layout, e, pts, half);
            }

            foreach (var node in layout.Graph.Nodes.Values)
            {
                float maxHalf = 3.5f;
                foreach (var e in layout.Graph.Edges)
                {
                    if (e.from == node.id || e.to == node.id)
                        maxHalf = Mathf.Max(maxHalf, e.width * 0.5f);
                }
                var disc = new MeshBatch();
                disc.AddDisc(new Vector3(node.x, 0.018f, node.z), maxHalf + 2.6f, 18,
                    Quaternion.identity, Vector2.one, true);
                disc.Commit(parent.transform, "Junction_" + node.id, WorldMaterials.Road, false, true);
            }

            roads.Commit(parent.transform, "RoadAsphalt", WorldMaterials.Road, false, true);
            sidewalks.Commit(parent.transform, "Sidewalks", WorldMaterials.Sidewalk, false, true);
            curbs.Commit(parent.transform, "Kerbs", WorldMaterials.Kerb, false, true);
            markings.Commit(parent.transform, "RoadMarkings", WorldMaterials.Marking, false, false);
            if (yellow.VertexCount > 0)
                yellow.Commit(parent.transform, "BusStopMarkings", WorldMaterials.MarkingYellow, false, false);
        }

        private static void AddSidewalk(MeshBatch top, MeshBatch curbs,
                                        Vector3 p0, Vector3 p1, Vector3 d, Vector3 left,
                                        float half, float sw, int side)
        {
            Vector3 outward = left * side;
            Vector3 inner0 = p0 + outward * half;
            Vector3 inner1 = p1 + outward * half;
            Vector3 outer0 = p0 + outward * (half + sw);
            Vector3 outer1 = p1 + outward * (half + sw);
            const float h = 0.16f;

            if (side > 0)
            {
                top.AddGroundQuad(inner0 + Vector3.up * h, outer0 + Vector3.up * h,
                    outer1 + Vector3.up * h, inner1 + Vector3.up * h, 4f);
                curbs.AddQuad(inner0, inner0 + Vector3.up * h, inner1 + Vector3.up * h, inner1,
                    new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f));
                curbs.AddQuad(outer0, outer1, outer1 + Vector3.up * h, outer0 + Vector3.up * h,
                    new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f));
            }
            else
            {
                top.AddGroundQuad(inner0 + Vector3.up * h, inner1 + Vector3.up * h,
                    outer1 + Vector3.up * h, outer0 + Vector3.up * h, 4f);
                curbs.AddQuad(inner0, inner1, inner1 + Vector3.up * h, inner0 + Vector3.up * h,
                    new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f));
                curbs.AddQuad(outer0, outer0 + Vector3.up * h, outer1 + Vector3.up * h, outer1,
                    new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f));
            }
        }

        private static void AddCenterLine(MeshBatch batch, List<Vector3> pts, float total)
        {
            float step = 9f;
            float dash = 3.4f;
            for (float s = 6f; s + dash < total - 6f; s += step)
            {
                Vector3 p = PointAtDistance(pts, s, out Vector3 d);
                Vector3 left = new Vector3(-d.z, 0f, d.x);
                Vector3 a = p + Vector3.up * 0.035f - left * 0.09f;
                Vector3 b = a + d * dash;
                Vector3 c = b + left * 0.18f;
                Vector3 e = a + left * 0.18f;
                batch.AddQuad(a, e, c, b,
                    new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f));
            }
        }

        private static void AddEdgeLines(MeshBatch batch, List<Vector3> pts, float half)
        {
            for (int i = 1; i < pts.Count; i++)
            {
                Vector3 p0 = pts[i - 1];
                Vector3 p1 = pts[i];
                Vector3 d = p1 - p0;
                float len = d.magnitude;
                if (len < 0.01f) continue;
                d /= len;
                Vector3 left = new Vector3(-d.z, 0f, d.x);
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 a = p0 + left * (s * (half - 0.45f)) + Vector3.up * 0.032f;
                    Vector3 b = p1 + left * (s * (half - 0.45f)) + Vector3.up * 0.032f;
                    Vector3 c = b + left * (s * 0.12f);
                    Vector3 e = a + left * (s * 0.12f);
                    batch.AddQuad(a, e, c, b,
                        new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f));
                }
            }
        }

        private static void AddCrossings(MeshBatch batch, List<Vector3> pts, float total,
                                         float half, AlgerBusSim.Data.RoadEdge edge)
        {
            if (total < 26f) return;
            for (int end = 0; end < 2; end++)
            {
                float s = end == 0 ? 8.5f : total - 8.5f;
                Vector3 p = PointAtDistance(pts, s, out Vector3 d);
                Vector3 left = new Vector3(-d.z, 0f, d.x);
                for (int i = 0; i < 6; i++)
                {
                    float off = (i - 2.5f) * 0.95f;
                    Vector3 c = p + d * off + Vector3.up * 0.034f;
                    Vector3 a = c + left * (half - 0.6f);
                    Vector3 b = c - left * (half - 0.6f);
                    Vector3 cc = b + d * 0.48f;
                    Vector3 dd = a + d * 0.48f;
                    batch.AddQuad(a, dd, cc, b,
                        new Vector2(0f, 0f), new Vector2(1f, 0f),
                        new Vector2(1f, 1f), new Vector2(0f, 1f));
                }
            }
        }

        private static void AddStopBoxes(MeshBatch batch, WorldLayout layout,
                                         AlgerBusSim.Data.RoadEdge edge, List<Vector3> pts, float half)
        {
            foreach (var s in layout.Graph.Stops)
            {
                if (s.edge != edge.id) continue;
                Vector3 p = layout.Graph.PointAt(edge, s.t, pts);
                Vector3 d = layout.Graph.DirectionAt(edge, s.t, pts);
                Vector3 right = new Vector3(d.z, 0f, -d.x);
                Vector3 c = p + right * (half * 0.42f) + Vector3.up * 0.04f;
                AddRectOutline(batch, c, d, right, 13f, 2.5f, 0.16f);
            }
        }

        private static void AddRectOutline(MeshBatch batch, Vector3 center, Vector3 dir,
                                           Vector3 right, float length, float width, float t)
        {
            Vector3 f = dir * (length * 0.5f);
            Vector3 r = right * (width * 0.5f);
            AddFlatQuad(batch, center - f - r, center - f + r, center - f + r + dir * t, center - f - r + dir * t);
            AddFlatQuad(batch, center + f - r, center + f - r + dir * t, center + f + r + dir * t, center + f + r);
            AddFlatQuad(batch, center - f - r, center - f - r + dir * t, center + f - r + dir * t, center + f - r);
            AddFlatQuad(batch, center - f + r, center + f + r, center + f + r + dir * t, center - f + r + dir * t);
        }

        private static void AddFlatQuad(MeshBatch batch, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            batch.AddFlatQuadUp(a, b, c, d,
                new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f));
        }

        // ------------------------------------------------------------- waterfront

        private static void BuildWaterfront(WorldLayout layout, GameObject parent)
        {
            var promenade = new MeshBatch();
            promenade.AddGroundQuad(new Vector3(-1600f, 0.12f, -132f),
                new Vector3(-1600f, 0.12f, -64f),
                new Vector3(1600f, 0.12f, -64f),
                new Vector3(1600f, 0.12f, -132f), 4f);
            promenade.Commit(parent.transform, "Corniche", WorldMaterials.Sidewalk, false, true);

            var port = new MeshBatch();
            port.AddGroundQuad(new Vector3(540f, 0.14f, -134f),
                new Vector3(540f, 0.14f, -62f),
                new Vector3(1080f, 0.14f, -62f),
                new Vector3(1080f, 0.14f, -134f), 6f);
            port.Commit(parent.transform, "PortQuay", WorldMaterials.Concrete, false, true);

            var palms = new MeshBatch();
            var leaves = new MeshBatch();
            var benches = new MeshBatch();
            var lamps = new MeshBatch();
            var heads = new MeshBatch();

            for (float x = -1520f; x <= 1520f; x += 27f)
            {
                if (x > 520f && x < 1100f) continue;
                AddPalm(palms, leaves, new Vector3(x, 0.12f, -82f), 7.5f + Mathf.Abs(Mathf.Sin(x)) * 2f);
            }
            for (float x = -1500f; x <= 1500f; x += 46f)
            {
                if (x > 520f && x < 1100f) continue;
                AddBench(benches, new Vector3(x, 0.12f, -72f), Quaternion.Euler(0f, 180f, 0f));
            }
            for (float x = -1520f; x <= 1520f; x += 32f)
            {
                AddLamp(lamps, heads, new Vector3(x, 0.12f, -100f), Quaternion.Euler(0f, 180f, 0f));
            }
            palms.Commit(parent.transform, "CornichePalms", WorldMaterials.Trunk, true, true);
            leaves.Commit(parent.transform, "CornicheFronds", WorldMaterials.Foliage, true, true);
            benches.Commit(parent.transform, "CornicheBenches", WorldMaterials.Wood, true, true);
            lamps.Commit(parent.transform, "CornicheLamps", WorldMaterials.Metal, true, false);
            var headGo = heads.Commit(parent.transform, "CornicheLampHeads", WorldMaterials.LampOff, false, false);
            layout.LampHeads.Add(headGo.GetComponent<MeshRenderer>());

            BuildPort(layout, parent);
        }

        private static void BuildPort(WorldLayout layout, GameObject parent)
        {
            var concrete = new MeshBatch();
            var metal = new MeshBatch();
            var containers = new MeshBatch();
            var colors = new MeshBatch();

            for (int i = 0; i < 2; i++)
            {
                float x = 640f + i * 210f;
                concrete.AddBox(new Vector3(x, 15f, -92f), new Vector3(5f, 30f, 5f),
                    Quaternion.identity, new Vector2(0.2f, 0.2f));
                metal.AddBox(new Vector3(x, 30f, -74f), new Vector3(4f, 1.6f, 42f),
                    Quaternion.identity, Vector2.one);
                metal.AddBox(new Vector3(x, 24f, -60f), new Vector3(0.35f, 12f, 0.35f),
                    Quaternion.identity, Vector2.one);
            }

            var rng = new System.Random(0x9A31);
            for (int i = 0; i < 18; i++)
            {
                float x = 570f + (float)rng.NextDouble() * 470f;
                float z = -70f - (float)rng.NextDouble() * 45f;
                int stack = rng.Next(1, 4);
                for (int s = 0; s < stack; s++)
                {
                    var b = (i + s) % 3 == 0 ? colors : containers;
                    b.AddBox(new Vector3(x, 1.4f + s * 2.7f, z),
                        new Vector3(6.1f, 2.6f, 2.5f), Quaternion.identity, new Vector2(0.4f, 0.4f));
                }
            }
            concrete.Commit(parent.transform, "PortConcrete", WorldMaterials.Concrete, true, true);
            metal.Commit(parent.transform, "PortCranes", WorldMaterials.Metal, true, true);
            containers.Commit(parent.transform, "PortContainers", WorldMaterials.AwningBlue, true, true);
            colors.Commit(parent.transform, "PortContainersRed", WorldMaterials.AwningRed, true, true);

            var ship = new MeshBatch();
            ship.AddBox(new Vector3(760f, 1.2f, -22f), new Vector3(86f, 7f, 16f),
                Quaternion.identity, new Vector2(0.1f, 0.1f));
            ship.AddBox(new Vector3(792f, 8f, -22f), new Vector3(20f, 14f, 13f),
                Quaternion.identity, new Vector2(0.2f, 0.2f));
            ship.AddBox(new Vector3(760f, 5.2f, -22f), new Vector3(60f, 2.2f, 12f),
                Quaternion.identity, new Vector2(0.2f, 0.2f));
            ship.Commit(parent.transform, "PortShip", WorldMaterials.White, true, true);
        }

        // ------------------------------------------------------------------ stops

        private static void BuildStops(WorldLayout layout, GameObject parent)
        {
            var metal = new MeshBatch();
            var glass = new MeshBatch();
            var wood = new MeshBatch();
            var panels = new MeshBatch();
            var rng = new System.Random(0x51D0);

            int index = 0;
            foreach (var s in layout.Graph.Stops)
            {
                var edge = layout.Graph.GetEdge(s.edge);
                if (edge == null) continue;
                var pts = layout.Graph.SampleEdge(edge);
                if (pts.Count < 2) continue;
                Vector3 p = layout.Graph.PointAt(edge, s.t, pts);
                Vector3 dir = layout.Graph.DirectionAt(edge, s.t, pts);
                Vector3 right = new Vector3(dir.z, 0f, -dir.x);
                float half = edge.width * 0.5f;
                float sw = edge.sidewalk > 0f ? edge.sidewalk : 2f;

                var sp = new StopPoint
                {
                    Id = s.id,
                    NameFr = s.name != null ? s.name.fr : s.id,
                    NameAr = s.name != null ? s.name.ar : s.id,
                    NameDz = s.name != null ? s.name.dz : s.id,
                    BusPosition = p + right * (half * 0.42f),
                    ShelterPosition = p + right * (half + sw * 0.58f),
                    Forward = dir,
                    Index = index++
                };
                layout.Stops.Add(sp);

                Vector3 c = sp.ShelterPosition;
                c.y = 0.16f;
                Quaternion faceRoad = Quaternion.LookRotation(-right, Vector3.up);

                // shelter structure
                metal.AddBox(c + faceRoad * new Vector3(-1.9f, 2.65f, 0f), new Vector3(0.12f, 2.5f, 0.12f),
                    faceRoad, Vector2.one);
                metal.AddBox(c + faceRoad * new Vector3(1.9f, 2.65f, 0f), new Vector3(0.12f, 2.5f, 0.12f),
                    faceRoad, Vector2.one);
                metal.AddBox(c + faceRoad * new Vector3(0f, 2.72f, 0.1f), new Vector3(4.2f, 0.12f, 1.9f),
                    faceRoad, Vector2.one);
                glass.AddBox(c + faceRoad * new Vector3(0f, 1.55f, -0.85f), new Vector3(4f, 2.2f, 0.06f),
                    faceRoad, Vector2.one);
                wood.AddBox(c + faceRoad * new Vector3(0f, 0.62f, -0.55f), new Vector3(2.6f, 0.12f, 0.5f),
                    faceRoad, Vector2.one);
                wood.AddBox(c + faceRoad * new Vector3(-1.15f, 0.32f, -0.55f), new Vector3(0.1f, 0.55f, 0.4f),
                    faceRoad, Vector2.one);
                wood.AddBox(c + faceRoad * new Vector3(1.15f, 0.32f, -0.55f), new Vector3(0.1f, 0.55f, 0.4f),
                    faceRoad, Vector2.one);

                Vector3 panelPos = c + faceRoad * new Vector3(2.35f, 1.45f, -0.05f);
                AddVerticalQuad(panels, panelPos, -right, 1.05f, 2.1f, Vector2.zero, Vector2.one);

                // curb pole + sign
                Vector3 polePos = c + right * 0.2f - dir * 2.1f;
                metal.AddCylinder(polePos, 0.055f, 2.7f, 6, Quaternion.identity,
                    new Vector2(1f, 1f), false);
                AddVerticalQuad(panels, polePos + Vector3.up * 2.05f, -right, 0.8f, 1.6f,
                    Vector2.zero, Vector2.one);

                // waiting passengers
                int waiting = 3 + rng.Next(4);
                for (int i = 0; i < waiting; i++)
                {
                    Vector3 wp = c + faceRoad * new Vector3(
                        (float)(rng.NextDouble() - 0.5) * 3.4f, 0f,
                        (float)(rng.NextDouble() - 0.5) * 1.4f);
                    var person = CreatePerson(wp, 0.9f + (float)rng.NextDouble() * 0.25f, rng);
                    sp.Waiting.Add(person);
                }
            }

            metal.Commit(parent.transform, "StopShelters", WorldMaterials.Metal, true, true);
            glass.Commit(parent.transform, "StopShelterGlass", WorldMaterials.Glass, true, false);
            wood.Commit(parent.transform, "StopBenches", WorldMaterials.Wood, true, true);
            panels.Commit(parent.transform, "StopPanels", WorldMaterials.StopPanel, false, false);
        }

        private static GameObject CreatePerson(Vector3 pos, float height, System.Random rng)
        {
            var root = new GameObject("WaitingPassenger");
            root.transform.SetParent(null, false);
            root.transform.position = new Vector3(pos.x, 0.16f, pos.z);

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            body.transform.localScale = new Vector3(0.36f, height * 0.5f, 0.36f);
            UnityEngine.Object.Destroy(body.GetComponent<Collider>());
            Color[] palette =
            {
                new Color(0.18f, 0.30f, 0.50f), new Color(0.55f, 0.18f, 0.20f),
                new Color(0.20f, 0.42f, 0.30f), new Color(0.75f, 0.70f, 0.60f),
                new Color(0.35f, 0.25f, 0.18f)
            };
            body.GetComponent<Renderer>().sharedMaterial =
                WorldMaterials.Solid(palette[rng.Next(palette.Length)], 0.1f);

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0f, height + 0.12f, 0f);
            head.transform.localScale = new Vector3(0.24f, 0.26f, 0.24f);
            UnityEngine.Object.Destroy(head.GetComponent<Collider>());
            head.GetComponent<Renderer>().sharedMaterial =
                WorldMaterials.Solid(new Color(0.62f, 0.46f, 0.34f), 0.1f);
            return root;
        }

        // -------------------------------------------------------------- landmarks

        private static void BuildLandmarks(WorldLayout layout, GameObject parent)
        {
            foreach (var lm in layout.City.landmarks)
            {
                var node = layout.Graph.GetNode(lm.nodeId);
                if (node == null) continue;
                Vector3 p = new Vector3(node.x, 0f, node.z);
                switch (lm.kind)
                {
                    case "neo_mauritsian": BuildGrandePoste(layout, parent, p); break;
                    case "martyrs": BuildMartyrs(layout, parent, p); break;
                    case "market": BuildMarket(layout, parent, p); break;
                    case "casbah": BuildCasbahStairs(layout, parent, p); break;
                }
            }
        }

        private static void BuildGrandePoste(WorldLayout layout, GameObject parent, Vector3 node)
        {
            Vector3 c = node + new Vector3(0f, 0f, -46f);
            var cream = new MeshBatch();
            var stone = new MeshBatch();
            var glass = new MeshBatch();
            var roof = new MeshBatch();

            cream.AddBox(c + Vector3.up * 10f, new Vector3(64f, 20f, 26f),
                Quaternion.identity, new Vector2(0.06f, 0.06f));
            cream.AddBox(c + new Vector3(-27f, 12f, 0f), new Vector3(14f, 24f, 22f),
                Quaternion.identity, new Vector2(0.06f, 0.06f));
            cream.AddBox(c + new Vector3(27f, 12f, 0f), new Vector3(14f, 24f, 22f),
                Quaternion.identity, new Vector2(0.06f, 0.06f));

            for (int i = -3; i <= 3; i++)
            {
                float x = c.x + i * 8.5f;
                stone.AddCylinder(new Vector3(x, 0f, c.z + 13.2f), 0.62f, 11.5f, 10,
                    Quaternion.identity, new Vector2(0.6f, 0.6f), true);
                stone.AddBox(new Vector3(x, 11.7f, c.z + 13.2f), new Vector3(1.6f, 0.5f, 1.6f),
                    Quaternion.identity, Vector2.one);
            }

            for (int i = -1; i <= 1; i++)
            {
                float x = c.x + i * 17f;
                glass.AddBox(new Vector3(x, 4.5f, c.z + 13.05f), new Vector3(4.2f, 8.4f, 0.2f),
                    Quaternion.identity, Vector2.one);
                var arch = new MeshBatch();
                arch.AddCylinder(new Vector3(x, 8.7f, c.z + 13.1f), 2.1f, 0.2f, 14,
                    Quaternion.Euler(90f, 0f, 0f), new Vector2(0.5f, 0.5f), true);
                arch.Commit(parent.transform, "GrandePosteArch", WorldMaterials.Concrete, true, true);
            }

            var dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dome.name = "GrandePosteDome";
            dome.transform.SetParent(parent.transform, false);
            dome.transform.position = c + new Vector3(0f, 22.5f, 0f);
            dome.transform.localScale = new Vector3(15f, 9f, 15f);
            dome.GetComponent<Renderer>().sharedMaterial =
                WorldMaterials.Solid(new Color(0.30f, 0.45f, 0.38f), 0.55f, 0.35f);
            UnityEngine.Object.Destroy(dome.GetComponent<Collider>());

            var smallDome = new MeshBatch();
            smallDome.AddCylinder(c + new Vector3(-27f, 24f, 0f), 4.4f, 0.4f, 12,
                Quaternion.identity, Vector2.one, true);
            smallDome.AddCylinder(c + new Vector3(27f, 24f, 0f), 4.4f, 0.4f, 12,
                Quaternion.identity, Vector2.one, true);
            smallDome.Commit(parent.transform, "GrandePosteDrum", WorldMaterials.Trim, true, true);

            roof.AddBox(c + new Vector3(0f, 20.4f, 0f), new Vector3(65f, 0.8f, 27f),
                Quaternion.identity, Vector2.one);
            roof.Commit(parent.transform, "GrandePosteRoof", WorldMaterials.RoofFlat, true, true);

            var plaque = new MeshBatch();
            AddVerticalQuad(plaque, c + new Vector3(0f, 16.5f, 13.15f), Vector3.forward,
                16f, 4f, Vector2.zero, Vector2.one);
            plaque.Commit(parent.transform, "GrandePostePlaque", WorldMaterials.PlaqueGrandePoste,
                false, false);

            cream.Commit(parent.transform, "GrandePosteCream", WorldMaterials.Trim, true, true);
            stone.Commit(parent.transform, "GrandePosteStone", WorldMaterials.Concrete, true, true);
            glass.Commit(parent.transform, "GrandePosteGlass", WorldMaterials.Glass, true, false);
        }

        private static void BuildMartyrs(WorldLayout layout, GameObject parent, Vector3 node)
        {
            Vector3 c = new Vector3(0f, 0.14f, -95f);
            var plaza = new MeshBatch();
            plaza.AddDisc(c, 24f, 22, Quaternion.identity, Vector2.one, true);
            plaza.Commit(parent.transform, "MartyrsPlaza", WorldMaterials.Concrete, false, true);

            var baseBatch = new MeshBatch();
            baseBatch.AddBox(new Vector3(c.x, 1.6f, c.z), new Vector3(15f, 3f, 15f),
                Quaternion.identity, new Vector2(0.2f, 0.2f));
            baseBatch.Commit(parent.transform, "MartyrsBase", WorldMaterials.Trim, true, true);

            var towers = new MeshBatch();
            for (int i = -1; i <= 1; i++)
            {
                Quaternion lean = Quaternion.Euler(0f, 0f, i * 15f);
                towers.AddBox(new Vector3(c.x + i * 5.5f, 17f, c.z), new Vector3(2.8f, 26f, 2.8f),
                    lean, new Vector2(0.3f, 0.3f));
            }
            towers.Commit(parent.transform, "MartyrsTowers", WorldMaterials.Concrete, true, true);

            var flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flame.name = "MartyrsFlame";
            flame.transform.SetParent(parent.transform, false);
            flame.transform.position = new Vector3(c.x, 32f, c.z);
            flame.transform.localScale = new Vector3(2.4f, 3.6f, 2.4f);
            flame.GetComponent<Renderer>().sharedMaterial = WorldMaterials.Taillight;
            UnityEngine.Object.Destroy(flame.GetComponent<Collider>());

            var flags = new MeshBatch();
            for (int i = -1; i <= 1; i += 2)
            {
                Vector3 fp = new Vector3(c.x + i * 16f, 0f, c.z + 6f);
                flags.AddCylinder(fp, 0.07f, 10f, 6, Quaternion.identity, Vector2.one, false);
            }
            flags.Commit(parent.transform, "MartyrsFlagPoles", WorldMaterials.Metal, true, false);

            var cloth = new MeshBatch();
            for (int i = -1; i <= 1; i += 2)
            {
                Vector3 fp = new Vector3(c.x + i * 16f, 9.4f, c.z + 6f);
                AddVerticalQuad(cloth, fp + Vector3.right * 0.85f, Vector3.forward, 1.7f, 1.1f,
                    Vector2.zero, Vector2.one);
            }
            cloth.Commit(parent.transform, "MartyrsFlags", WorldMaterials.Flag, false, false);
        }

        private static void BuildMarket(WorldLayout layout, GameObject parent, Vector3 node)
        {
            var posts = new MeshBatch();
            var tables = new MeshBatch();
            var awnRed = new MeshBatch();
            var awnGreen = new MeshBatch();
            var crates = new MeshBatch();
            var rng = new System.Random(0x2B71);

            for (int i = 0; i < 9; i++)
            {
                float x = node.x - 40f + i * 10f;
                float z = node.z - 20f;
                Vector3 c = new Vector3(x, 0.16f, z);
                for (int px = -1; px <= 1; px += 2)
                {
                    for (int pz = -1; pz <= 1; pz += 2)
                    {
                        posts.AddCylinder(c + new Vector3(px * 1.4f, 0f, pz * 1.2f),
                            0.05f, 2.3f, 6, Quaternion.identity, Vector2.one, false);
                    }
                }
                tables.AddBox(c + new Vector3(0f, 0.85f, 0f), new Vector3(2.9f, 0.1f, 2.3f),
                    Quaternion.identity, Vector2.one);
                var awning = (i & 1) == 0 ? awnRed : awnGreen;
                awning.AddBox(c + new Vector3(0f, 2.4f, 0f), new Vector3(3.3f, 0.1f, 2.7f),
                    Quaternion.identity, Vector2.one);
                for (int k = 0; k < 4; k++)
                {
                    crates.AddBox(c + new Vector3(-1f + k * 0.7f, 1.15f, (float)(rng.NextDouble() - 0.5) * 1.2f),
                        new Vector3(0.55f, 0.45f, 0.55f), Quaternion.identity, Vector2.one);
                }
            }
            posts.Commit(parent.transform, "MarketPosts", WorldMaterials.Metal, true, false);
            tables.Commit(parent.transform, "MarketTables", WorldMaterials.Wood, true, true);
            awnRed.Commit(parent.transform, "MarketAwningsRed", WorldMaterials.AwningRed, true, false);
            awnGreen.Commit(parent.transform, "MarketAwningsGreen", WorldMaterials.AwningGreen, true, false);
            crates.Commit(parent.transform, "MarketCrates", WorldMaterials.AwningYellow, true, true);
        }

        private static void BuildCasbahStairs(WorldLayout layout, GameObject parent, Vector3 node)
        {
            var stairs = new MeshBatch();
            Vector3 start = new Vector3(node.x - 30f, 0f, node.z - 12f);
            for (int i = 0; i < 22; i++)
            {
                stairs.AddBox(start + new Vector3(0f, 0.12f + i * 0.28f, -i * 1.5f),
                    new Vector3(5.5f, 0.3f + i * 0.28f, 1.6f), Quaternion.identity, new Vector2(0.4f, 0.4f));
            }
            stairs.Commit(parent.transform, "CasbahStairs", WorldMaterials.Concrete, true, true);
        }

        // --------------------------------------------------------- street props

        private static void BuildStreetFurniture(WorldLayout layout, GameObject parent)
        {
            var trunks = new MeshBatch();
            var leaves = new MeshBatch();
            var lamps = new MeshBatch();
            var heads = new MeshBatch();
            var benches = new MeshBatch();
            var bins = new MeshBatch();
            var metal = new MeshBatch();
            var signs = new MeshBatch();

            foreach (var e in layout.Graph.Edges)
            {
                var pts = layout.Graph.SampleEdge(e);
                if (pts.Count < 2) continue;
                float total = PolyLength(pts);
                if (total < 40f) continue;
                float half = e.width * 0.5f;
                float sw = e.sidewalk > 0f ? e.sidewalk : 2f;
                bool coastal = e.id.StartsWith("e0");

                for (float s = 20f; s < total - 20f; s += 28f)
                {
                    Vector3 p = PointAtDistance(pts, s, out Vector3 d);
                    Vector3 left = new Vector3(-d.z, 0f, d.x);
                    int side = ((int)(s / 28f) & 1) == 0 ? 1 : -1;
                    if (coastal && left.z * side > 0f) continue;
                    Vector3 pos = p + left * (side * (half + sw * 0.55f));
                    pos.y = 0.16f;
                    AddLamp(lamps, heads, pos, Quaternion.LookRotation(-left * side, Vector3.up));
                }

                for (float s = 32f; s < total - 24f; s += 34f)
                {
                    Vector3 p = PointAtDistance(pts, s, out Vector3 d);
                    Vector3 left = new Vector3(-d.z, 0f, d.x);
                    int side = ((int)(s / 34f) & 1) == 0 ? -1 : 1;
                    if (coastal && left.z * side > 0f) continue;
                    Vector3 pos = p + left * (side * (half + sw * 0.72f));
                    pos.y = 0.16f;
                    if (e.kind == "boulevard") AddPalm(trunks, leaves, pos, 6.5f);
                    else AddFicus(trunks, leaves, pos);
                }

                for (float s = 55f; s < total - 30f; s += 78f)
                {
                    Vector3 p = PointAtDistance(pts, s, out Vector3 d);
                    Vector3 left = new Vector3(-d.z, 0f, d.x);
                    int side = ((int)(s / 78f) & 1) == 0 ? 1 : -1;
                    if (coastal && left.z * side > 0f) continue;
                    Vector3 pos = p + left * (side * (half + sw * 0.5f));
                    pos.y = 0.16f;
                    AddBench(benches, pos, Quaternion.LookRotation(left * side, Vector3.up));
                    bins.AddCylinder(pos + left * (side * 1.2f), 0.28f, 0.9f, 8,
                        Quaternion.identity, Vector2.one, true);
                }
            }

            // a few regulatory signs on the boulevard
            string[] signKeys = { "sign_speed50", "sign_stop", "sign_crossing", "sign_busstop" };
            var rng = new System.Random(0x6A2D);
            foreach (var e in layout.Graph.Edges)
            {
                if (e.kind != "boulevard") continue;
                var pts = layout.Graph.SampleEdge(e);
                if (pts.Count < 2) continue;
                float total = PolyLength(pts);
                for (int i = 0; i < 2; i++)
                {
                    float s = 30f + i * (total - 60f);
                    if (s <= 10f) continue;
                    Vector3 p = PointAtDistance(pts, s, out Vector3 d);
                    Vector3 left = new Vector3(-d.z, 0f, d.x);
                    int side = i == 0 ? 1 : -1;
                    Vector3 pos = p + left * (side * (e.width * 0.5f + e.sidewalk * 0.5f));
                    pos.y = 0.16f;
                    metal.AddCylinder(pos, 0.05f, 2.6f, 6, Quaternion.identity, Vector2.one, false);
                    string key = signKeys[rng.Next(signKeys.Length)];
                    AddSign(signs, pos + Vector3.up * 2.35f, -left * side, key);
                }
            }

            trunks.Commit(parent.transform, "TreeTrunks", WorldMaterials.Trunk, true, true);
            leaves.Commit(parent.transform, "TreeLeaves", WorldMaterials.Foliage, true, true);
            lamps.Commit(parent.transform, "StreetLamps", WorldMaterials.Metal, true, false);
            var headGo = heads.Commit(parent.transform, "StreetLampHeads", WorldMaterials.LampOff, false, false);
            layout.LampHeads.Add(headGo.GetComponent<MeshRenderer>());
            benches.Commit(parent.transform, "Benches", WorldMaterials.Wood, true, true);
            bins.Commit(parent.transform, "Bins", WorldMaterials.AwningGreen, true, true);
            metal.Commit(parent.transform, "SignPoles", WorldMaterials.Metal, true, false);
            signs.Commit(parent.transform, "StreetSigns", WorldMaterials.Signs, false, false);
        }

        private static void AddPalm(MeshBatch trunks, MeshBatch leaves, Vector3 pos, float height)
        {
            trunks.AddCylinder(pos, 0.19f, height, 7,
                Quaternion.Euler(0f, 0f, 3f), new Vector2(1f, 1f), false);
            Vector3 top = pos + new Vector3(0.35f, height, 0f);
            for (int i = 0; i < 8; i++)
            {
                float a = i * 45f;
                Quaternion rot = Quaternion.Euler(0f, a, -18f);
                leaves.AddBox(top + rot * new Vector3(0f, 0f, 1.6f),
                    new Vector3(0.55f, 0.06f, 3.4f), rot, Vector2.one);
            }
        }

        private static void AddFicus(MeshBatch trunks, MeshBatch leaves, Vector3 pos)
        {
            trunks.AddCylinder(pos, 0.16f, 3.2f, 6, Quaternion.identity, Vector2.one, false);
            leaves.AddBox(pos + Vector3.up * 4.2f, new Vector3(3.4f, 2.6f, 3.4f),
                Quaternion.Euler(0f, 20f, 0f), Vector2.one);
            leaves.AddBox(pos + new Vector3(0.6f, 3.6f, 0.3f), new Vector3(2.2f, 2f, 2.2f),
                Quaternion.Euler(0f, -15f, 0f), Vector2.one);
        }

        private static void AddLamp(MeshBatch metal, MeshBatch heads, Vector3 pos, Quaternion rot)
        {
            metal.AddCylinder(pos, 0.075f, 5.4f, 6, Quaternion.identity, new Vector2(1f, 1f), false);
            Vector3 arm = pos + rot * new Vector3(0f, 5.3f, 0.8f);
            metal.AddBox(arm, new Vector3(0.08f, 0.08f, 1.7f), rot, Vector2.one);
            heads.AddBox(arm + rot * new Vector3(0f, -0.08f, 0.85f),
                new Vector3(0.55f, 0.16f, 0.28f), rot, Vector2.one);
        }

        private static void AddBench(MeshBatch batch, Vector3 pos, Quaternion rot)
        {
            batch.AddBox(pos + rot * new Vector3(0f, 0.48f, 0f), new Vector3(1.9f, 0.1f, 0.5f),
                rot, Vector2.one);
            batch.AddBox(pos + rot * new Vector3(0f, 0.82f, -0.22f), new Vector3(1.9f, 0.5f, 0.08f),
                rot, Vector2.one);
            batch.AddBox(pos + rot * new Vector3(-0.75f, 0.24f, 0f), new Vector3(0.08f, 0.5f, 0.45f),
                rot, Vector2.one);
            batch.AddBox(pos + rot * new Vector3(0.75f, 0.24f, 0f), new Vector3(0.08f, 0.5f, 0.45f),
                rot, Vector2.one);
        }

        private static void AddSign(MeshBatch batch, Vector3 center, Vector3 forward, string key)
        {
            var sprite = AtlasRegistry.Sign(key);
            if (sprite == null) return;
            float texW = 2048f;
            float texH = 2048f;
            float u0 = sprite.x / texW;
            float u1 = (sprite.x + sprite.w) / texW;
            float v1 = 1f - sprite.y / texH;
            float v0 = 1f - (sprite.y + sprite.h) / texH;
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            Vector3 a = center - right * 0.4f - Vector3.up * 0.4f;
            Vector3 b = center + right * 0.4f - Vector3.up * 0.4f;
            Vector3 c = center + right * 0.4f + Vector3.up * 0.4f;
            Vector3 d = center - right * 0.4f + Vector3.up * 0.4f;
            batch.AddQuad(a, b, c, d,
                new Vector2(u0, v0), new Vector2(u1, v0),
                new Vector2(u1, v1), new Vector2(u0, v1));
        }

        private static void AddVerticalQuad(MeshBatch batch, Vector3 center, Vector3 forward,
                                            float width, float height, Vector2 uv0, Vector2 uv1)
        {
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            Vector3 a = center - right * (width * 0.5f) - Vector3.up * (height * 0.5f);
            Vector3 b = center + right * (width * 0.5f) - Vector3.up * (height * 0.5f);
            Vector3 c = center + right * (width * 0.5f) + Vector3.up * (height * 0.5f);
            Vector3 d = center - right * (width * 0.5f) + Vector3.up * (height * 0.5f);
            batch.AddQuad(a, b, c, d,
                new Vector2(uv0.x, uv0.y), new Vector2(uv1.x, uv0.y),
                new Vector2(uv1.x, uv1.y), new Vector2(uv0.x, uv1.y));
        }

        // ------------------------------------------------------------- signals

        private static void BuildTrafficLights(WorldLayout layout, GameObject parent)
        {
            var root = new GameObject("TrafficLights");
            root.transform.SetParent(parent.transform, false);
            var system = root.AddComponent<AlgerBusSim.Traffic.TrafficLightSystem>();

            string[] junctions = { "c5", "r5" };
            foreach (string id in junctions)
            {
                var node = layout.Graph.GetNode(id);
                if (node == null) continue;
                Vector3 c = new Vector3(node.x, 0f, node.z);
                for (int i = 0; i < 4; i++)
                {
                    float ang = i * 90f + 45f;
                    Vector3 offset = Quaternion.Euler(0f, ang, 0f) * new Vector3(11f, 0f, 0f);
                    Vector3 pos = c + offset;
                    var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    pole.name = "SignalPole";
                    pole.transform.SetParent(root.transform, false);
                    pole.transform.position = pos + Vector3.up * 2.2f;
                    pole.transform.localScale = new Vector3(0.16f, 2.2f, 0.16f);
                    pole.GetComponent<Renderer>().sharedMaterial = WorldMaterials.Metal;
                    UnityEngine.Object.Destroy(pole.GetComponent<Collider>());

                    var head = new GameObject("SignalHead");
                    head.transform.SetParent(root.transform, false);
                    head.transform.position = pos + Vector3.up * 4.2f;
                    head.transform.rotation = Quaternion.LookRotation((c - pos).normalized, Vector3.up);
                    var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    box.name = "Box";
                    box.transform.SetParent(head.transform, false);
                    box.transform.localScale = new Vector3(0.5f, 1.25f, 0.28f);
                    box.GetComponent<Renderer>().sharedMaterial = WorldMaterials.Dark;
                    UnityEngine.Object.Destroy(box.GetComponent<Collider>());

                    var lamps = new Renderer[3];
                    for (int l = 0; l < 3; l++)
                    {
                        var bulb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        bulb.name = "Bulb" + l;
                        bulb.transform.SetParent(head.transform, false);
                        bulb.transform.localPosition = new Vector3(0f, 0.38f - l * 0.38f, 0.16f);
                        bulb.transform.localScale = new Vector3(0.22f, 0.22f, 0.12f);
                        bulb.GetComponent<Renderer>().sharedMaterial = WorldMaterials.SignalOff;
                        UnityEngine.Object.Destroy(bulb.GetComponent<Collider>());
                        lamps[l] = bulb.GetComponent<Renderer>();
                    }
                    system.AddHead(new AlgerBusSim.Traffic.TrafficLightSystem.Head
                    {
                        Root = head.transform,
                        Red = lamps[0],
                        Yellow = lamps[1],
                        Green = lamps[2],
                        Axis = (i % 2 == 0) ? 0 : 1
                    });
                }
            }
            layout.Traffic = system;
        }

        // -------------------------------------------------------------- helpers

        private static void ComputeRouteStart(WorldLayout layout)
        {
            var node = layout.Graph.GetNode(layout.City.startNodeId);
            if (node == null) return;
            AlgerBusSim.Data.RoadEdge best = null;
            foreach (var e in layout.Graph.From(node.id))
            {
                if (e.kind == "boulevard") { best = e; break; }
                if (best == null) best = e;
            }
            if (best == null) return;
            var pts = layout.Graph.SampleEdge(best);
            if (pts.Count < 2) return;
            Vector3 p = PointAtDistance(pts, 18f, out Vector3 d);
            Vector3 right = new Vector3(d.z, 0f, -d.x);
            layout.RouteStart = p + right * (best.width * 0.22f) + Vector3.up * 0.2f;
            layout.RouteStartRotation = Quaternion.LookRotation(d, Vector3.up);
        }

        private static float PolyLength(List<Vector3> pts)
        {
            float len = 0f;
            for (int i = 1; i < pts.Count; i++) len += Vector3.Distance(pts[i - 1], pts[i]);
            return len;
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
