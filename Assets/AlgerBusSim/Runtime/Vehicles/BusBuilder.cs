using System.Collections.Generic;
using UnityEngine;
using AlgerBusSim.Art;

namespace AlgerBusSim.Vehicles
{
    public static class BusBuilder
    {
        public static GameObject Build(string busId, Vector3 position, Quaternion rotation)
        {
            var go = new GameObject("Bus_" + busId);
            go.transform.SetPositionAndRotation(position, rotation);
            go.layer = 8; // Player
            go.tag = "Player";

            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 12500f;
            rb.drag = 0.02f;
            rb.angularDrag = 0.6f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            var col = go.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 1.55f, 0f);
            col.size = new Vector3(2.35f, 2.7f, 11.4f);

            var controller = go.AddComponent<BusController>();

            BuildBody(go);
            BuildWheels(go, controller);
            BuildLights(go, controller);
            BuildDoors(go, controller);

            rb.centerOfMass = new Vector3(0f, 1.05f, 0.15f);

            return go;
        }

        private static void BuildBody(GameObject go)
        {
            var t = go.transform;
            var white = WorldMaterials.White;
            var dark = WorldMaterials.Dark;
            var metal = WorldMaterials.Metal;
            var glass = WorldMaterials.Glass;
            var teal = WorldMaterials.Solid(Palette.FromHex(Palette.BusStripeTeal), 0.1f);
            var blue = WorldMaterials.Solid(Palette.FromHex(Palette.BusDoorBlue), 0.15f);
            var roofLight = WorldMaterials.Solid(Palette.FromHex(Palette.BusRoofLight), 0.3f);

            // Lower body
            Box(t, "LowerBody", new Vector3(0f, 0.85f, 0f), new Vector3(2.5f, 1.1f, 11.6f), white);
            // Skirt stripe
            Box(t, "Skirt", new Vector3(0f, 0.45f, 0f), new Vector3(2.58f, 0.5f, 11.7f), teal);
            Box(t, "SkirtBlue", new Vector3(0f, 0.85f, 0f), new Vector3(2.58f, 0.3f, 11.7f), blue);
            // Mid body
            Box(t, "MidBody", new Vector3(0f, 1.95f, 0f), new Vector3(2.5f, 1.1f, 11.6f), white);
            // Upper body
            Box(t, "UpperBody", new Vector3(0f, 2.85f, 0f), new Vector3(2.42f, 0.7f, 11.4f), white);
            // Roof
            Box(t, "Roof", new Vector3(0f, 3.25f, 0f), new Vector3(2.35f, 0.22f, 11.2f), roofLight);
            // Roof AC
            Box(t, "AC", new Vector3(0f, 3.45f, -1.5f), new Vector3(1.6f, 0.35f, 3.0f), metal);
            // Roof vents
            for (int i = -4; i <= 4; i++)
            {
                if (i == 0) continue;
                Box(t, "Vent" + i, new Vector3(0f, 3.42f, i * 1.1f), new Vector3(0.6f, 0.12f, 0.4f), metal);
            }

            // Window band (both sides)
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * 1.265f;
                // main glass strip
                Box(t, "WindowBand" + side, new Vector3(side * 1.30f, 2.2f, -0.1f), new Vector3(0.1f, 1.1f, 10.2f), glass);
                // pillars
                for (int i = 0; i < 9; i++)
                {
                    float z = -4.4f + i * 1.1f;
                    Box(t, "Pillar" + side + i, new Vector3(side * 1.31f, 2.2f, z), new Vector3(0.12f, 1.15f, 0.14f), white);
                }
            }
            // Front windshield
            Box(t, "Windshield", new Vector3(0f, 2.3f, 5.85f), new Vector3(2.2f, 1.5f, 0.1f), glass);
            // windshield frame
            Box(t, "WindshieldFrameTop", new Vector3(0f, 2.95f, 5.78f), new Vector3(2.2f, 0.12f, 0.12f), white);
            Box(t, "WindshieldFrameBot", new Vector3(0f, 1.55f, 5.78f), new Vector3(2.2f, 0.12f, 0.12f), white);
            Box(t, "WindshieldFrameL", new Vector3(-1.06f, 2.25f, 5.78f), new Vector3(0.12f, 1.35f, 0.12f), white);
            Box(t, "WindshieldFrameR", new Vector3(1.06f, 2.25f, 5.78f), new Vector3(0.12f, 1.35f, 0.12f), white);

