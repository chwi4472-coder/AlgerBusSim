using System.Collections.Generic;
using UnityEngine;

namespace AlgerBusSim.World
{
    /// <summary>
    /// Accumulates flat-shaded geometry into a single mesh so a whole district
    /// costs one draw call. Every primitive is authored from code, no imports.
    /// </summary>
    public sealed class MeshBatch
    {
        private readonly List<Vector3> _verts = new List<Vector3>();
        private readonly List<Vector3> _normals = new List<Vector3>();
        private readonly List<Vector2> _uvs = new List<Vector2>();
        private readonly List<int> _tris = new List<int>();

        public int VertexCount => _verts.Count;

        public void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d,
                            Vector2 ua, Vector2 ub, Vector2 uc, Vector2 ud)
        {
            int i = _verts.Count;
            Vector3 n = Vector3.Cross(b - a, c - a).normalized;
            _verts.Add(a); _verts.Add(b); _verts.Add(c); _verts.Add(d);
            _normals.Add(n); _normals.Add(n); _normals.Add(n); _normals.Add(n);
            _uvs.Add(ua); _uvs.Add(ub); _uvs.Add(uc); _uvs.Add(ud);
            _tris.Add(i); _tris.Add(i + 1); _tris.Add(i + 2);
            _tris.Add(i); _tris.Add(i + 2); _tris.Add(i + 3);
        }

