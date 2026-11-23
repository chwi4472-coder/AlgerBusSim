using System.Collections.Generic;
using UnityEngine;

namespace AlgerBusSim.World
{
    public sealed class StopPoint
    {
        public string Id;
        public string NameFr;
        public string NameAr;
        public string NameDz;
        public Vector3 BusPosition;
        public Vector3 ShelterPosition;
        public Vector3 Forward;
        public int Index;
        public readonly List<GameObject> Waiting = new List<GameObject>();

        public string Name(string language)
        {
            if (language == "ar") return NameAr;
            if (language == "dz") return NameDz;
            return NameFr;
        }
    }

    public sealed class WorldLayout
    {
        public AlgerBusSim.Data.CityData City;
        public RoadGraph Graph;
        public GameObject Root;
        public Material WaterMaterial;
        public Vector3 RouteStart;
        public Quaternion RouteStartRotation = Quaternion.identity;
        public readonly List<StopPoint> Stops = new List<StopPoint>();
        public readonly List<MeshRenderer> LampHeads = new List<MeshRenderer>();
        public AlgerBusSim.Traffic.TrafficLightSystem Traffic;
    }
}
