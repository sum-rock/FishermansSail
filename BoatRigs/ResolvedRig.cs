using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FishermansSail
{
    internal sealed class ResolvedStay
    {
        internal StayVariantDefinition Definition;
        internal Mast Donor,
            Fore,
            Aft,
            HeightReference,
            FurlControl;
        internal Mast[] ForeSections,
            AftSections;
    }

    internal sealed class ResolvedStayGroup
    {
        internal BoatPart SourcePart;
        internal ResolvedStay[] Variants;

        // Resolve the complete profile before appending anything: a missing donor
        // must never shift later groups into earlier saved part slots.
        internal static ResolvedStayGroup[] Resolve(
            BoatRigDefinition definition,
            BoatPart[] parts,
            BoatRefs boat
        )
        {
            var masts = parts
                .SelectMany(p => p.partOptions)
                .Where(o => o)
                .Select(o => o.GetComponent<Mast>())
                .Where(m => m)
                .Distinct()
                .ToDictionary(m => m.orderIndex);
            Func<int, Mast> physical = id =>
            {
                if (
                    !masts.TryGetValue(id, out var mast)
                    || mast.onlyStaysails
                    || !mast.GetComponent<CapsuleCollider>()
                )
                    throw new InvalidOperationException(
                        $"Configured physical mast {id} is missing."
                    );
                return mast;
            };
            return definition
                .Groups.Select(group =>
                {
                    if (group.SourcePart >= parts.Length)
                        throw new InvalidOperationException(
                            $"Configured source part {group.SourcePart} is missing."
                        );
                    var part = parts[group.SourcePart];
                    return new ResolvedStayGroup
                    {
                        SourcePart = part,
                        Variants = group
                            .Variants.Select(v =>
                            {
                                if (
                                    !masts.TryGetValue(v.Donor, out var donor)
                                    || !donor.onlyStaysails
                                    || !part.partOptions.Contains(
                                        donor.GetComponent<BoatPartOption>()
                                    )
                                )
                                    throw new InvalidOperationException(
                                        $"Donor {v.Donor} is missing from part {group.SourcePart}."
                                    );
                                int index = StayGeometry.MountIndex(v.Donor);
                                if (
                                    !donor.walkColMast
                                    || !boat.walkCol
                                    || donor.mastHeight < 0.25f
                                    || (boat.masts.Length > index && boat.masts[index])
                                )
                                    throw new InvalidOperationException(
                                        $"Donor {v.Donor} has invalid geometry or occupied mount {index}."
                                    );
                                var result = new ResolvedStay
                                {
                                    Definition = v,
                                    Donor = donor,
                                    Fore = physical(v.Fore),
                                    Aft = physical(v.Aft),
                                    HeightReference = physical(v.HeightReference),
                                    FurlControl = physical(v.FurlControl),
                                    ForeSections = v.ForeSections.Select(physical).ToArray(),
                                    AftSections = v.AftSections.Select(physical).ToArray(),
                                };
                                var required = donor.GetComponent<BoatPartOption>().requires;
                                if (
                                    required == null
                                    || !required.Contains(
                                        result.Fore.GetComponent<BoatPartOption>()
                                    )
                                    || !required.Contains(result.Aft.GetComponent<BoatPartOption>())
                                )
                                    throw new InvalidOperationException(
                                        $"Donor {v.Donor} no longer requires its configured mast pair."
                                    );
                                foreach (
                                    var sections in new[]
                                    {
                                        result.ForeSections,
                                        result.AftSections,
                                    }
                                )
                                    for (int i = 1; i < sections.Length; i++)
                                        if (
                                            !(
                                                sections[i - 1]
                                                    .GetComponent<BoatPartOption>()
                                                    .requires
                                                ?? new List<BoatPartOption>()
                                            ).Contains(sections[i].GetComponent<BoatPartOption>())
                                        )
                                            throw new InvalidOperationException(
                                                $"Mast section {sections[i].orderIndex} is not a required continuation."
                                            );
                                return result;
                            })
                            .ToArray(),
                    };
                })
                .ToArray();
        }
    }
}