        /// <summary>Horizontal quad facing up, corners in clockwise order seen from above.</summary>
        public void AddGroundQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float tile)
        {
            float ab = Vector3.Distance(a, b);
            float ad = Vector3.Distance(a, d);
            AddQuad(a, b, c, d,
                new Vector2(0f, 0f),
                new Vector2(0f, ab / tile),
                new Vector2(ad / tile, ab / tile),
                new Vector2(ad / tile, 0f));
        }

        public void AddBox(Vector3 center, Vector3 size, Quaternion rot, Vector2 uvPerMeter)
        {
            Vector3 h = size * 0.5f;
            Vector3 p000 = new Vector3(-h.x, -h.y, -h.z);
            Vector3 p100 = new Vector3(h.x, -h.y, -h.z);
            Vector3 p110 = new Vector3(h.x, h.y, -h.z);
            Vector3 p010 = new Vector3(-h.x, h.y, -h.z);
            Vector3 p001 = new Vector3(-h.x, -h.y, h.z);
            Vector3 p101 = new Vector3(h.x, -h.y, h.z);
            Vector3 p111 = new Vector3(h.x, h.y, h.z);
            Vector3 p011 = new Vector3(-h.x, h.y, h.z);

            AddBoxFace(p000, p010, p110, p100, rot, center, size, uvPerMeter);
            AddBoxFace(p001, p101, p111, p011, rot, center, size, uvPerMeter);
            AddBoxFace(p000, p001, p011, p010, rot, center, size, uvPerMeter);
            AddBoxFace(p100, p110, p111, p101, rot, center, size, uvPerMeter);
            AddBoxFace(p000, p100, p101, p001, rot, center, size, uvPerMeter);
            AddBoxFace(p010, p011, p111, p110, rot, center, size, uvPerMeter);
        }

        private void AddBoxFace(Vector3 a, Vector3 b, Vector3 c, Vector3 d,
                                Quaternion rot, Vector3 center, Vector3 size, Vector2 uvPerMeter)
        {
            Vector3 wa = center + rot * a;
            Vector3 wb = center + rot * b;
            Vector3 wc = center + rot * c;
            Vector3 wd = center + rot * d;

            Vector2 ua, ub, uc, ud;
            Vector3 n = Vector3.Cross(b - a, c - a);
            if (Mathf.Abs(n.y) > Mathf.Abs(n.x) && Mathf.Abs(n.y) > Mathf.Abs(n.z))
            {
                ua = new Vector2(a.x + size.x * 0.5f, a.z + size.z * 0.5f);
                ub = new Vector2(b.x + size.x * 0.5f, b.z + size.z * 0.5f);
                uc = new Vector2(c.x + size.x * 0.5f, c.z + size.z * 0.5f);
                ud = new Vector2(d.x + size.x * 0.5f, d.z + size.z * 0.5f);
            }
            else if (Mathf.Abs(n.x) > Mathf.Abs(n.z))
            {
                ua = new Vector2(a.z + size.z * 0.5f, a.y + size.y * 0.5f);
                ub = new Vector2(b.z + size.z * 0.5f, b.y + size.y * 0.5f);
                uc = new Vector2(c.z + size.z * 0.5f, c.y + size.y * 0.5f);
                ud = new Vector2(d.z + size.z * 0.5f, d.y + size.y * 0.5f);
            }
            else
            {
                ua = new Vector2(a.x + size.x * 0.5f, a.y + size.y * 0.5f);
                ub = new Vector2(b.x + size.x * 0.5f, b.y + size.y * 0.5f);
                uc = new Vector2(c.x + size.x * 0.5f, c.y + size.y * 0.5f);
                ud = new Vector2(d.x + size.x * 0.5f, d.y + size.y * 0.5f);
            }

            AddQuad(wa, wb, wc, wd,
                Vector2.Scale(ua, uvPerMeter),
                Vector2.Scale(ub, uvPerMeter),
                Vector2.Scale(uc, uvPerMeter),
                Vector2.Scale(ud, uvPerMeter));
        }

        public void AddCylinder(Vector3 baseCenter, float radius, float height, int segments,
                                Quaternion rot, Vector2 uvPerMeter, bool caps = true)
        {
            segments = Mathf.Max(3, segments);
            float circumference = 2f * Mathf.PI * radius;
            for (int i = 0; i < segments; i++)
            {
                float t0 = (float)i / segments;
                float t1 = (float)(i + 1) / segments;
                float a0 = t0 * Mathf.PI * 2f;
                float a1 = t1 * Mathf.PI * 2f;
                Vector3 r0 = new Vector3(Mathf.Cos(a0) * radius, 0f, Mathf.Sin(a0) * radius);
                Vector3 r1 = new Vector3(Mathf.Cos(a1) * radius, 0f, Mathf.Sin(a1) * radius);
                Vector3 b0 = baseCenter + rot * r0;
                Vector3 b1 = baseCenter + rot * r1;
                Vector3 u0 = baseCenter + rot * (r0 + Vector3.up * height);
                Vector3 u1 = baseCenter + rot * (r1 + Vector3.up * height);
                AddQuad(b0, u0, u1, b1,
                    new Vector2(t0 * circumference * uvPerMeter.x, 0f),
                    new Vector2(t0 * circumference * uvPerMeter.x, height * uvPerMeter.y),
                    new Vector2(t1 * circumference * uvPerMeter.x, height * uvPerMeter.y),
                    new Vector2(t1 * circumference * uvPerMeter.x, 0f));
            }
            if (caps)
            {
                AddDisc(baseCenter, radius, segments, rot, uvPerMeter, false);
                AddDisc(baseCenter + rot * Vector3.up * height, radius, segments, rot, uvPerMeter, true);
            }
        }

        /// <summary>Horizontal disc; up=true makes the visible side +Y.</summary>
        public void AddDisc(Vector3 center, float radius, int segments, Quaternion rot,
                            Vector2 uvPerMeter, bool up)
        {
            segments = Mathf.Max(3, segments);
            int start = _verts.Count;
            Vector3 n = up ? Vector3.up : Vector3.down;
            _verts.Add(center);
            _normals.Add(rot * n);
            _uvs.Add(new Vector2(0.5f, 0.5f));
            for (int i = 0; i <= segments; i++)
            {
                float a = (float)i / segments * Mathf.PI * 2f;
                Vector3 p = new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
                _verts.Add(center + rot * p);
                _normals.Add(rot * n);
                _uvs.Add(new Vector2(0.5f + Mathf.Cos(a) * 0.5f, 0.5f + Mathf.Sin(a) * 0.5f));
            }
            for (int i = 0; i < segments; i++)
            {
                int i0 = start + 1 + i;
                int i1 = start + 2 + i;
                if (up)
                {
                    _tris.Add(start); _tris.Add(i1); _tris.Add(i0);
                }
                else
                {
                    _tris.Add(start); _tris.Add(i0); _tris.Add(i1);
                }
            }
        }

        public void AddFlatQuadUp(Vector3 a, Vector3 b, Vector3 c, Vector3 d,
                                 Vector2 ua, Vector2 ub, Vector2 uc, Vector2 ud)
        {
            if (Vector3.Cross(b - a, c - a).y < 0f)
            {
                var tb = b; b = d; d = tb;
                var tu = ub; ub = ud; ud = tu;
            }
            AddQuad(a, b, c, d, ua, ub, uc, ud);
        }

        public GameObject Commit(Transform parent, string name, Material material,
                                 bool castShadows = true, bool receiveShadows = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var mesh = new Mesh();
            mesh.name = name;
            mesh.indexFormat = _verts.Count > 65000
                ? UnityEngine.Rendering.IndexFormat.UInt32
                : UnityEngine.Rendering.IndexFormat.UInt16;
            mesh.SetVertices(_verts);
            mesh.SetNormals(_normals);
            mesh.SetUVs(0, _uvs);
            mesh.SetTriangles(_tris, 0);
            mesh.RecalculateBounds();
            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = material;
            mr.shadowCastingMode = castShadows
                ? UnityEngine.Rendering.ShadowCastingMode.On
                : UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = receiveShadows;
            return go;
        }
    }
}