            // Destination sign above windshield
            var dest = new GameObject("DestinationSign");
            dest.transform.SetParent(t, false);
            dest.transform.localPosition = new Vector3(0f, 2.95f, 5.8f);
            dest.transform.localScale = new Vector3(2.0f, 0.42f, 0.05f);
            var destMf = dest.AddComponent<MeshFilter>();
            destMf.sharedMesh = BuildQuadMesh();
            var destMr = dest.AddComponent<MeshRenderer>();
            destMr.sharedMaterial = WorldMaterials.Destination;
            destMr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // Front face
            Box(t, "FrontFace", new Vector3(0f, 1.55f, 5.82f), new Vector3(2.45f, 2.6f, 0.12f), white);
            // grille
            Box(t, "Grille", new Vector3(0f, 0.95f, 5.86f), new Vector3(1.6f, 0.6f, 0.08f), dark);
            // bumper
            Box(t, "BumperFront", new Vector3(0f, 0.35f, 5.9f), new Vector3(2.5f, 0.35f, 0.25f), dark);
            // mirrors
            Box(t, "MirrorL", new Vector3(-1.35f, 2.1f, 4.6f), new Vector3(0.12f, 0.22f, 0.4f), metal);
            Box(t, "MirrorR", new Vector3(1.35f, 2.1f, 4.6f), new Vector3(0.12f, 0.22f, 0.4f), metal);

