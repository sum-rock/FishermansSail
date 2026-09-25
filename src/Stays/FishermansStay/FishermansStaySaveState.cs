using System.Collections.Generic;

namespace MoreSailwindSails.Stays.FishermansStay
{
    internal static class FishermansStaySaveState
    {
        // A cancelled order can restore a snapshot made before this part existed.
        // Missing appended slots mean None, never the current preview's option.
        internal static int RestoreOption(IReadOnlyList<int> savedOptions, int partIndex) =>
            savedOptions != null && partIndex >= 0 && partIndex < savedOptions.Count
                ? savedOptions[partIndex]
                : 0;
    }
}
