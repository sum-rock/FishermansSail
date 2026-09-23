using System;
using FishermansSail;
using UnityEngine;

internal static class BillowChecks
{
    internal static void Run()
    {
        foreach (float width in new[] { 0.25f, 6f, 13.8f, 40f })
        {
            var data = FishermanGeometry.Create(width);
            foreach (float angle in new[] { -80f, -50f, -20f, 0f, 20f, 50f, 80f })
            foreach (float unroll in new[] { 0f, 0.02f, 0.5f, 0.75f, 0.9f, 0.98f, 1f })
            foreach (float load in new[] { -1f, 0f, 1f })
            foreach (float camber in new[] { -1f, -0.5f, 0f, 0.5f, 1f })
                CheckPose(data, width, angle, unroll, load, camber);
            Check(
                FishermanBillow.ClothTravel(width, 0.5f, 0) > width * 0.08f
                    && FishermanBillow.ClothTravel(width, 0.5f, 0) < width * 0.12f,
                "Top cloth must flex within its moving camber peak."
            );
            CheckSweep(width);
            CheckFurlSweep(width);
            CheckClewTaper(width);
            var invalid = new Vector3[FishermanGeometry.Rows + 1];
            Check(
                !FishermanTension.Fit(
                    Vector3.zero,
                    Vector3.zero,
                    Vector3.zero,
                    Vector3.zero,
                    width,
                    width,
                    1,
                    invalid
                ),
                "Coincident anchors must return a guarded failure."
            );
            foreach (var point in invalid)
                Near(point, Vector3.zero, width, "A degenerate fit must remain finite.");
            Check(
                !FishermanTension.Fit(
                    Vector3.zero,
                    Vector3.zero,
                    Vector3.right * (5 * width),
                    Vector3.zero,
                    width,
                    width,
                    1,
                    invalid
                ),
                "Impossible anchor spans must not masquerade as fitted cloth."
            );
            Near(
                invalid[invalid.Length - 1],
                Vector3.zero,
                width,
                "An impossible fit must retain the fixed head."
            );
        }
        float previous = 1;
        for (int i = 0; i < 100; i++)
        {
            float next = FishermanBillow.SmoothLoad(previous, -1, 1f / 60);
            Check(
                next <= previous && next >= -1 && previous - next < 0.1f,
                "Wind changes must remain smooth without overshooting."
            );
            previous = next;
        }
        Check(previous < -0.98f, "Wind response failed to settle on the new tack.");
        Check(
            FishermanBillow.SmoothLoad(0, float.NaN, 0.016f) == 0,
            "Invalid wind must not corrupt attachment transforms."
        );
        Console.WriteLine(
            "PASS: moving upper head, free leech skin targets, coupled foot/leech tension, mirrored tacks, rotated/scaled rigs, furling, and cambered skinning."
        );
    }

