using System;
using System.Globalization;
using System.IO;
using System.Linq;
using MoreSailwindSails.BoatRigs;
using MoreSailwindSails.Controls;
using UnityEngine;

namespace MoreSailwindSails.Tests.GeometryChecks.FishermansStay;

internal static class BrigWinchChecks
{
    private static void Check(bool condition, string message)
    {
        if (!condition)
            throw new Exception(message);
    }

    private static Vector3 Parse(string text)
    {
        var values = text.Split(',')
            .Select(x => float.Parse(x, CultureInfo.InvariantCulture))
            .ToArray();
        return new Vector3(values[0], values[1], values[2]);
    }

    private static string[][] Measurements(string name) =>
        File.ReadLines(Path.Combine(AppContext.BaseDirectory, "FishermansStay", name))
            .Where(line => !line.StartsWith("#"))
            .Select(line => line.Split('|'))
            .ToArray();

    internal static void Run()
    {
        var faces = Measurements("BrigRailMeasurements.txt")
            .Select(f => new
            {
                Role = (WinchRole)Enum.Parse(typeof(WinchRole), f[0]),
                OuterFore = Parse(f[1]),
                InnerFore = Parse(f[2]),
                OuterAft = Parse(f[3]),
                InnerAft = Parse(f[4]),
            })
            .ToArray();
        var donors = Measurements("WinchMeasurements.txt")
            .Where(f => f[0] == Brig.Definition.BoatName && f[2] != "reef")
            .Select(f => new
            {
                Mount = Brig.Definition.WinchMount(
                    int.Parse(f[1]),
                    (WinchRole)Enum.Parse(typeof(WinchRole), f[2], true)
                ),
                Origin = Parse(f[3]),
                Normal = Parse(f[4]).normalized,
                Radius = float.Parse(f[6], CultureInfo.InvariantCulture),
            })
            .ToArray();

        // Independent measured face bounds: verify surface contact, cross-rail
        // centering and longitudinal clearance, not just agreement with authored lines.
        bool Supported(WinchPlacement candidate, Vector3 sourceNormal, float radius, WinchRole role)
        {
            var normal = candidate.Rotation * sourceNormal;
            var contact = candidate.Position - normal * 0.0766848f;
            return faces
                .Where(f => f.Role == role)
                .Any(f =>
                {
                    float t = (contact.z - f.OuterFore.z) / (f.OuterAft.z - f.OuterFore.z);
                    if (t < 0f || t > 1f)
                        return false;
                    var outer = Vector3.Lerp(f.OuterFore, f.OuterAft, t);
                    var inner = Vector3.Lerp(f.InnerFore, f.InnerAft, t);
                    var fore = (f.OuterFore + f.InnerFore) * 0.5f;
                    var aft = (f.OuterAft + f.InnerAft) * 0.5f;
                    var faceNormal = Vector3.Cross(Vector3.right, aft - fore).normalized;
                    return (contact - (outer + inner) * 0.5f).magnitude < 0.001f
                        && (contact - fore).magnitude >= radius - 0.001f
                        && (contact - aft).magnitude >= radius - 0.001f
                        && Vector3.Dot(normal, faceNormal) > 0.99999f;
                });
        }

        Check(donors.Length == 24, "Brig sheet donor coverage changed.");
        foreach (var donor in donors)
        {
            var candidates = WinchPlacementGeometry.Candidates(
                donor.Mount,
                donor.Origin,
                donor.Radius,
                donor.Origin
            );
            Check(candidates.Length > 0, "Brig sheet has no rail candidates.");
            foreach (var candidate in candidates)
            {
                Check(
                    Supported(candidate, donor.Normal, donor.Radius, donor.Mount.Role),
                    $"Brig {donor.Mount.Mast}/{donor.Mount.Role} leaves the solid rail cap."
                );
                Check(
                    (candidate.Position - donor.Origin).magnitude <= 1.4011f,
                    "Brig rail candidate escaped its bounded donor range."
                );
                // Stair rails are inside |x| < 1.86; side-cap centers remain outside 2.5 m.
                Check(
                    Math.Abs(candidate.Position.x) > 2.5f,
                    "Brig sheet entered the stair opening."
                );
            }
            var oldAcrossRail = new WinchPlacement(
                donor.Origin + Vector3.right * 0.53f,
                Quaternion.identity
            );
            Check(
                !Supported(oldAcrossRail, donor.Normal, donor.Radius, donor.Mount.Role),
                "Rail regression fixture accepts the old across-boat placement."
            );

            var reservations = new WinchReservations();
            var positions = candidates.Select(c => c.Position).ToArray();
            bool Obstructed(Vector3 p, float r) =>
                donors.Any(d =>
                    d.Mount.Mast == donor.Mount.Mast
                    && WinchReservations.Overlap(p, r, d.Origin, d.Radius)
                );
            int count = 0;
            while (
                reservations.Acquire(donor, new object(), positions, donor.Radius, Obstructed)
                != null
            )
                Check(++count <= positions.Length, "Rail capacity is unbounded.");
            Check(count >= 2, "Brig rail cannot serve two added controls beside its native donor.");
            Check(reservations.Count == count, "Rail exhaustion leaked a reservation.");
        }

        // The pictured two stay groups, with native and topmast fittings occupying
        // adjacent rail positions. Alternate donor identities must share the surface.
        var mixed = new WinchReservations();
        var installed = donors.Where(d => d.Mount.Mast == 20 || d.Mount.Mast == 22).ToArray();
        foreach (var donor in installed)
        {
            var positions = WinchPlacementGeometry
                .Candidates(donor.Mount, donor.Origin, donor.Radius, donor.Origin)
                .Select(c => c.Position)
                .ToArray();
            for (int sail = 0; sail < 2; sail++)
                Check(
                    mixed.Acquire(
                        new object(),
                        new object(),
                        positions,
                        donor.Radius,
                        (p, r) =>
                            donors.Any(d =>
                                (
                                    d.Mount.Mast == 20
                                    || d.Mount.Mast == 22
                                    || d.Mount.Mast == 64
                                    || d.Mount.Mast == 65
                                ) && WinchReservations.Overlap(p, r, d.Origin, d.Radius)
                            )
                    ) != null,
                    "Mixed Brig staysails collide with neighboring native fittings."
                );
        }

        var sample = donors[0];
        Check(
            WinchPlacementGeometry.Candidates(sample.Mount, sample.Origin, 3f, sample.Origin).Length
                == 0,
            "Oversized fitting escaped finite rail bounds."
        );
        Check(
            WinchPlacementGeometry
                .Candidates(
                    sample.Mount,
                    sample.Origin + Vector3.up * 10f,
                    sample.Radius,
                    sample.Origin
                )
                .Length == 0,
            "Distant donor escaped finite rail bounds."
        );
        Console.WriteLine(
            "PASS: all 24 Brig sheet mappings seat on measured rail caps, avoid stairs, share finite space and reject unsupported placements."
        );
    }
}
