using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using FishermansSail;
using UnityEngine;

internal static class StayChecks
{
    internal static void Run(string fixture = null)
    {
        var aft = new Vector3(0, 20, 0);
        var foreBottom = new Vector3(12, 2, 0);
        var foreTop = new Vector3(12, 25, 0);
        var fore = StayGeometry.AtHeight(foreBottom, foreTop, aft.y);
        Check(
            fore.x == 12 && fore.y == aft.y && fore.z == 0,
            "The fore attachment must match the aft height."
        );
        Check(
            Math.Abs(StayGeometry.Span(aft, fore) - 12) < 0.0001,
            "Mount length must be horizontal mast spacing."
        );
        Check(
            StayGeometry.SupportsHeight(foreBottom, foreTop, aft.y),
            "A taller foremast must accept the attachment."
        );
        Check(
            !StayGeometry.SupportsHeight(foreBottom, new Vector3(12, 18, 0), aft.y),
            "A short foremast must not accept a floating attachment."
        );

        // A raked spar: intersect its actual axis, rather than keeping the lower
        // attachment's X/Z or assuming fore/aft is a particular coordinate axis.
        var raked = StayGeometry.AtHeight(new Vector3(4, 0, 12), new Vector3(6, 30, 15), 15);
        Check(
            raked.x == 5 && raked.y == 15 && raked.z == 13.5f,
            "Raked mast intersection is wrong."
        );
        var shifted = StayGeometry.AtHeight(
            foreBottom + new Vector3(-50, 3, 90),
            foreTop + new Vector3(-50, 3, 90),
            aft.y + 3
        );
        Check(
            Math.Abs(StayGeometry.Span(aft + new Vector3(-50, 3, 90), shifted) - 12) < 0.0001,
            "Boat-frame translations must preserve the span."
        );
        Reject(() => StayGeometry.AtHeight(Vector3.zero, Vector3.right, 5));
        Reject(() => StayGeometry.AtHeight(Vector3.zero, foreTop, float.NaN));
        Reject(() => StayGeometry.Span(aft, aft));
        Reject(() => StayGeometry.Span(aft, foreTop));
        Reject(() => StayGeometry.MountIndex(-1));
        Reject(() => StayGeometry.MountIndex(128));

        // The stock brig mount has +X upward; its cloth lives in a differently
        // oriented child frame. Flatten the mount without flipping that roll.
        var frameUp = StayGeometry.FrameUp(new Vector3(0, 0, -1), new Vector3(-1, 0, 0));
        var right = Vector3.Cross(frameUp, new Vector3(-1, 0, 0));
        Check(right.y > 0.999f, "Flattening the stock stay must not flip the sail frame.");
        var projectedUp = StayGeometry.FrameUp(new Vector3(0.2f, 0, -1), new Vector3(-1, 0, 0));
        Check(
            Math.Abs(projectedUp.x) < 0.0001f && projectedUp.z < -0.999f,
            "The retained roll must be perpendicular to the stay."
        );
        Reject(() => StayGeometry.FrameUp(Vector3.right, Vector3.right));

        var indices = new HashSet<int>();
        for (int i = 0; i < StayGeometry.SourceIndexLimit; i++)
        {
            int index = StayGeometry.MountIndex(i);
            Check(
                index >= 128 && index < StayGeometry.MountCapacity && indices.Add(index),
                "Mount indices must be distinct and outside the source range."
            );
        }
        Check(
            StayGeometry.MountIndex(15) == 143 && StayGeometry.MountIndex(20) == 148,
            "Stable identities changed."
        );
        Check(StayGeometry.IsUpperStay("middle top stay 1-1"), "Brig upper stay was missed.");
        Check(StayGeometry.IsUpperStay("mid_stay_upper"), "Jong upper stay was missed.");
        Check(
            StayGeometry.IsUpperStay("mizzen top stay 2"),
            "Aft mast names must not restrict discovery."
        );
        Check(!StayGeometry.IsUpperStay("midstay_0-0_bottom"), "A lower stay was misclassified.");
        Check(!StayGeometry.IsUpperStay(null), "Missing names must not match.");
        Console.WriteLine(
            "PASS: horizontal stay placement, raked/short masts, alternate axes, stable mount identities, and invalid inputs."
        );

        if (fixture == null)
            return;
        using var doc = JsonDocument.Parse(File.ReadAllText(fixture));
        int candidates = 0,
            supported = 0;
        foreach (var item in doc.RootElement.EnumerateArray())
        {
            var a = Read(item, "aft");
            var bottom = Read(item, "foreBottom");
            var top = Read(item, "foreTop");
            var b = StayGeometry.AtHeight(bottom, top, a.y);
            Check(b.y == a.y && StayGeometry.Span(a, b) > 0, "Invalid local game stay geometry.");
            if (
                StayGeometry.SupportsHeight(bottom, top, a.y)
                && StayGeometry.SupportsHeight(Read(item, "aftBottom"), Read(item, "aftTop"), a.y)
            )
                supported++;
            candidates++;
        }
        Check(candidates > 0 && supported > 0, "Game fixture contains no usable upper stays.");
        Console.WriteLine(
            $"PASS: local game fixture ({supported}/{candidates} upper-stay variants support the shared attachment height)."
        );
    }

    private static Vector3 Read(JsonElement item, string key)
    {
        var a = item.GetProperty(key);
        return new Vector3(a[0].GetSingle(), a[1].GetSingle(), a[2].GetSingle());
    }

    private static void Check(bool value, string message)
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
        throw new Exception("Invalid stay geometry or identity was accepted.");
    }
}