    private static void CheckPose(
        SailMeshData data,
        float width,
        float angle,
        float unroll,
        float load,
        float camber
    )
    {
        var rest = data.Corners;
        var requested = HoistPose.Corner(rest, 3, unroll);
        requested = FlyingSailGeometry.RotateAroundMast(requested, rest[0], Vector3.right, angle);
        var head = MovingHead(rest, angle, unroll);
        Near(
            head,
            FlyingSailGeometry.RotateAroundMast(
                HoistPose.Corner(rest, 1, unroll),
                rest[0],
                Vector3.right,
                angle * 0.85f * FishermanBillow.Deployment(unroll)
            ),
            width,
            "Upper corner did not follow the specified sheet angle."
        );
        Check(
            Math.Abs(
                (head - HoistPose.Corner(rest, 0, unroll)).magnitude
                    - width * MastInstallationGeometry.HoistScale(unroll)
            )
                < width * 1e-5f,
            "Moving the head stretched the top span."
        );
        var bow = FishermanBillow.SupportBow(
            requested,
            head,
            Vector3.up,
            new Vector3(-1, 0, 0),
            width,
            load
        );
        bow *= FishermanBillow.Deployment(unroll);
        float restLength = width * MastInstallationGeometry.HoistScale(unroll);
        var points = new Vector3[FishermanGeometry.Rows + 1];
        var tack = HoistPose.Corner(rest, 2, unroll);
        float footLength = (HoistPose.Corner(rest, 3, unroll) - tack).magnitude;
        float deployment = FishermanBillow.Deployment(unroll);
        float reserve = 1 - 0.01f * deployment;
        Check(
            FishermanTension.Fit(
                requested,
                head,
                tack,
                bow,
                restLength,
                footLength,
                deployment,
                points
            ),
            "A supported sail must fit both edges."
        );
        Near(
            points[points.Length - 1],
            head,
            width,
            "Upper aft corner left its sheet-driven target."
        );
        Check(
            Math.Abs((points[0] - tack).magnitude - footLength * reserve) < width * 1e-4f,
            "Leech correction introduced excess foot slack or stretched the foot."
        );
        float length = 0;
        for (int i = 1; i < points.Length; i++)
        {
            float segment = (points[i] - points[i - 1]).magnitude;
            length += segment;
            Check(
                segment <= restLength / FishermanGeometry.Rows * 1.001f,
                "A leech skin-target segment stretched beyond its available cloth."
            );
        }
        Check(
            Math.Abs(length - restLength * reserve) < width * 0.0005f,
            "The leech must retain its length while foot tension is maintained."
        );
        if (angle != 0 && unroll >= 0.98f)
            Check(
                Math.Abs(points[0].y) > width * 0.05f,
                "The moving head must allow useful outward clew movement."
            );

        // The same calculation must work after translation, scaling and mast/boat rotation.
        var transformed = new Vector3[points.Length];
        Vector3 Rotate(Vector3 point) =>
            FlyingSailGeometry.RotateAroundMast(
                FlyingSailGeometry.RotateAroundMast(point, Vector3.zero, Vector3.up, 12),
                Vector3.zero,
                Vector3.forward,
                25
            );
        var offset = new Vector3(10, 20, -30);
        const float scale = 0.55f;
        Check(
            FishermanTension.Fit(
                offset + Rotate(requested * scale),
                offset + Rotate(head * scale),
                offset + Rotate(tack * scale),
                Rotate(bow * scale),
                restLength * scale,
                footLength * scale,
                deployment,
                transformed
            ),
            "Transformed fit failed."
        );
        for (int i = 0; i < points.Length; i++)
            Near(
                transformed[i],
                offset + Rotate(points[i] * scale),
                width,
                "Moving or scaling the boat changed the fitted curve."
            );

        var mirrored = new Vector3[points.Length];
        Check(
            FishermanTension.Fit(
                new Vector3(requested.x, -requested.y, requested.z),
                new Vector3(head.x, -head.y, head.z),
                tack,
                new Vector3(bow.x, -bow.y, bow.z),
                restLength,
                footLength,
                deployment,
                mirrored
            ),
            "Mirrored fit failed."
        );
        for (int i = 0; i < points.Length; i++)
            Near(
                mirrored[i],
                new Vector3(points[i].x, -points[i].y, points[i].z),
                width,
                "The moving attachment must work identically on the opposite tack."
            );

        // Reconstruct the actual translation-only skin, including camber offsets.
        // Skin rest offsets rotate with the native sail frame before the
        // procedural bones are positioned in world space.
        Vector3 SkinRest(Vector3 p) =>
            FlyingSailGeometry.RotateAroundMast(p, rest[0], Vector3.right, angle);
        var bones = new Vector3[data.BonePositions.Length];
        bones[0] = HoistPose.Corner(rest, 0, unroll);
        bones[2] = HoistPose.Corner(rest, 2, unroll);
        for (int row = 0; row <= FishermanGeometry.Rows; row++)
            bones[FishermanGeometry.LeechBone(row)] = points[FishermanGeometry.Rows - row];
        var camberNormal = FishermanBillow.CamberNormal(bones[0], bones[2], bones[1], bones[3]);
        for (int row = 0; row <= FishermanGeometry.Rows; row++)
        for (int col = 0; col < FishermanGeometry.ShapeColumns; col++)
        {
            int bone = FishermanGeometry.ShapeBone(row, col);
            if (bone == 0 || bone == 2)
                continue;
            float v = (float)row / FishermanGeometry.Rows;
            bones[bone] = FishermanBillow.ShapePoint(
                Vector3.Lerp(bones[0], bones[2], v),
                bones[FishermanGeometry.LeechBone(row)],
                camberNormal,
                width,
                (float)col / FishermanGeometry.ShapeColumns,
                v,
                camber * deployment
            );
        }
        var vertices = new Vector3[data.Vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            var w = data.Weights[i];
            var vertex = data.Vertices[i];
            vertices[i] =
                SkinRest(vertex)
                + (bones[w.boneIndex0] - SkinRest(data.BonePositions[w.boneIndex0])) * w.weight0
                + (bones[w.boneIndex1] - SkinRest(data.BonePositions[w.boneIndex1])) * w.weight1
                + (bones[w.boneIndex2] - SkinRest(data.BonePositions[w.boneIndex2])) * w.weight2
                + (bones[w.boneIndex3] - SkinRest(data.BonePositions[w.boneIndex3])) * w.weight3;
            Check(
                float.IsFinite(vertices[i].x)
                    && float.IsFinite(vertices[i].y)
                    && float.IsFinite(vertices[i].z),
                "Posed cloth contains a non-finite vertex."
            );
        }
        for (int row = 0; row <= FishermanGeometry.Rows; row++)
        {
            float v = (float)row / FishermanGeometry.Rows;
            var expectedFore =
                Vector3.Lerp(bones[0], bones[2], v)
                + camberNormal * (FishermanGeometry.RestCamber(width, 0, v) * camber * deployment);
            Near(
                vertices[row * (FishermanGeometry.Columns + 1)],
                expectedFore,
                width,
                "Forward-edge camber must follow the selected side and gather through furling."
            );
            Near(
                vertices[row * (FishermanGeometry.Columns + 1) + FishermanGeometry.Columns],
                points[FishermanGeometry.Rows - row],
                width,
                "Leech skin targets separated from the tension-fitted curve."
            );
        }
        for (int i = 0; i < data.Triangles.Length; i += 3)
        {
            var a = vertices[data.Triangles[i]];
            var b = vertices[data.Triangles[i + 1]];
            var c = vertices[data.Triangles[i + 2]];
            // A twisted panel can face past the boat's center plane, so a
            // negative projected Y normal is not itself an inverted triangle.
            var normal = Vector3.Cross(b - a, c - a);
            Check(
                normal.magnitude > width * width * 1e-8f,
                $"Collapsed triangle {i / 3}: angle={angle}, unroll={unroll}, load={load}, camber={camber}."
            );
            if (i % 6 == 0)
            {
                var nextA = vertices[data.Triangles[i + 3]];
                var nextB = vertices[data.Triangles[i + 4]];
                var nextC = vertices[data.Triangles[i + 5]];
                Check(
                    Vector3.Dot(normal, Vector3.Cross(nextB - nextA, nextC - nextA)) > 0,
                    $"Folded quad {i / 6}: angle={angle}, unroll={unroll}, load={load}, camber={camber}."
                );
            }
        }
    }

