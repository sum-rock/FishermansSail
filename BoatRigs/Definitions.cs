using System;
using System.Collections.Generic;
using System.Linq;

namespace FishermansSail
{
    internal sealed class MastSupportDefinition
    {
        // Native rig ID supplying sheet winch assets; it need not be installed.
        internal readonly int SheetControlSource;

        // Sections are ordered from upper to lower. The last aft section
        // identifies the physical support when optional upper sections are absent.
        internal readonly int[] ForeSections,
            AftSections;

        internal MastSupportDefinition(
            int sheetControlSource,
            int[] foreSections,
            int[] aftSections
        )
        {
            if (
                sheetControlSource < 0
                || foreSections.Length == 0
                || aftSections.Length == 0
                || foreSections.Concat(aftSections).Any(id => id < 0)
                || foreSections.Intersect(aftSections).Any()
            )
                throw new ArgumentException("Invalid mast support or control source.");
            SheetControlSource = sheetControlSource;
            ForeSections = foreSections;
            AftSections = aftSections;
        }
    }

    internal sealed class BoatRigDefinition
    {
        internal readonly string BoatName;
        internal readonly MastSupportDefinition[] Supports;

        internal IEnumerable<IGrouping<int, MastSupportDefinition>> MastPairs(int foreIndex) =>
            Supports
                .Where(s => s.ForeSections.Contains(foreIndex))
                .GroupBy(s => s.AftSections.Last());

        internal BoatRigDefinition(string boatName, params MastSupportDefinition[] supports)
        {
            if (
                supports.Length == 0
                || supports.Select(s => s.SheetControlSource).Distinct().Count() != supports.Length
            )
                throw new ArgumentException("Empty mast supports or duplicate control source.");
            BoatName = boatName;
            Supports = supports;
        }
    }

    internal static partial class BoatRigCatalog
    {
        internal static BoatRigDefinition[] All =>
            new[] { Brig, Junk, Jong, Sanbuq, Cog, Leopard, Shroud };

        internal static BoatRigDefinition Find(string boatName)
        {
            if (boatName == null)
                return null;
            while (boatName.EndsWith("(Clone)", StringComparison.Ordinal))
                boatName = boatName.Substring(0, boatName.Length - 7).TrimEnd();
            return All.FirstOrDefault(d => d.BoatName == boatName);
        }
    }
}
