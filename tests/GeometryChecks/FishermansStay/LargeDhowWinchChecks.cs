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
            // Include all measured native variants, even mutually exclusive ones.
            // In particular, mainmast controls must clear their topmast controls.
            bool Obstructed(Vector3 p, float radius) =>
                donors.Any(d => WinchReservations.Overlap(p, radius, d.Origin, d.Radius));
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
            if (donor.Id == 2 || donor.Id == 4)
            {
                float height = Vector3.Dot(first.Position - donor.Origin, mount.Direction);
                if (Math.Abs(height - 1.4f) > 0.0001f)
                    throw new Exception(
                        "Large dhow mainmast clearance no longer exercises the upper band end."
                    );
            }
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
            "PASS: nine large-dhow mast controls clear measured native neighbors, including topmast fittings, within finite placement bounds."
        );
    }
}
