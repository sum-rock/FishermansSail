using System;
using System.Collections.Generic;
using System.Linq;
using MoreSailwindSails.BoatRigs;

namespace MoreSailwindSails.Tests.GeometryChecks.FishermansStay;

internal static class ProfileChecks
{
    internal static void Run()
    {
        var leopard = Leopard.Definition;
        if (
            !ReferenceEquals(BoatRigCatalog.Find("BOAT LEOPARD (207)(Clone)(Clone)"), leopard)
            || !leopard.Sections(12).SequenceEqual(new[] { 12, 11, 10 })
            || leopard.Base(12) != 10
            || !Brig.Definition.Sections(56).SequenceEqual(new[] { 56, 5 })
            || Brig.Definition.Sections(-1).Length != 0
        )
            throw new Exception("Profile lookup or ordered mast ancestry changed.");

        Reject<ArgumentException>(() => leopard.Sections(127));
        Reject<ArgumentException>(() => leopard.Base(127));
        Reject<InvalidOperationException>(() => leopard.WinchMount(127, WinchRole.Reef));
        foreach (var boat in BoatRigCatalog.All)
        foreach (var winch in boat.WinchMounts)
            if (!ReferenceEquals(boat.WinchMount(winch.Mast, winch.Role), winch))
                throw new Exception("Winch lookup escaped its containing boat profile.");

        BoatRigDefinition Profile(
            IReadOnlyDictionary<int, int> parents,
            params WinchMountDefinition[] winches
        ) =>
            new BoatRigDefinition(
                "test",
                Brig.Definition.Supports,
                Array.Empty<FishermansStayGroupDefinition>(),
                parents,
                winches
            );

        Reject<ArgumentException>(() => Profile(new Dictionary<int, int> { { 0, 1 } }));
        Reject<ArgumentException>(() => Profile(new Dictionary<int, int> { { 0, 0 } }));
        Reject<ArgumentException>(() => Profile(new Dictionary<int, int> { { 0, 1 }, { 1, 0 } }));
        Reject<ArgumentException>(() => Profile(new Dictionary<int, int> { { 0, -2 } }));
        var parents = new Dictionary<int, int> { { 0, -1 } };
        var profile = Profile(parents);
        parents[0] = 0;
        if (!profile.Sections(0).SequenceEqual(new[] { 0 }))
            throw new Exception("Caller mutation corrupted validated mast ancestry.");
        var sample = Brig.Definition.WinchMounts[0];
        Reject<ArgumentException>(() => Profile(new Dictionary<int, int>(), sample, sample));
        Console.WriteLine(
            "PASS: complete boat-profile lookup, ordered mast ancestry, missing entries, duplicate winches and cyclic/missing-parent rejection."
        );
    }

    private static void Reject<T>(Action action)
        where T : Exception
    {
        try
        {
            action();
        }
        catch (T)
        {
            return;
        }
        throw new Exception("Expected " + typeof(T).Name + " for invalid profile lookup/data.");
    }
}
