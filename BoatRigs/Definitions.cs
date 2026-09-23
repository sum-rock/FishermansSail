using System;
using System.Collections.Generic;
using System.Linq;

namespace FishermansSail
{
    internal sealed class StayVariantDefinition
    {
        internal readonly int Donor,
            Fore,
            Aft,
            HeightReference,
            FurlControl;
        internal readonly bool IsMizzen;
        internal readonly int[] ForeSections,
            AftSections;

        internal StayVariantDefinition(
            int donor,
            int fore,
            int aft,
            bool isMizzen,
            int heightReference,
            int furlControl,
            int[] foreSections,
            int[] aftSections
        )
        {
            StayGeometry.MountIndex(donor);
            if (
                fore == aft
                || foreSections.Length == 0
                || aftSections.Length == 0
                || foreSections[0] != fore
                || aftSections[0] != aft
                || foreSections.Intersect(aftSections).Any()
                || (heightReference != fore && heightReference != aft)
                || (furlControl != fore && furlControl != aft)
            )
                throw new ArgumentException("Invalid mast pair or attachment sections.");
            Donor = donor;
            Fore = fore;
            Aft = aft;
            IsMizzen = isMizzen;
            HeightReference = heightReference;
            FurlControl = furlControl;
            ForeSections = foreSections;
            AftSections = aftSections;
        }
    }

    internal sealed class StayGroupDefinition
    {
        internal readonly int SourcePart;
        internal readonly StayVariantDefinition[] Variants;

        internal StayGroupDefinition(int sourcePart, params StayVariantDefinition[] variants)
        {
            if (sourcePart < 0 || variants.Length == 0)
                throw new ArgumentException("Empty stay group.");
            SourcePart = sourcePart;
            Variants = variants;
        }
    }

    internal sealed class BoatRigDefinition
    {
        internal readonly string BoatName;
        internal readonly StayGroupDefinition[] Groups;

        // Historical stay donors describe physical mast pairs and supply control
        // assets. Their rigging options are no longer installation prerequisites.
        internal IEnumerable<IGrouping<int, StayVariantDefinition>> MastPairs(int foreIndex) =>
            Groups
                .SelectMany(g => g.Variants)
                .Where(v => v.ForeSections.Contains(foreIndex))
                .GroupBy(v => v.AftSections.Last());

        internal BoatRigDefinition(string boatName, params StayGroupDefinition[] groups)
        {
            var donors = groups.SelectMany(g => g.Variants).Select(v => v.Donor).ToArray();
            if (
                groups.Select(g => g.SourcePart).Distinct().Count() != groups.Length
                || donors.Distinct().Count() != donors.Length
            )
                throw new ArgumentException("Duplicate stay group or donor.");
            BoatName = boatName;
            Groups = groups;
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