    private static void CheckSweep(float width)
    {
        var rest = FishermanGeometry.Create(width).Corners;
        foreach (float unroll in new[] { 0.5f, 0.75f, 0.8f, 0.9f, 0.98f, 1f })
        foreach (float load in new[] { -1f, 0f, 1f })
        {
            Vector3[] previous = null;
            var tack = HoistPose.Corner(rest, 2, unroll);
            var restClew = HoistPose.Corner(rest, 3, unroll);
            float footLength = (restClew - tack).magnitude;
            float deployment = FishermanBillow.Deployment(unroll);
            for (int angle = -80; angle <= 80; angle++)
            {
                var requested = FlyingSailGeometry.RotateAroundMast(
                    restClew,
                    rest[0],
                    Vector3.right,
                    angle
                );
                var head = MovingHead(rest, angle, unroll);
                var bow =
                    FishermanBillow.SupportBow(
                        requested,
                        head,
                        Vector3.up,
                        new Vector3(-1, 0, 0),
                        width,
                        load
                    ) * deployment;
                var points = new Vector3[FishermanGeometry.Rows + 1];
                Check(
                    FishermanTension.Fit(
                        requested,
                        head,
                        tack,
                        bow,
                        width * MastInstallationGeometry.HoistScale(unroll),
                        footLength,
                        deployment,
                        points
                    ),
                    "Continuous trim sweep failed to fit."
                );
                Check(
                    Math.Abs((points[0] - tack).magnitude - footLength * (1 - 0.01f * deployment))
                        < width * 1e-4f,
                    "Trim sweep lost foot tension."
                );
                if (previous != null)
                    for (int i = 0; i < points.Length; i++)
                        Check(
                            (points[i] - previous[i]).magnitude < width * 0.025f,
                            "The leech skin target jumped during a one-degree trim change."
                        );
                previous = points;
            }
        }
    }

