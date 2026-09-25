using System;
using MoreSailwindSails.BoatRigs;
using UnityEngine;

namespace MoreSailwindSails.Controls
{
    internal static class WinchPlacementGeometry
    {
        internal static WinchPlacement[] Candidates(
            WinchMountDefinition definition,
            Vector3 origin,
            float radius,
            Vector3 axisPoint
        )
        {
            float spacing = Math.Max(0.35f, radius * 2f + 0.02f);
            var result = new System.Collections.Generic.List<WinchPlacement>();
            var offsets = definition.OnMast
                ? new[] { 1, -1, 2, 3 }
                : new[] { 1, -1, 2, -2, 3, -3, 4, -4 };
            foreach (int offset in offsets)
                if (
                    Math.Abs(offset * spacing) <= 1.401f
                    && (!definition.OnMast || offset * spacing >= -0.701f)
                )
                    result.Add(
                        new WinchPlacement(
                            origin + definition.Direction * (offset * spacing),
                            Quaternion.identity
                        )
                    );
            if (definition.OnMast)
            {
                // Overflow stays within the same height band, on another mast face.
                // Rotate the fitting and its radial position together, never just its position.
                var center =
                    axisPoint
                    + definition.Direction * Vector3.Dot(origin - axisPoint, definition.Direction);
                var radial = origin - center;
                foreach (float degrees in new[] { 90f, -90f, 180f })
                {
                    var rotation = WinchPlacement.Turn(definition.Direction, degrees);
                    foreach (int offset in new[] { 0, 1, -1, 2 })
                        if (Math.Abs(offset * spacing) <= 1.401f && offset * spacing >= -0.701f)
                            result.Add(
                                new WinchPlacement(
                                    center
                                        + rotation * radial
                                        + definition.Direction * (offset * spacing),
                                    rotation
                                )
                            );
                }
            }
            return result.ToArray();
        }
    }

    internal readonly struct WinchPlacement
    {
        internal readonly Vector3 Position;
        internal readonly Quaternion Rotation;

        internal WinchPlacement(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        internal static Quaternion Turn(Vector3 axis, float degrees)
        {
            double half = degrees * Math.PI / 360;
            var imaginary = axis * (float)Math.Sin(half);
            return new Quaternion(imaginary.x, imaginary.y, imaginary.z, (float)Math.Cos(half));
        }
    }
}
