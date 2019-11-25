using System;
using System.Collections.Generic;

namespace vBot
{
    public static class Util
    {
        public static string GetNFirstChars(string str, int n)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            return str.Substring(0, Math.Min(str.Length, n));
        }

        public static int Levenshtein(string s1, string s2)
        {
            int n = s1.Length;
            int m = s2.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0)
                return m;
            if (m == 0)
                return n;

            for (int i = 0; i <= n; i++)
                d[i, 0] = i;
            for (int j = 0; j <= m; j++)
                d[0, j] = j;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (s2[j - 1] == s1[i - 1]) ? 0 : 1;

                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }

            return d[n, m];
        }

        public static T GetClosestElement<T>(string str, IReadOnlyCollection<T> collection, Func<T, string> f = null)
        {
            int dist = Int32.MaxValue;
            T ret = default(T);
            str = str.ToLower();
            foreach (var elt in collection)
            {
                int tmp = Util.Levenshtein(str, f == null ? elt.ToString().ToLower() : f(elt).ToLower());
                if (tmp == 0)
                    return elt;
                if (tmp >= dist)
                    continue;
                dist = tmp;
                ret = elt;
            }
            return ret;
        }
        
        public static string FormatText(string text, string style = "")
        {
            text = text.Replace("*", "");
            text = text.Replace("~", "");
            text = text.Replace("_", "");
            return style + text + style;
        }
    }
}
