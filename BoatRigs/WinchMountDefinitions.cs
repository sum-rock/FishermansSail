using System;
using System.Linq;
using UnityEngine;

namespace FishermansSail.BoatRigs
{
    internal enum WinchRole
    {
        Reef,
        Left,
        Right,
        Mid,
    }

    internal sealed class WinchMountDefinition
    {
        internal readonly string Boat;
        internal readonly int Mast;
        internal readonly WinchRole Role;
        internal readonly Vector3 Direction;
        internal readonly bool OnMast;
        internal readonly int Support;

        internal WinchMountDefinition(
            string boat,
            int mast,
            WinchRole role,
            Vector3 direction,
            bool onMast,
            int support
        )
        {
            Boat = boat;
            Mast = mast;
            Role = role;
            Direction = direction.normalized;
            OnMast = onMast;
            Support = support;
        }

        internal WinchPlacement[] Candidates(Vector3 origin, float radius, Vector3 axisPoint)
        {
            float spacing = Math.Max(0.35f, radius * 2f + 0.02f);
            var result = new System.Collections.Generic.List<WinchPlacement>();
            var offsets = OnMast ? new[] { 1, -1, 2, 3 } : new[] { 1, -1, 2, -2, 3, -3, 4, -4 };
            foreach (int offset in offsets)
                if (
                    Math.Abs(offset * spacing) <= 1.401f
                    && (!OnMast || offset * spacing >= -0.701f)
                )
                    result.Add(
                        new WinchPlacement(
                            origin + Direction * (offset * spacing),
                            Quaternion.identity
                        )
                    );
            if (OnMast)
            {
                // Overflow stays within the same height band, on another mast face.
                // Rotate the fitting and its radial position together, never just its position.
                var center = axisPoint + Direction * Vector3.Dot(origin - axisPoint, Direction);
                var radial = origin - center;
                foreach (float degrees in new[] { 90f, -90f, 180f })
                {
                    var rotation = WinchPlacement.Turn(Direction, degrees);
                    foreach (int offset in new[] { 0, 1, -1, 2 })
                        if (Math.Abs(offset * spacing) <= 1.401f && offset * spacing >= -0.701f)
                            result.Add(
                                new WinchPlacement(
                                    center + rotation * radial + Direction * (offset * spacing),
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

    internal static class WinchMountDefinitions
    {
        internal static WinchMountDefinition Find(string boat, int mast, WinchRole role)
        {
            string name = BoatRigCatalog.Find(boat)?.BoatName;
            return All.FirstOrDefault(d => d.Boat == name && d.Mast == mast && d.Role == role)
                ?? throw new InvalidOperationException(
                    $"No authored winch mounting direction: {boat}/{mast}/{role}."
                );
        }

        // Measured 2026-09-24 from level24, shipyard_expansion.assets, leopard,
        // and veil piercer. Directions are boat-local, not donor-wheel axes (which spin).
        // Native donor positions and face rotations remain the attachment datum.
        internal static readonly WinchMountDefinition[] All = Build();

        private static WinchMountDefinition[] Build()
        {
            var result = new System.Collections.Generic.List<WinchMountDefinition>();
            void Add(
                string boat,
                int mast,
                WinchRole role,
                float x,
                float y,
                float z,
                bool onMast,
                int support
            ) =>
                result.Add(
                    new WinchMountDefinition(
                        boat,
                        mast,
                        role,
                        new Vector3(x, y, z),
                        onMast,
                        support
                    )
                );
            // BOAT medi medium (50)
            Add("BOAT medi medium (50)", 2, WinchRole.Reef, 0f, 1f, 0f, true, 2);
            Add("BOAT medi medium (50)", 3, WinchRole.Reef, 0f, 1f, 0f, true, 3);
            Add("BOAT medi medium (50)", 4, WinchRole.Reef, 0f, 1f, 0f, true, 4);
            Add("BOAT medi medium (50)", 5, WinchRole.Reef, 0f, 1f, 0f, true, 5);
            Add("BOAT medi medium (50)", 6, WinchRole.Reef, 0f, 1f, 0f, true, 6);
            Add("BOAT medi medium (50)", 7, WinchRole.Reef, 0f, 1f, 0f, true, 7);
            Add("BOAT medi medium (50)", 15, WinchRole.Left, 0.992723f, 0f, 0.120421f, false, -1);
            Add("BOAT medi medium (50)", 15, WinchRole.Right, 0.992723f, 0f, 0.120421f, false, -1);
            Add("BOAT medi medium (50)", 16, WinchRole.Left, 0.992723f, 0f, 0.120421f, false, -1);
            Add("BOAT medi medium (50)", 16, WinchRole.Right, 0.992723f, 0f, 0.120421f, false, -1);
            Add("BOAT medi medium (50)", 18, WinchRole.Left, 0.992723f, 0f, 0.120421f, false, -1);
            Add("BOAT medi medium (50)", 18, WinchRole.Right, 0.992723f, 0f, 0.120421f, false, -1);
            Add("BOAT medi medium (50)", 20, WinchRole.Left, 0.992723f, 0f, 0.120421f, false, -1);
            Add("BOAT medi medium (50)", 20, WinchRole.Right, 0.992723f, 0f, 0.120421f, false, -1);
            Add("BOAT medi medium (50)", 22, WinchRole.Left, 0.943135f, 0f, 0.332409f, false, -1);
            Add("BOAT medi medium (50)", 22, WinchRole.Right, 0.992723f, 0f, 0.120421f, false, -1);
            Add("BOAT medi medium (50)", 24, WinchRole.Left, 0.943135f, 0f, 0.332409f, false, -1);
            Add("BOAT medi medium (50)", 24, WinchRole.Right, 0.992723f, 0f, 0.120421f, false, -1);
            Add("BOAT medi medium (50)", 56, WinchRole.Reef, 0f, 1f, 0f, true, 5);
            Add("BOAT medi medium (50)", 58, WinchRole.Reef, 0f, 1f, 0f, true, 58);
            Add("BOAT medi medium (50)", 59, WinchRole.Reef, 0f, 1f, 0f, true, 59);
            Add("BOAT medi medium (50)", 60, WinchRole.Reef, 0f, 1f, 0f, true, 60);
            Add("BOAT medi medium (50)", 61, WinchRole.Left, 0.549092f, 0f, -0.835762f, false, -1);
            Add("BOAT medi medium (50)", 61, WinchRole.Right, 0.992722f, 0f, 0.120428f, false, -1);
            Add("BOAT medi medium (50)", 62, WinchRole.Left, 0.549092f, 0f, -0.835762f, false, -1);
            Add("BOAT medi medium (50)", 62, WinchRole.Right, 0.992722f, 0f, 0.120428f, false, -1);
            Add("BOAT medi medium (50)", 63, WinchRole.Left, 0.549092f, 0f, -0.835762f, false, -1);
            Add("BOAT medi medium (50)", 63, WinchRole.Right, 0.992722f, 0f, 0.120428f, false, -1);
            Add("BOAT medi medium (50)", 64, WinchRole.Left, 0.549092f, 0f, -0.835762f, false, -1);
            Add("BOAT medi medium (50)", 64, WinchRole.Right, 0.992722f, 0f, 0.120428f, false, -1);
            Add("BOAT medi medium (50)", 65, WinchRole.Left, 0.943135f, 0f, 0.332409f, false, -1);
            Add("BOAT medi medium (50)", 65, WinchRole.Right, 0.992722f, 0f, 0.120426f, false, -1);
            Add("BOAT medi medium (50)", 66, WinchRole.Left, 0.943135f, 0f, 0.332409f, false, -1);
            Add("BOAT medi medium (50)", 66, WinchRole.Right, 0.992722f, 0f, 0.120426f, false, -1);
            // BOAT junk medium (80)
            Add("BOAT junk medium (80)", 5, WinchRole.Left, 0.50333f, 0f, -0.864094f, false, -1);
            Add("BOAT junk medium (80)", 5, WinchRole.Right, 0.482513f, 0f, 0.875889f, false, -1);
            Add("BOAT junk medium (80)", 6, WinchRole.Left, 0.50333f, 0f, -0.864094f, false, -1);
            Add("BOAT junk medium (80)", 6, WinchRole.Right, 0.482513f, 0f, 0.875889f, false, -1);
            Add("BOAT junk medium (80)", 7, WinchRole.Left, 0.414732f, 0f, -0.909944f, false, -1);
            Add("BOAT junk medium (80)", 7, WinchRole.Right, 0.68046f, 0f, 0.732785f, false, -1);
            Add("BOAT junk medium (80)", 9, WinchRole.Reef, 0f, 1f, 0f, true, 9);
            Add("BOAT junk medium (80)", 10, WinchRole.Reef, -0.942643f, 0f, 0.333801f, false, -1);
            Add("BOAT junk medium (80)", 11, WinchRole.Reef, -0.942643f, 0f, 0.333801f, false, -1);
            Add("BOAT junk medium (80)", 12, WinchRole.Reef, 0f, 1f, 0f, true, 12);
            Add("BOAT junk medium (80)", 13, WinchRole.Reef, 0f, 1f, 0f, true, 13);
            Add("BOAT junk medium (80)", 16, WinchRole.Left, -0.087094f, 0f, -0.9962f, false, -1);
            Add("BOAT junk medium (80)", 16, WinchRole.Right, 0.149905f, 0f, 0.9887f, false, -1);
            Add("BOAT junk medium (80)", 52, WinchRole.Left, 0.414732f, 0f, -0.909944f, false, -1);
            Add("BOAT junk medium (80)", 52, WinchRole.Right, 0.680461f, 0f, 0.732785f, false, -1);
            Add("BOAT junk medium (80)", 53, WinchRole.Reef, -0.000349f, 1f, 0f, true, 53);
            Add("BOAT junk medium (80)", 57, WinchRole.Reef, 0f, 1f, 0f, true, 57);
            Add("BOAT junk medium (80)", 58, WinchRole.Reef, 0f, 0.965926f, 0.258819f, true, 58);
            Add("BOAT junk medium (80)", 61, WinchRole.Left, -0.087092f, 0f, -0.9962f, false, -1);
            Add("BOAT junk medium (80)", 61, WinchRole.Right, 0.149906f, 0f, 0.9887f, false, -1);
            Add("BOAT junk medium (80)", 65, WinchRole.Left, 0.50333f, 0f, -0.864094f, false, -1);
            Add("BOAT junk medium (80)", 65, WinchRole.Right, 0.482512f, 0f, 0.87589f, false, -1);
            Add("BOAT junk medium (80)", 66, WinchRole.Left, 0.50333f, 0f, -0.864094f, false, -1);
            Add("BOAT junk medium (80)", 66, WinchRole.Right, 0.482512f, 0f, 0.87589f, false, -1);
            // BOAT junk large (70)
            Add("BOAT junk large (70)", 1, WinchRole.Reef, 0f, 1f, 0f, true, 1);
            Add("BOAT junk large (70)", 2, WinchRole.Reef, 0f, 1f, 0f, true, 2);
            Add("BOAT junk large (70)", 3, WinchRole.Reef, 0f, 1f, 0f, true, 3);
            Add("BOAT junk large (70)", 4, WinchRole.Reef, 0f, 1f, 0f, true, 4);
            Add("BOAT junk large (70)", 10, WinchRole.Left, -0.613855f, 0f, -0.789419f, false, -1);
            Add("BOAT junk large (70)", 10, WinchRole.Right, -0.613855f, 0f, -0.789419f, false, -1);
            Add("BOAT junk large (70)", 12, WinchRole.Left, -0.613855f, 0f, -0.789419f, false, -1);
            Add("BOAT junk large (70)", 12, WinchRole.Right, -0.613855f, 0f, -0.789419f, false, -1);
            Add("BOAT junk large (70)", 13, WinchRole.Left, -0.999546f, 0f, -0.030141f, false, -1);
            Add("BOAT junk large (70)", 13, WinchRole.Right, 0.999384f, 0f, -0.035098f, false, -1);
            Add("BOAT junk large (70)", 51, WinchRole.Reef, 0f, 1f, 0f, true, 51);
            Add("BOAT junk large (70)", 52, WinchRole.Reef, 0f, 1f, 0f, true, 52);
            Add("BOAT junk large (70)", 53, WinchRole.Reef, 0f, 1f, 0f, true, 53);
            Add("BOAT junk large (70)", 55, WinchRole.Left, -0.613851f, 0f, -0.789422f, false, -1);
            Add("BOAT junk large (70)", 55, WinchRole.Right, -0.613851f, 0f, -0.789422f, false, -1);
            Add("BOAT junk large (70)", 56, WinchRole.Left, -0.999546f, 0f, -0.030142f, false, -1);
            Add("BOAT junk large (70)", 56, WinchRole.Right, 0.999384f, 0f, -0.035099f, false, -1);
            Add("BOAT junk large (70)", 58, WinchRole.Left, -0.613851f, 0f, -0.789422f, false, -1);
            Add("BOAT junk large (70)", 58, WinchRole.Right, -0.613851f, 0f, -0.789422f, false, -1);
            Add("BOAT junk large (70)", 71, WinchRole.Left, -0.999546f, 0f, -0.030144f, false, -1);
            Add("BOAT junk large (70)", 71, WinchRole.Right, 0.999384f, 0f, -0.035099f, false, -1);
            Add("BOAT junk large (70)", 73, WinchRole.Left, -0.613851f, 0f, -0.789422f, false, -1);
            Add("BOAT junk large (70)", 73, WinchRole.Right, -0.613851f, 0f, -0.789422f, false, -1);
            Add("BOAT junk large (70)", 75, WinchRole.Left, -0.999546f, 0f, -0.030142f, false, -1);
            Add("BOAT junk large (70)", 75, WinchRole.Right, 0.999384f, 0f, -0.035099f, false, -1);
            // BOAT dhow medium (20)
            Add("BOAT dhow medium (20)", 9, WinchRole.Left, 0.849129f, 0f, -0.528186f, false, -1);
            Add("BOAT dhow medium (20)", 9, WinchRole.Right, 0.961436f, 0f, 0.275028f, false, -1);
            Add("BOAT dhow medium (20)", 10, WinchRole.Reef, 0f, 1f, 0f, true, 10);
            Add("BOAT dhow medium (20)", 11, WinchRole.Reef, 0f, 1f, 0f, true, 11);
            Add("BOAT dhow medium (20)", 12, WinchRole.Reef, 0f, 0.995601f, -0.093689f, true, 12);
            Add("BOAT dhow medium (20)", 14, WinchRole.Reef, 0f, 1f, 0f, true, 11);
            Add("BOAT dhow medium (20)", 51, WinchRole.Reef, 0f, 1f, 0f, true, 51);
            Add("BOAT dhow medium (20)", 54, WinchRole.Left, 0.035298f, 0f, -0.999377f, false, -1);
            Add("BOAT dhow medium (20)", 54, WinchRole.Right, 0.075087f, 0f, 0.997177f, false, -1);
            Add("BOAT dhow medium (20)", 55, WinchRole.Reef, 0f, 1f, 0f, true, 55);
            Add("BOAT dhow medium (20)", 57, WinchRole.Left, -0.496266f, 0f, -0.868171f, false, -1);
            Add("BOAT dhow medium (20)", 57, WinchRole.Right, -0.716543f, 0f, 0.697543f, false, -1);
            Add("BOAT dhow medium (20)", 58, WinchRole.Left, -0.496266f, 0f, -0.868171f, false, -1);
            Add("BOAT dhow medium (20)", 58, WinchRole.Right, -0.716543f, 0f, 0.697543f, false, -1);
            Add("BOAT dhow medium (20)", 59, WinchRole.Reef, 0f, 1f, 0f, true, 55);
            Add("BOAT dhow medium (20)", 60, WinchRole.Left, 0.035298f, 0f, -0.999377f, false, -1);
            Add("BOAT dhow medium (20)", 60, WinchRole.Right, 0.075087f, 0f, 0.997177f, false, -1);
            Add("BOAT dhow medium (20)", 61, WinchRole.Left, 0.035298f, 0f, -0.999377f, false, -1);
            Add("BOAT dhow medium (20)", 61, WinchRole.Right, 0.075087f, 0f, 0.997177f, false, -1);
            Add("BOAT dhow medium (20)", 62, WinchRole.Reef, 0f, 0.951057f, 0.309017f, true, 62);
            Add("BOAT dhow medium (20)", 66, WinchRole.Left, 0.035298f, 0f, -0.999377f, false, -1);
            Add("BOAT dhow medium (20)", 66, WinchRole.Right, 0.075087f, 0f, 0.997177f, false, -1);
            Add("BOAT dhow medium (20)", 67, WinchRole.Left, 0.035298f, 0f, -0.999377f, false, -1);
            Add("BOAT dhow medium (20)", 67, WinchRole.Right, 0.075087f, 0f, 0.997177f, false, -1);
            Add("BOAT dhow medium (20)", 68, WinchRole.Left, -0.496266f, 0f, -0.868171f, false, -1);
            Add("BOAT dhow medium (20)", 68, WinchRole.Right, -0.716543f, 0f, 0.697543f, false, -1);
            Add("BOAT dhow medium (20)", 69, WinchRole.Reef, 0f, 1f, 0f, true, 69);
            Add("BOAT dhow medium (20)", 70, WinchRole.Reef, 0f, 1f, 0f, true, 69);
            Add("BOAT dhow medium (20)", 71, WinchRole.Left, 0.035298f, 0f, -0.999377f, false, -1);
            Add("BOAT dhow medium (20)", 71, WinchRole.Right, 0.075087f, 0f, 0.997177f, false, -1);
            Add("BOAT dhow medium (20)", 79, WinchRole.Left, -0.496266f, 0f, -0.868171f, false, -1);
            Add("BOAT dhow medium (20)", 79, WinchRole.Right, -0.716543f, 0f, 0.697543f, false, -1);
            Add("BOAT dhow medium (20)", 80, WinchRole.Reef, 0f, 0.995595f, -0.093759f, true, 80);
            Add("BOAT dhow medium (20)", 81, WinchRole.Left, 0.035298f, 0f, -0.999377f, false, -1);
            Add("BOAT dhow medium (20)", 81, WinchRole.Right, 0.075087f, 0f, 0.997177f, false, -1);
            Add("BOAT dhow medium (20)", 82, WinchRole.Left, 0.035298f, 0f, -0.999377f, false, -1);
            Add("BOAT dhow medium (20)", 82, WinchRole.Right, 0.075087f, 0f, 0.997177f, false, -1);
            // BOAT medi small (40)
            Add("BOAT medi small (40)", 5, WinchRole.Reef, 0f, 1f, 0f, true, 5);
            Add("BOAT medi small (40)", 8, WinchRole.Reef, 0f, 1f, 0f, true, 8);
            Add("BOAT medi small (40)", 51, WinchRole.Left, 0.758075f, 0f, -0.652167f, false, -1);
            Add("BOAT medi small (40)", 51, WinchRole.Right, 0.545723f, 0f, 0.837966f, false, -1);
            Add("BOAT medi small (40)", 57, WinchRole.Reef, 0f, 1f, 0f, true, 57);
            Add("BOAT medi small (40)", 58, WinchRole.Left, 0.758075f, 0f, -0.652167f, false, -1);
            Add("BOAT medi small (40)", 58, WinchRole.Right, 0.545723f, 0f, 0.837966f, false, -1);
            Add("BOAT medi small (40)", 65, WinchRole.Left, 0.758075f, 0f, -0.652167f, false, -1);
            Add("BOAT medi small (40)", 65, WinchRole.Right, 0.545723f, 0f, 0.837966f, false, -1);
            // BOAT LEOPARD (207)
            Add("BOAT LEOPARD (207)", 7, WinchRole.Reef, -1f, 0f, -0.000002f, false, -1);
            Add("BOAT LEOPARD (207)", 8, WinchRole.Reef, -1f, 0f, -0.000002f, false, -1);
            Add("BOAT LEOPARD (207)", 9, WinchRole.Reef, -1f, 0f, -0.000002f, false, -1);
            Add("BOAT LEOPARD (207)", 10, WinchRole.Reef, -1f, 0f, -0.000002f, false, -1);
            Add("BOAT LEOPARD (207)", 11, WinchRole.Reef, 0f, 0.087156f, 0.996195f, false, -1);
            Add("BOAT LEOPARD (207)", 12, WinchRole.Reef, 0f, 0.087156f, 0.996195f, false, -1);
            Add("BOAT LEOPARD (207)", 13, WinchRole.Left, 0f, 0f, -1f, false, -1);
            Add("BOAT LEOPARD (207)", 13, WinchRole.Right, 0f, 0f, 1f, false, -1);
            Add("BOAT LEOPARD (207)", 17, WinchRole.Left, 0f, 0f, -1f, false, -1);
            Add("BOAT LEOPARD (207)", 17, WinchRole.Right, 0f, 0f, 1f, false, -1);
            Add("BOAT LEOPARD (207)", 18, WinchRole.Left, 0f, 0f, -1f, false, -1);
            Add("BOAT LEOPARD (207)", 18, WinchRole.Right, 0f, 0f, 1f, false, -1);
            Add("BOAT LEOPARD (207)", 19, WinchRole.Left, 0f, 0f, -1f, false, -1);
            Add("BOAT LEOPARD (207)", 19, WinchRole.Right, 0f, 0f, 1f, false, -1);
            // BOAT Shroud Large
            Add("BOAT Shroud Large", 7, WinchRole.Reef, -0.121869f, 0f, -0.992546f, false, -1);
            Add("BOAT Shroud Large", 8, WinchRole.Reef, -0.121869f, 0f, -0.992546f, false, -1);
            Add("BOAT Shroud Large", 9, WinchRole.Reef, -0.121869f, 0f, -0.992546f, false, -1);
            Add("BOAT Shroud Large", 10, WinchRole.Reef, -0.121869f, 0f, -0.992546f, false, -1);
            Add("BOAT Shroud Large", 24, WinchRole.Left, 0f, 0f, 1f, false, -1);
            Add("BOAT Shroud Large", 24, WinchRole.Right, 0f, 0f, 1f, false, -1);
            Add("BOAT Shroud Large", 25, WinchRole.Left, 0.99572f, 0f, 0.092419f, false, -1);
            Add("BOAT Shroud Large", 25, WinchRole.Right, 0.99572f, 0f, 0.092419f, false, -1);
            return result.ToArray();
        }
    }
}
