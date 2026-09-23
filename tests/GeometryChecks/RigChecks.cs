using System;
using System.Linq;
using FishermansSail;

internal static class RigChecks
{
    internal static void Run()
    {
        CheckAttachmentRequirements();
        OrderTextChecks.Run();
        // Recorded v0.5.3 appended group/option sequence. Save files use positions,
        // so sorting these groups globally or omitting one breaks compatibility.
        string[] names =
        {
            "BOAT medi medium (50)",
            "BOAT junk medium (80)",
            "BOAT junk large (70)",
            "BOAT dhow medium (20)",
            "BOAT medi small (40)",
            "BOAT LEOPARD (207)",
            "BOAT Shroud Large",
        };
        string[] layouts =
        {
            "8:15,16,18,20;10:22,24;26:61,62,63,64;27:65,66",
            "5:16,61;7:5,6;15:65,66",
            "7:10;9:12,55,58,73;11:71",
            "19:60,67,71,81,82;21:58,68,79;8:54,61,66",
            "6:51,58,65",
            "22:18;21:17;23:19",
            "21:25",
        };
        for (int i = 0; i < names.Length; i++)
        {
            var profile = BoatRigCatalog.Find(names[i]);
            Assert(
                profile != null && BoatRigCatalog.Find(names[i] + "(Clone)") == profile,
                "Boat lookup failed."
            );
            string actual = string.Join(
                ";",
                profile.Groups.Select(g =>
                    g.SourcePart + ":" + string.Join(",", g.Variants.Select(v => v.Donor))
                )
            );
            Assert(actual == layouts[i], "Saved part or option ordering changed: " + names[i]);
            foreach (var v in profile.Groups.SelectMany(g => g.Variants))
            {
                Assert(
                    StayGeometry.MountIndex(v.Donor) == 128 + v.Donor,
                    "Saved mount identity changed."
                );
                Assert(
                    v.HeightReference == (v.IsMizzen ? v.Aft : v.Fore),
                    "Wrong height reference."
                );
                Assert(v.FurlControl == v.HeightReference, "Wrong furl control mast.");
            }
        }
        Assert(
            BoatRigCatalog.Find("BOAT medi medium (50) custom") == null
                && BoatRigCatalog.Find(null) == null,
            "Unknown boats must not use guessed profiles."
        );
        var brig = BoatRigCatalog.Find(names[0]).Groups.SelectMany(g => g.Variants).ToArray();
        Assert(
            brig.Single(v => v.Donor == 61).AftSections.SequenceEqual(new[] { 56, 5 }),
            "Brig topmast lost its lower mainmast attachment."
        );
        Assert(
            brig.Single(v => v.Donor == 62).AftSections.SequenceEqual(new[] { 58, 4 }),
            "Alternate Brig topmast uses wrong lower section."
        );
        var rear = BoatRigCatalog
            .Find(names[2])
            .Groups.SelectMany(g => g.Variants)
            .Single(v => v.Donor == 71);
        Assert(
            rear.Fore == 52 && rear.Aft == 4 && rear.IsMizzen,
            "Jong rear mast pair is reversed."
        );
        Reject(() =>
            new BoatRigDefinition(
                "duplicate",
                new StayGroupDefinition(0, brig[0]),
                new StayGroupDefinition(1, brig[0])
            )
        );
        Reject(() => new StayVariantDefinition(1, 2, 2, false, 2, 2, new[] { 2 }, new[] { 2 }));
        Console.WriteLine(
            "PASS: seven boat profiles, saved group/option ordering, mount identities, explicit mast sections and unknown-boat handling."
        );
    }

    private static void CheckAttachmentRequirements()
    {
        var brig = BoatRigCatalog.Find("BOAT medi medium (50)");
        foreach (var variant in brig.Groups.Single(g => g.SourcePart == 26).Variants)
        {
            // Horizontal endpoints touch the lower mainmast, unlike the donors.
            var donorRequirements = new[] { variant.Fore, variant.Aft, 99 };
            var required = StayRequirements.ForAttachments(
                donorRequirements,
                variant.Fore,
                variant.Aft,
                variant.Fore,
                variant.AftSections[1],
                variant.HeightReference,
                variant.FurlControl
            );
            var installed = new[] { variant.Fore, variant.AftSections[1], 99 };
            Assert(
                required.All(installed.Contains),
                "Brig stay still requires the unused main topmast."
            );
            Assert(
                !required.All(new[] { variant.Fore, 99 }.Contains),
                "Missing lower mainmast must block installation."
            );
            Assert(required.Contains(99), "Unrelated donor prerequisites were dropped.");
            Assert(
                donorRequirements.SequenceEqual(new[] { variant.Fore, variant.Aft, 99 }),
                "Donor prerequisites were mutated."
            );
        }
        var mizzen = StayRequirements.ForAttachments(new[] { 4, 59 }, 4, 59, 4, 7, 59, 59);
        Assert(
            mizzen.OrderBy(i => i).SequenceEqual(new[] { 4, 7, 59 }),
            "A required height/furl topmast must remain required even when the endpoint touches a lower section."
        );
        var upper = StayRequirements.ForAttachments(new[] { 2, 58 }, 2, 58, 2, 58, 2, 2);
        Assert(
            upper.OrderBy(i => i).SequenceEqual(new[] { 2, 58 }),
            "A stay that actually touches the topmast must require it."
        );
        Console.WriteLine(
            "PASS: Brig attachment prerequisites, missing lower masts, retained height/control donors and untouched source requirements."
        );
    }

    private static void Assert(bool value, string message)
    {
        if (!value)
            throw new Exception(message);
    }

    private static void Reject(Action action)
    {
        try
        {
            action();
        }
        catch (ArgumentException)
        {
            return;
        }
        throw new Exception("Invalid rig definition was accepted.");
    }
}
