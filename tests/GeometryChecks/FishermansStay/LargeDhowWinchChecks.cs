using System;
using System.Globalization;
using System.IO;
using System.Linq;
using MoreSailwindSails.BoatRigs;
using MoreSailwindSails.Controls;
using UnityEngine;

namespace MoreSailwindSails.Tests.GeometryChecks.FishermansStay;

internal static class LargeDhowWinchChecks
{
    internal static void Run()
    {
        Vector3 Parse(string text)
        {
            var values = text.Split(',')
                .Select(v => float.Parse(v, CultureInfo.InvariantCulture))
                .ToArray();
            return new Vector3(values[0], values[1], values[2]);
        }
        var profile = LargeDhow.Definition;
        var donors = File.ReadLines(
                Path.Combine(AppContext.BaseDirectory, "FishermansStay", "WinchMeasurements.txt")
            )
            .Where(line => line.StartsWith(profile.BoatName + "|", StringComparison.Ordinal))
            .Select(line => line.Split('|'))
            .Select(f => new
            {
                Id = int.Parse(f[1]),
                Role = (WinchRole)Enum.Parse(typeof(WinchRole), f[2], true),
                Origin = Parse(f[3]),
                Axis = Parse(f[5]),
                Radius = float.Parse(f[6], CultureInfo.InvariantCulture),
            })
            .ToArray();
        // Runtime checks every native fitting, not just the first entry in each
        // mast's control arrays. Include all rows, stay controls and old variants.
        var natives = File.ReadLines(
                Path.Combine(
                    AppContext.BaseDirectory,
                    "FishermansStay",
                    "LargeDhowNativeWinchMeasurements.txt"
                )
            )
            .Where(line => !line.StartsWith("#", StringComparison.Ordinal))
            .Select(line => line.Split('|'))
            .Select(f => new
            {
                Path = f[0],
                Origin = Parse(f[1]),
                Radius = float.Parse(f[2], CultureInfo.InvariantCulture),
            })
            .ToArray();
        if (natives.Length != 85 || natives.Select(n => n.Path).Distinct().Count() != 85)
            throw new Exception("Missing or duplicate native large-dhow obstruction measurements.");
        int checkedMasts = 0;
        foreach (var donor in donors.Where(d => d.Role == WinchRole.Reef))
        {
            var mount = profile.WinchMount(donor.Id, donor.Role);
            var candidates = WinchPlacementGeometry.Candidates(
                mount,
                donor.Origin,
                donor.Radius,
                donor.Axis
            );
            int expectedSource = new[] { 0, 1, 2, 4 }.Contains(donor.Id) ? 2 : -1;
            if (mount.SourceIndex != expectedSource)
                throw new Exception("Large dhow reef donor no longer matches the measured row.");
            bool Obstructed(Vector3 p, float radius) =>
                natives.Any(d => WinchReservations.Overlap(p, radius, d.Origin, d.Radius));
            if (donor.Id == 2 || donor.Id == 4)
            {
                // Reproduce the reported hidden halyard: the old lower-row
                // donor has no clear candidate against the complete native rig.
                var lower = natives.Single(n =>
                    n.Path.Contains("reef_winch (mast0", StringComparison.Ordinal)
                    && Math.Abs(n.Origin.z - donor.Origin.z) < 0.001f
                );
                var blocked = WinchPlacementGeometry.Candidates(
                    mount,
                    lower.Origin,
                    lower.Radius,
                    donor.Axis
                );
                if (blocked.Any(p => !Obstructed(p.Position, lower.Radius)))
                    throw new Exception(
                        "Missing-halyard regression no longer reproduces the blocked lower row."
                    );
            }
            var reservations = new WinchReservations();
            var positions = candidates.Select(c => c.Position).ToArray();
            var first = reservations.Acquire(
                donor,
                new object(),
                positions,
                donor.Radius,
                Obstructed
            );
            if (first == null)
                throw new Exception(
                    "Large dhow reef control blocked by native fittings: " + donor.Id
                );
            float height = Vector3.Dot(first.Position - donor.Origin, mount.Direction);
            if (height < -0.701f || height > 1.401f)
                throw new Exception(
                    "Large dhow control escaped the donor's bounded mounting band."
                );
            int count = 1;
            while (
                reservations.Acquire(donor, new object(), positions, donor.Radius, Obstructed)
                != null
            )
                if (++count > positions.Length)
                    throw new Exception("Large dhow mast reservations did not exhaust.");
            if (reservations.Count != count)
                throw new Exception("Large dhow mast exhaustion leaked a reservation.");
            checkedMasts++;
        }
        if (checkedMasts != 9)
            throw new Exception("Missing large dhow mast-control measurements.");
        Console.WriteLine(
            "PASS: nine large-dhow mast controls clear all 85 native fittings within bounded travel; both blocked lower-mainmast halyard cases reproduced."
        );
    }
}
