using System;
using System.Collections.Generic;

namespace FishermansSail
{
    internal static class StayOrderText
    {
        internal const int LineWidth = 45;

        internal static bool NeedsWrapping(string line) =>
            line != null
            && line.Length > LineWidth
            && line.IndexOf("Triatic Stay", StringComparison.Ordinal) >= 0;

        internal static IEnumerable<string> Wrap(string line)
        {
            foreach (var paragraph in line.Replace("\r\n", "\n").Split('\n'))
            {
                string remaining = paragraph;
                while (remaining.Length > LineWidth)
                {
                    int split = remaining.LastIndexOf(' ', LineWidth);
                    if (split <= 0)
                        split = LineWidth;
                    yield return remaining.Substring(0, split);
                    remaining = remaining.Substring(split).TrimStart(' ');
                }
                yield return remaining;
            }
        }
    }
}