    private static void CheckFurlSweep(float width)
    {
        var rest = FishermanGeometry.Create(width).Corners;
        foreach (float angle in new[] { -80f, -20f, 20f, 80f })
        {
            Vector3[] previous = null;
            for (int step = 0; step <= 1000; step++)
            {
                float unroll = step / 1000f;
                var head = MovingHead(rest, angle, unroll);
                var clew = HoistPose.Corner(rest, 3, unroll);
                var tack = HoistPose.Corner(rest, 2, unroll);
                float footLength = (clew - tack).magnitude;
                clew = FlyingSailGeometry.RotateAroundMast(clew, rest[0], Vector3.right, angle);
                var bow = FishermanBillow.SupportBow(
                    clew,
                    head,
                    Vector3.up,
                    new Vector3(-1, 0, 0),
                    width,
                    1
                );
                float deployment = FishermanBillow.Deployment(unroll);
                var points = new Vector3[FishermanGeometry.Rows + 1];
                Check(
                    FishermanTension.Fit(
                        clew,
                        head,
                        tack,
                        bow * deployment,
                        width * MastInstallationGeometry.HoistScale(unroll),
                        footLength,
                        deployment,
                        points
                    ),
                    "Furl transition lost its feasible corner span."
                );
                if (previous != null)
                    for (int i = 0; i < points.Length; i++)
                        Check(
                            (points[i] - previous[i]).magnitude < width * 0.015f,
                            "Aft edge jumped during a 0.1-percent furl change."
                        );
                previous = points;
            }
        }
    }

    private static Vector3 MovingHead(Vector3[] rest, float angle, float unroll) =>
        FlyingSailGeometry.UpperHead(
            HoistPose.Corner(rest, 1, unroll),
            FlyingSailGeometry.RotateAroundMast(
                HoistPose.Corner(rest, 1, unroll),
                rest[0],
                Vector3.right,
                angle
            ),
            rest[0],
            Vector3.right,
            unroll
        );

    private static void CheckClewTaper(float width)
    {
        float previous = 0;
        for (int i = 0; i <= 20; i++)
        {
            float distance = i / 100f;
            float taper = FishermanBillow.ClewTaper(1 - distance, 1);
            Check(
                taper >= previous && taper >= 0 && taper <= 1,
                "Clew reinforcement must transition smoothly out into the foot."
            );
            previous = taper;
        }
        Check(
            FishermanBillow.ClewTaper(1, 1) == 0 && Math.Abs(previous - 1) < 1e-5f,
            "Clew taper must span the final twenty percent of the panel."
        );
        for (int i = 0; i <= 32; i++)
        {
            float t = i / 32f;
            Check(
                FishermanBillow.ClewTaper(t, 0) == 1 && FishermanBillow.ClewTaper(0, t) == 1,
                "Reinforcing the clew must not reduce top or forward-edge billow."
            );
        }
        float near = FishermanBillow.ClothTravel(width, 23f / 24, 1);
        Check(
            near > 0 && near < width * 0.005f,
            "The first foot vertex beside the clew needs a small nonzero movement allowance."
        );
        float boundary = FishermanBillow.ClothTravel(width, 0.8f, 1);
        float inside = FishermanBillow.ClothTravel(width, 0.80001f, 1);
        Check(
            Math.Abs(boundary - inside) < width * 1e-4f,
            "Clew taper introduced a discontinuity at the reinforcement boundary."
        );
    }

    private static void Near(Vector3 a, Vector3 b, float width, string message) =>
        Check((a - b).magnitude < Math.Max(width * 1e-4f, 1e-5f), message);

    private static void Check(bool value, string message)
    {
        if (!value)
            throw new Exception(message);
    }
}
