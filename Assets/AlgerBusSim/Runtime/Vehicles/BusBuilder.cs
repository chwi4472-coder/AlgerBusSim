using System.Collections.Generic;
using UnityEngine;

namespace AlgerBusSim.Vehicles
{
    public static class BusBuilder
    {
        public static GameObject Build(string busId, Vector3 position)
        {
            var go = new GameObject("Bus_" + busId);
            go.transform.position = position;
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.SetParent(go.transform);
            body.transform.localPosition = new Vector3(0f, 1.55f, 0f);
            body.transform.localScale = new Vector3(2.5f, 3.1f, 12.0f);
            body.name = "Body";
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 12500f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            return go;
        }

        public static Mesh BuildBodyMesh()
        {
            var mesh = new Mesh();
            mesh.name = "BusBody";
            // simple box
            Vector3[] v = new Vector3[]
            {
                new Vector3(-1.25f,0f,-6f), new Vector3(1.25f,0f,-6f), new Vector3(1.25f,3.1f,-6f), new Vector3(-1.25f,3.1f,-6f),
                new Vector3(-1.25f,0f,6f), new Vector3(1.25f,0f,6f), new Vector3(1.25f,3.1f,6f), new Vector3(-1.25f,3.1f,6f)
            };
            int[] t = new int[]
            {
                0,2,1, 0,3,2, 4,5,6, 4,6,7, 0,1,5, 0,5,4, 2,3,7, 2,7,6, 0,4,7, 0,7,3, 1,2,6, 1,6,5
            };
            mesh.vertices = v;
            mesh.triangles = t;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
