using System;
using System.Collections.Generic;

namespace FishermansSail.Stays.FishermansStay
{
    internal static class FishermansStayOrderText
    {
        internal const int LineWidth = 45;

        internal static bool NeedsWrapping(string line)
        {
            if (line == null || line.Length <= LineWidth)
                return false;
            const string name = "Fisherman's Stay";
            int index = line.IndexOf(name, StringComparison.Ordinal);
            while (index >= 0)
            {
                int end = index + name.Length;
                if (end == line.Length || !char.IsLetter(line[end]))
                    return true;
                index = line.IndexOf(name, end, StringComparison.Ordinal);
            }
            return false;
        }

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
