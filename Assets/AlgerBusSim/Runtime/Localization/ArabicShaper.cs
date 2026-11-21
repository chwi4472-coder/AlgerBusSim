using System.Collections.Generic;
using System.Text;

namespace AlgerBusSim.Localization
{
    /// <summary>
    /// Shapes Arabic into Unicode Arabic Presentation Forms and reorders it
    /// into visual (left-to-right) order for Unity's legacy text stack, which
    /// does not do this itself.  Mirrors tools/arabic.py exactly.
    /// </summary>
    public static class ArabicShaper
    {
        public const int JoinNone = 0;
        public const int JoinRight = 1;
        public const int JoinDual = 2;

        private static readonly AlgerBusSim.Localization.Generated.ArabicShapingTable Table
            = new AlgerBusSim.Localization.Generated.ArabicShapingTable();

        private static readonly HashSet<int> Tasheel = BuildTasheel();
        private static readonly HashSet<int> LetterSet = BuildLetters();

        private static HashSet<int> BuildTasheel()
        {
            var set = new HashSet<int>();
            for (int cp = 0x064B; cp <= 0x065F; cp++) set.Add(cp);
            set.Add(0x0670);
            for (int cp = 0x06D6; cp <= 0x06DC; cp++) set.Add(cp);
            for (int cp = 0x06DF; cp <= 0x06E4; cp++) set.Add(cp);
            set.Add(0x06E7);
            set.Add(0x06E8);
            for (int cp = 0x06EA; cp <= 0x06ED; cp++) set.Add(cp);
            return set;
        }

        private static HashSet<int> BuildLetters()
        {
            var set = new HashSet<int>();
            for (int cp = 0x0621; cp <= 0x064A; cp++) set.Add(cp);
            set.Add(0x0671);
            set.Add(0x06A4);
            set.Add(0x06A9);
            set.Add(0x06AF);
            return set;
        }

        private static bool IsArabic(int cp)
        {
            return (cp >= 0x0600 && cp <= 0x06FF) || (cp >= 0x0750 && cp <= 0x077F) ||
                   (cp >= 0x08A0 && cp <= 0x08FF) || (cp >= 0xFB50 && cp <= 0xFDFF) ||
                   (cp >= 0xFE70 && cp <= 0xFEFF);
        }

        private static bool IsLetter(int cp)
        {
            return LetterSet.Contains(cp);
        }

        private static bool JoinsForward(int cp)
        {
            int[] t = Table.FormsLookup(cp, out _, out _);
            if (t == null) return false;
            int join = t[5];
            return join == JoinDual;
        }

        private static bool JoinsBackward(int cp)
        {
            int[] t = Table.FormsLookup(cp, out _, out _);
            if (t == null) return false;
            int join = t[5];
            return join != JoinNone;
        }

        public static List<int> ShapeLogical(string text)
        {
            var outCp = new List<int>();
            int i = 0;
            while (i < text.Length)
            {
                int cp = text[i];
                if (cp == 0x0644 && i + 1 < text.Length && IsLamAlef(text[i + 1]))
                {
                    int next = text[i + 1];
                    int isolated, fin;
                    int ok = Table.LamAlefLookupRow(next, out isolated, out fin);
                    int prev = outCp.Count > 0 ? outCp[outCp.Count - 1] : 0;
                    bool joinedPrev = prev != 0 && JoinsForward(prev);
                    outCp.Add(joinedPrev ? fin : isolated);
                    i += 2;
                    continue;
                }
                if (Tasheel.Contains(cp)) { outCp.Add(cp); i++; continue; }
                int[] t = Table.FormsLookup(cp, out int iso, out int fin);
                if (t == null) { outCp.Add(cp); i++; continue; }
                int j = i + 1;
                while (j < text.Length && Tasheel.Contains(text[j])) j++;
                int next = j < text.Length ? text[j] : 0;
                int prev = 0;
                for (int k = outCp.Count - 1; k >= 0; k--)
                {
                    int c = outCp[k];
                    if (Tasheel.Contains(c)) continue;
                    prev = c;
                    break;
                }
                bool canPrev = prev != 0 && JoinsForward(prev) && JoinsBackward(cp);
                bool canNext = t[5] == JoinDual && next != 0 && JoinsBackward(next);
                if (canPrev && canNext && t[4] >= 0) outCp.Add(t[4]);
                else if (canNext && t[3] >= 0) outCp.Add(t[3]);
                else if (canPrev && t[2] >= 0) outCp.Add(t[2]);
                else outCp.Add(t[1] >= 0 ? t[1] : cp);
                i++;
            }
            return outCp;
        }

        public static string Shape(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var logical = ShapeLogical(text);
            var kinds = new List<string>();
            var cps = new List<int>();
            foreach (int cp in logical)
            {
                string k = CharKind(cp);
                kinds.Add(k);
                cps.Add(cp);
            }
            // group into runs of equal kind
            var runKinds = new List<string>();
            var runCps = new List<List<int>>();
            var cur = new List<int>();
            string curKind = null;
            for (int i = 0; i < cps.Count; i++)
            {
                string k = kinds[i];
                if (cur.Count > 0 && k != curKind)
                {
                    runKinds.Add(curKind);
                    runCps.Add(cur);
                    cur = new List<int>();
                }
                curKind = k;
                cur.Add(cps[i]);
            }
            if (cur.Count > 0) { runKinds.Add(curKind); runCps.Add(cur); }

            // merge neutral runs into the previous run (or the next one if first)
            var mergedKinds = new List<string>();
            var mergedCps = new List<List<int>>();
            for (int i = 0; i < runKinds.Count; i++)
            {
                if ((runKinds[i] == "neu" || runKinds[i] == "ws") && mergedCps.Count > 0)
                {
                    mergedCps[mergedCps.Count - 1].AddRange(runCps[i]);
                }
                else
                {
                    mergedKinds.Add(runKinds[i]);
                    mergedCps.Add(new List<int>(runCps[i]));
                }
            }
            if (mergedKinds.Count > 1 && (mergedKinds[0] == "neu" || mergedKinds[0] == "ws"))
            {
                mergedCps[1].InsertRange(0, mergedCps[0]);
                mergedKinds.RemoveAt(0);
                mergedCps.RemoveAt(0);
            }

            // visual order: reverse run order, keep LTR runs internally forward
            var sb = new StringBuilder();
            for (int i = mergedKinds.Count - 1; i >= 0; i--)
            {
                if (mergedKinds[i] == "ltr")
                {
                    for (int c = 0; c < mergedCps[i].Count; c++) sb.Append((char)mergedCps[i][c]);
                }
                else
                {
                    for (int c = mergedCps[i].Count - 1; c >= 0; c--) sb.Append((char)mergedCps[i][c]);
                }
            }
            return sb.ToString();
        }

        private static string CharKind(int cp)
        {
            if (Tasheel.Contains(cp)) return "nsm";
            if (IsArabic(cp)) return "rtl";
            char ch = (char)cp;
            if (ch >= '0' && ch <= '9') return "ltr";
            if (ch >= 'A' && ch <= 'Z') return "ltr";
            if (ch >= 'a' && ch <= 'z') return "ltr";
            if (ch == ' ' || ch == '\t' || ch == '\u00A0') return "ws";
            return "neu";
        }

        private static bool IsLamAlef(int cp)
        {
            return cp == 0x0622 || cp == 0x0623 || cp == 0x0625 || cp == 0x0627;
        }
    }
}
