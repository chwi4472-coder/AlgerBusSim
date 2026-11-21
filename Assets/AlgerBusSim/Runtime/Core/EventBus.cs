using System;
using System.Collections.Generic;

namespace AlgerBusSim.Core
{
    public static class EventBus
    {
        private static readonly Dictionary<string, List<Action<object>>> Listeners =
            new Dictionary<string, List<Action<object>>>();

        public static void Subscribe(string topic, Action<object> handler)
        {
            if (!Listeners.ContainsKey(topic)) Listeners[topic] = new List<Action<object>>();
            Listeners[topic].Add(handler);
        }

        public static void Publish(string topic, object payload)
        {
            List<Action<object>> list;
            if (Listeners.TryGetValue(topic, out list))
            {
                foreach (var h in list) h(payload);
            }
        }

        public static void Clear()
        {
            Listeners.Clear();
        }
    }
}
