using System;
using MoreSailwindSails.Sails.FishermansFlyingSail;
using UnityEngine;

namespace MoreSailwindSails.Tests.GeometryChecks.FishermansFlyingSail;

internal static class AerodynamicChecks
{
    internal static void Run()
    {
        foreach (float width in new[] { 0.25f, 6f, 13.8f, 40f })
        foreach (float trim in new[] { -70f, -40f, -15f, 15f, 40f, 70f })
        {
            var cut = FishermansFlyingSailGeometry.Create(width).Corners;
            Vector3 World(Vector3 point) => new Vector3(0, point.x, point.z + width);
            var foreHead = World(cut[0]);
            var foreTack = World(cut[2]);
            var aftHead = FishermansFlyingSailFrameGeometry.UpperHead(
                World(cut[1]),
                FishermansFlyingSailFrameGeometry.RotateAroundMast(
                    World(cut[1]),
                    foreHead,
                    Vector3.up,
                    trim
                ),
                foreHead,
                Vector3.up,
                1
            );
            var requestedClew = FishermansFlyingSailFrameGeometry.RotateAroundMast(
                World(cut[3]),
                Vector3.zero,
                Vector3.up,
                trim
            );
            var leech = new Vector3[FishermansFlyingSailGeometry.Rows + 1];
            Check(
                FishermansFlyingSailTension.Fit(
                    requestedClew,
                    aftHead,
                    foreTack,
                    Vector3.zero,
                    (cut[1] - cut[3]).magnitude,
                    (World(cut[3]) - foreTack).magnitude,
                    1,
                    leech
                ),
                "Aerodynamic fixture must preserve both cloth edges."
            );
            Check(
                FishermansFlyingSailAerodynamics.TryFrame(
                    foreHead,
                    foreTack,
                    aftHead,
                    leech[0],
                    out var frame
                ),
                "A deployed sail must have a usable wind frame."
            );
            Near(frame.MastAxis, Vector3.up, "Wind sensor's forward axis must follow the mast.");
            Check(
                Math.Abs(Vector3.Dot(frame.MastAxis, frame.AlongSail)) < 1e-5f
                    && Math.Abs(Vector3.Dot(frame.Normal, frame.AlongSail)) < 1e-5f
                    && Math.Abs(frame.Normal.magnitude - 1) < 1e-5f,
                "Aerodynamic frame axes must be orthonormal."
            );
            Near(
                Vector3.Cross(frame.Normal, frame.MastAxis),
                frame.AlongSail,
                "LookRotation must put the wind sensor's right axis along the sail."
            );
            var usefulWind = frame.Normal * Math.Sign(frame.Normal.z);
            Check(
                NativeForwardFraction(usefulWind, frame, 1, 1) > 0.1f,
                "A useful broadside load must generate forward force on either tack."
            );
            Check(
                NativeForwardFraction(frame.AlongSail, frame, 1, 1) == 0,
                "An edge-on sail must still luff rather than receive artificial power."
            );
            Check(
                NativeForwardFraction(usefulWind, frame, 0, 1) == 0
                    && NativeForwardFraction(usefulWind, frame, 1, 0) == 0,
                "The native furl and wind-shadow multipliers must still suppress force."
            );
            // Reproduce the donor-axis mismatch: right ended up along the mast,
            // while forward pointed normal to the cloth. Broadside wind then
            // produces a negative angle and trips the native 13-degree gate.
            Check(
                NativeAngle(usefulWind, frame.MastAxis, frame.Normal) < 0,
                "The zero-force regression reproducer no longer matches the old axes."
            );
            Check(
                Math.Abs(NativeAngle(usefulWind, frame.AlongSail, frame.MastAxis) - 90) < 0.01f,
                "Corrected axes must recognize broadside wind."
            );
            Near(
                FishermansFlyingSailAerodynamics.ForceDirection(-usefulWind, frame.Normal),
                -usefulWind,
                "Force direction must switch with the loaded face of the sail."
            );
            Check(
                frame.Center.y < 0 && frame.Center.y > foreTack.y,
                "The application point must lie within the deployed sail height."
            );

            Vector3 Rotate(Vector3 p) =>
                FishermansFlyingSailFrameGeometry.RotateAroundMast(
                    FishermansFlyingSailFrameGeometry.RotateAroundMast(
                        p,
                        Vector3.zero,
                        Vector3.right,
                        8
                    ),
                    Vector3.zero,
                    Vector3.forward,
                    -25
                );
            var offset = new Vector3(13, -5, 9);
            Vector3 Move(Vector3 p) => Rotate(p * 0.55f) + offset;
            Check(
                FishermansFlyingSailAerodynamics.TryFrame(
                    Move(foreHead),
                    Move(foreTack),
                    Move(aftHead),
                    Move(leech[0]),
                    out var moved
                ),
                "Rake, heel and scaling must preserve a valid frame."
            );
            Near(moved.AlongSail, Rotate(frame.AlongSail), "Boat movement changed the chord axis.");
            Near(moved.Normal, Rotate(frame.Normal), "Boat movement changed the force normal.");
            Near(moved.Center, Move(frame.Center), "Centroid failed to follow the posed sail.");
        }
        Check(
            !FishermansFlyingSailAerodynamics.TryFrame(
                Vector3.zero,
                Vector3.zero,
                Vector3.zero,
                Vector3.zero,
                out _
            ),
            "An uninitialized sail must use the guarded fallback."
        );
        for (int col = 0; col <= FishermansFlyingSailGeometry.Columns; col++)
        for (int row = 0; row <= FishermansFlyingSailGeometry.Rows; row++)
        {
            float travel = FishermansFlyingSailBillow.ClothTravel(
                1,
                (float)col / FishermansFlyingSailGeometry.Columns,
                (float)row / FishermansFlyingSailGeometry.Rows
            );
            Check(
                travel >= -1e-6f && travel <= 0.28f,
                "Cloth travel must stay bounded rather than restoring the crumpling allowance."
            );
        }
        Check(
            FishermansFlyingSailBillow.ClothTravel(1, 1, 0.5f) <= 0.061f,
            "The free leech must have bounded movement around its fitted outline."
        );
        Console.WriteLine(
            "PASS: reproduced donor-axis zero-force bug, corrected wind capture, forward force on both tacks, native luff/furl/shadow behavior, posed centroid and bounded cloth travel."
        );
    }

