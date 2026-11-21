using System;
using System.Collections.Generic;

namespace AlgerBusSim.Core
{
    public static class ServiceHub
    {
        private static readonly Dictionary<string, object> Services = new Dictionary<string, object>();

        public static void Register(string key, object service)
        {
            Services[key] = service;
        }

        public static T Get<T>(string key) where T : class
        {
            object o;
            if (Services.TryGetValue(key, out o)) return o as T;
            return null;
        }

        public static bool Has(string key)
        {
            return Services.ContainsKey(key);
        }

        public static void Clear()
        {
            Services.Clear();
        }
    }
}