            // Rear face
            Box(t, "RearFace", new Vector3(0f, 1.55f, -5.82f), new Vector3(2.45f, 2.6f, 0.12f), white);
            // rear engine grille
            Box(t, "RearGrille", new Vector3(0f, 1.1f, -5.86f), new Vector3(1.8f, 0.9f, 0.08f), dark);
            // bumper rear
            Box(t, "BumperRear", new Vector3(0f, 0.35f, -5.9f), new Vector3(2.5f, 0.35f, 0.25f), dark);
            // rear plate
            var plate = new GameObject("RearPlate");
            plate.transform.SetParent(t, false);
            plate.transform.localPosition = new Vector3(0f, 0.85f, -5.88f);
            plate.transform.localScale = new Vector3(0.5f, 0.12f, 0.02f);
            var pmf = plate.AddComponent<MeshFilter>(); pmf.sharedMesh = BuildQuadMesh();
            var pmr = plate.AddComponent<MeshRenderer>(); pmr.sharedMaterial = WorldMaterials.DecalPlate;
            pmr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // side decals
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * 1.28f;
                var decal = new GameObject("Decal" + side);
                decal.transform.SetParent(t, false);
                decal.transform.localPosition = new Vector3(side * 1.32f, 1.5f, 0.3f);
                decal.transform.localRotation = Quaternion.Euler(0f, side > 0 ? 90f : -90f, 0f);
                decal.transform.localScale = new Vector3(6.0f, 1.0f, 1f);
                var dmf = decal.AddComponent<MeshFilter>(); dmf.sharedMesh = BuildQuadMesh();
                var dmr = decal.AddComponent<MeshRenderer>(); dmr.sharedMaterial = WorldMaterials.DecalBus;
                dmr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            // front plate
            var fplate = new GameObject("FrontPlate");
            fplate.transform.SetParent(t, false);
            fplate.transform.localPosition = new Vector3(0f, 0.85f, 5.88f);
            fplate.transform.localScale = new Vector3(0.5f, 0.12f, 0.02f);
            var fpmf = fplate.AddComponent<MeshFilter>(); fpmf.sharedMesh = BuildQuadMesh();
            var fpmr = fplate.AddComponent<MeshRenderer>(); fpmr.sharedMaterial = WorldMaterials.DecalPlate;
            fpmr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // Wheels visuals are added in BuildWheels
        }

        private static void BuildWheels(GameObject go, BusController controller)
        {
            var t = go.transform;
            var wheelPositions = new Vector3[]
            {
                new Vector3(-1.05f, 0.52f, 3.9f),
                new Vector3(1.05f, 0.52f, 3.9f),
                new Vector3(-1.05f, 0.52f, -3.4f),
                new Vector3(1.05f, 0.52f, -3.4f),
            };
            var isFront = new[] { true, true, false, false };

            var colliders = new WheelCollider[4];
            for (int i = 0; i < 4; i++)
            {
                var wcGo = new GameObject("WheelCollider" + i);
                wcGo.transform.SetParent(t, false);
                wcGo.transform.localPosition = wheelPositions[i];
                var wc = wcGo.AddComponent<WheelCollider>();
                wc.radius = 0.52f;
                wc.mass = 40f;
                wc.wheelDampingRate = 0.6f;
                wc.suspensionDistance = 0.18f;
                JointSpring js = wc.suspensionSpring;
                js.spring = 95000f;
                js.damper = 9000f;
                js.targetPosition = 0.5f;
                wc.suspensionSpring = js;
                wc.forceAppPointDistance = 0.05f;
                // friction
                WheelFrictionCurve fwd = wc.forwardFriction;
                fwd.extremumSlip = 0.4f; fwd.extremumValue = 0.9f;
                fwd.asymptoteSlip = 0.8f; fwd.asymptoteValue = 0.6f;
                fwd.stiffness = 1.4f;
                wc.forwardFriction = fwd;
                WheelFrictionCurve side = wc.sidewaysFriction;
                side.extremumSlip = 0.25f; side.extremumValue = 1.0f;
                side.asymptoteSlip = 0.5f; side.asymptoteValue = 0.8f;
                side.stiffness = 1.6f;
                wc.sidewaysFriction = side;

                colliders[i] = wc;

                // visual wheel
                var vwGo = new GameObject("WheelVisual" + i);
                vwGo.transform.SetParent(t, false);
                var wv = vwGo.AddComponent<WheelVisuals>();
                wv.Collider = wc;

                var meshGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                meshGo.name = "Mesh";
                meshGo.transform.SetParent(vwGo.transform, false);
                meshGo.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                meshGo.transform.localScale = new Vector3(0.32f, 0.52f, 0.32f);
                UnityEngine.Object.Destroy(meshGo.GetComponent<Collider>());
                meshGo.GetComponent<Renderer>().sharedMaterial = WorldMaterials.Dark;

                // hub cap
                var hub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                hub.name = "Hub";
                hub.transform.SetParent(meshGo.transform, false);
                hub.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                hub.transform.localScale = new Vector3(0.6f, 1.05f, 0.6f);
                hub.transform.localPosition = new Vector3(0f, 0.01f, 0f);
                UnityEngine.Object.Destroy(hub.GetComponent<Collider>());
                hub.GetComponent<Renderer>().sharedMaterial = WorldMaterials.Metal;
            }

            controller.FrontLeft = colliders[0];
            controller.FrontRight = colliders[1];
            controller.RearLeft = colliders[2];
            controller.RearRight = colliders[3];
        }

        private static void BuildLights(GameObject go, BusController controller)
        {
            var t = go.transform;
            // Headlight emissive quads
            for (int side = -1; side <= 1; side += 2)
            {
                var hl = new GameObject("Headlight" + side);
                hl.transform.SetParent(t, false);
                hl.transform.localPosition = new Vector3(side * 0.85f, 0.85f, 5.86f);
                hl.transform.localScale = new Vector3(0.35f, 0.22f, 0.02f);
                var mf = hl.AddComponent<MeshFilter>(); mf.sharedMesh = BuildQuadMesh();
                var mr = hl.AddComponent<MeshRenderer>(); mr.sharedMaterial = WorldMaterials.Headlight;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            // Taillights
            for (int side = -1; side <= 1; side += 2)
            {
                var tl = new GameObject("Taillight" + side);
                tl.transform.SetParent(t, false);
                tl.transform.localPosition = new Vector3(side * 0.85f, 0.95f, -5.86f);
                tl.transform.localScale = new Vector3(0.3f, 0.2f, 0.02f);
                var mf = tl.AddComponent<MeshFilter>(); mf.sharedMesh = BuildQuadMesh();
                var mr = tl.AddComponent<MeshRenderer>(); mr.sharedMaterial = WorldMaterials.Taillight;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            // Spotlights (children of bus, enabled at night by BusController)
            for (int side = -1; side <= 1; side += 2)
            {
                var spot = new GameObject("Spotlight" + side);
                spot.transform.SetParent(t, false);
                spot.transform.localPosition = new Vector3(side * 0.85f, 0.85f, 5.86f);
                spot.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                var l = spot.AddComponent<Light>();
                l.type = LightType.Spot;
                l.color = new Color(1f, 0.95f, 0.8f);
                l.intensity = 2.2f;
                l.range = 60f;
                l.spotAngle = 38f;
                l.shadows = LightShadows.Soft;
                l.enabled = false;
                controller.HeadlightSpots.Add(l);
            }
        }

        private static void BuildDoors(GameObject go, BusController controller)
        {
            var t = go.transform;
            // Front door at z=3.9, middle door at z=-0.8, on right side x=1.26
            float[] zs = { 3.9f, -0.8f };
            foreach (var z in zs)
            {
                // Left panel (front half)
                var lp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                lp.name = "DoorLeft_" + z;
                lp.transform.SetParent(t, false);
                lp.transform.localPosition = new Vector3(1.265f, 1.6f, z + 0.36f);
                lp.transform.localScale = new Vector3(0.06f, 2.0f, 0.7f);
                UnityEngine.Object.Destroy(lp.GetComponent<Collider>());
                lp.GetComponent<Renderer>().sharedMaterial = WorldMaterials.Glass;
                controller.DoorSystem.LeftPanels.Add(lp.transform);

                var rp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rp.name = "DoorRight_" + z;
                rp.transform.SetParent(t, false);
                rp.transform.localPosition = new Vector3(1.265f, 1.6f, z - 0.36f);
                rp.transform.localScale = new Vector3(0.06f, 2.0f, 0.7f);
                UnityEngine.Object.Destroy(rp.GetComponent<Collider>());
                rp.GetComponent<Renderer>().sharedMaterial = WorldMaterials.Glass;
                controller.DoorSystem.RightPanels.Add(rp.transform);
            }
            controller.DoorSystem.Bind();
        }

        private static void Box(Transform parent, string name, Vector3 pos, Vector3 size, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = size;
            UnityEngine.Object.Destroy(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static Mesh BuildQuadMesh()
        {
            var mesh = new Mesh();
            mesh.name = "Quad";
            Vector3[] v = new Vector3[]
            {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(0.5f, -0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f)
            };
            int[] t = new int[] { 0, 1, 2, 0, 2, 3 };
            Vector2[] u = new Vector2[]
            {
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(1f, 1f), new Vector2(0f, 1f)
            };
            mesh.vertices = v;
            mesh.triangles = t;
            mesh.uv = u;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
