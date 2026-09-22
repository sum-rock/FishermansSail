using System.Collections.Generic;
using System.Linq;

namespace FishermansSail
{
    internal static class StayRequirements
    {
        // Replace only the donor's endpoint requirements. Other prerequisites
        // still belong to the original rig; height and control donors must exist.
        internal static List<T> ForAttachments<T>(
            IEnumerable<T> original,
            T forePrimary,
            T aftPrimary,
            T foreAttachment,
            T aftAttachment,
            T heightReference,
            T furlControl
        ) =>
            original
                .Where(o =>
                    !EqualityComparer<T>.Default.Equals(o, forePrimary)
                    && !EqualityComparer<T>.Default.Equals(o, aftPrimary)
                )
                .Concat(new[] { foreAttachment, aftAttachment, heightReference, furlControl })
                .Distinct()
                .ToList();
    }
}
