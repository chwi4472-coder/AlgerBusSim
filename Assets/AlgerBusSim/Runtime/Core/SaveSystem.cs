using System;
using System.IO;

namespace AlgerBusSim.Core
{
    public static class SaveSystem
    {
        public static string SavePath = "profile.json";

        public static string ToJson(object obj)
        {
            return UnityEngine.JsonUtility.ToJson(obj, true);
        }

        public static T FromJson<T>(string json)
        {
            return UnityEngine.JsonUtility.FromJson<T>(json);
        }

        public static void WriteFile(string path, string content)
        {
            File.WriteAllText(path, content);
        }

        public static string ReadFile(string path)
        {
            if (!File.Exists(path)) return null;
            return File.ReadAllText(path);
        }
    }
}
