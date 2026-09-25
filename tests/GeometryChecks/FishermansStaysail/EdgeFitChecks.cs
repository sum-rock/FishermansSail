using System;
using FishermansSail.Sails.FishermansStaysail;
using UnityEngine;

namespace FishermansSail.Tests.GeometryChecks.FishermansStaysail;

internal static class EdgeFitChecks
{
    internal static void Run()
    {
        var head = new Vector3(1, 0, 0);
        var tack = new Vector3(0, -1, 0);
        var clew = new Vector3(1, -1, 0);
        var points = new Vector3[33];
        foreach (float load in new[] { -1f, 0f, 1f })
        {
            if (
                !FishermansStaysailEdgeFit.Fit(
                    head,
                    clew,
                    tack,
                    Vector3.forward,
                    Vector3.down,
                    1,
                    load,
                    1,
                    1,
                    1,
                    points
                )
            )
                throw new Exception("Feasible bowed edge fit failed.");
            float bow = Vector3.Cross(points[16] - points[0], head - points[0]).magnitude;
            if (load == 0)
                BehaviorCases.Near(bow, 0, 1e-6f, "unloaded leech remains straight");
            else if (bow <= 1e-5f)
                throw new Exception("Loaded leech lost its support bow.");
            BehaviorCases.Near(
                (points[0] - tack).magnitude,
                0.99f,
                1e-5f,
                "bow retains foot tension"
            );
            BehaviorCases.Near((points[32] - head).magnitude, 0, 0, "bow retains fixed head");
            float length = 0;
            for (int i = 1; i < points.Length; i++)
                length += (points[i] - points[i - 1]).magnitude;
            if (!float.IsFinite(length) || length > 0.99001f)
                throw new Exception("Support bow exceeded its leech budget.");
        }

        foreach (float length in new[] { 0f, 0.1f })
        {
            Array.Fill(points, new Vector3(float.NaN, float.NaN, float.NaN));
            if (
                FishermansStaysailEdgeFit.Fit(
                    head,
                    clew,
                    tack,
                    Vector3.forward,
                    Vector3.down,
                    1,
                    1,
                    1,
                    length,
                    length,
                    points
                )
            )
                throw new Exception("Degenerate or impossible edge fit must report failure.");
            foreach (var point in points)
                if (!float.IsFinite(point.sqrMagnitude))
                    throw new Exception(
                        "Rejected fit must replace all points with a finite fallback."
                    );
            BehaviorCases.Near((points[32] - head).magnitude, 0, 0, "fallback retains fixed head");
        }
        Console.WriteLine(
            "PASS (executed): active support bow, coupled edge budgets and finite degenerate/impossible-fit fallback."
        );
    }
}
