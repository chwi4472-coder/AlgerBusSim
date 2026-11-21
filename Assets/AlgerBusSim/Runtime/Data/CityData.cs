using System;
using System.Collections.Generic;

namespace AlgerBusSim.Data
{
    [Serializable]
    public class CityName
    {
        public string fr;
        public string ar;
        public string dz;
    }

    [Serializable]
    public class District
    {
        public string id;
        public CityName name;
        public int kind;
    }

    [Serializable]
    public class Landmark
    {
        public string id;
        public string districtId;
        public string nodeId;
        public string kind;
    }

    [Serializable]
    public class CityData
    {
        public string id;
        public string slug;
        public CityName name;
        public float boundsHalfWidthKm;
        public float boundsHalfDepthKm;
        public string startNodeId;
        public string endNodeId;
        public int timeZoneOffsetHours;
        public List<District> districts;
        public List<Landmark> landmarks;
    }

    [Serializable]
    public class RoadNode
    {
        public string id;
        public float x;
        public float z;
        public float y;
        public string kind;
    }

    [Serializable]
    public class RoadEdge
    {
        public string id;
        public string from;
        public string to;
        public int lanes;
        public float width;
        public string kind;
        public int speedLimit;
        public bool oneway;
        public string profile;
        public float sidewalk;
        public bool parking;
    }

    [Serializable]
    public class Stop
    {
        public string id;
        public string edge;
        public float t;
        public CityName name;
    }

    [Serializable]
    public class RoadGraphData
    {
        public List<RoadNode> nodes;
        public List<RoadEdge> edges;
        public List<Stop> stops;
    }
}
