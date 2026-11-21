using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace AlgerBusSim.Localization
{
    public enum Language
    {
        French = 0,
        Arabic = 1,
        Darija = 2,
    }

    public static class Localization
    {
        private static readonly Dictionary<string, string> Strings = new Dictionary<string, string>();
        private static string _language = "fr";

        public static string Language
        {
            get { return _language; }
            set
            {
                _language = value;
                Load();
            }
        }

        public static bool IsRtl
        {
            get { return _language == "ar" || _language == "dz"; }
        }

        public static string Get(string key)
        {
            string raw;
            if (!Strings.TryGetValue(key, out raw)) raw = key;
            string text = raw;
            if (_language == "ar" || _language == "dz")
            {
                text = ArabicShaper.Shape(text);
            }
            return text;
        }

        public static string Get(string key, params object[] args)
        {
            string pattern = Get(key);
            string result = pattern;
            for (int i = 0; i < args.Length; i++)
            {
                result = result.Replace("{" + (i + 1) + "}", args[i] == null ? "" : args[i].ToString());
            }
            if (_language == "ar" || _language == "dz")
            {
                result = ArabicShaper.Shape(result);
            }
            return result;
        }

        public static void Load()
        {
            Strings.Clear();
            string path = Application.streamingAssetsPath + "/data/localization/" + _language + ".csv";
            var request = UnityWebRequest.Get(path);
            request.SendWebRequest();
            while (!request.isDone) { }
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("[AlgerBusSim] Localization load failed for " + _language + ": " + request.error);
                return;
            }
            string[] lines = request.downloadHandler.text.Split('\n');
            foreach (string line in lines)
            {
                string l = line.Trim();
                if (l.Length == 0 || l.StartsWith("#")) continue;
                int comma = l.IndexOf(',');
                if (comma < 0) continue;
                string key = l.Substring(0, comma).Trim();
                string value = l.Substring(comma + 1).Trim();
                Strings[key] = value;
            }
        }

        public static string FormatDzd(long amount)
        {
            bool arabic = _language == "ar" || _language == "dz";
            string digits = arabic ? ToArabicNumerals(amount.ToString()) : amount.ToString();
            string unit = arabic ? "د.ج" : "دج";
            return digits + " " + unit;
        }

        private static string ToArabicNumerals(string digits)
        {
            var sb = new System.Text.StringBuilder();
            foreach (char c in digits)
            {
                if (c >= '0' && c <= '9')
                {
                    sb.Append((char)('0' + (c - '0') + 9));
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