    // Independent transcription of the inspected game's GetWindAngle and
    // UpdateWindForceOnSail formulas. Production retains those native methods;
    // these checks validate the geometry supplied to them, not a replacement.
    private static float NativeAngle(Vector3 wind, Vector3 right, Vector3 forward)
    {
        float along = Vector3.Angle(wind, right);
        float vertical = Vector3.Angle(wind, forward);
        if (along > 90)
            along = 180 - along;
        if (vertical > 90)
            vertical = 180 - vertical;
        return along * (vertical - (90 - vertical) * 0.44f) / 90;
    }

    private static float NativeForwardFraction(
        Vector3 wind,
        FishermansFlyingSailWindFrame frame,
        float unroll,
        float shadow
    )
    {
        const float upwind = 0.7f;
        float angle = NativeAngle(wind, frame.AlongSail, frame.MastAxis);
        float gate = Math.Max(0, Math.Min(1, (angle - 13) / 3));
        float capture =
            (upwind + (1 - upwind) * Math.Max(0, Math.Min(1, angle / 90))) * gate * unroll * shadow;
        var normal = FishermansFlyingSailAerodynamics.ForceDirection(wind, frame.Normal);
        float forceAngle = Vector3.Angle(normal, Vector3.forward);
        float sign = forceAngle > 90 ? -0.33f : 1;
        if (forceAngle > 90)
            forceAngle = 180 - forceAngle;
        float share = (90 - forceAngle) / 90;
        return sign * capture * (share + (1 - share) * upwind);
    }

    private static void Near(Vector3 a, Vector3 b, string message) =>
        Check((a - b).magnitude < 0.001f, message);

    private static void Check(bool value, string message)
    {
        if (!value)
            throw new Exception(message);
    }
}
