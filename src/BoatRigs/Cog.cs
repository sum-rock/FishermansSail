using System.Collections.Generic;
using UnityEngine;

namespace FishermansSail.BoatRigs
{
    internal static class Cog
    {
        internal static readonly BoatRigDefinition Definition = new BoatRigDefinition(
            "BOAT medi small (40)",
            Supports(),
            Stays(),
            MastParents(),
            WinchMounts()
        );

        private static MastSupportDefinition[] Supports() =>
            new[]
            {
                new MastSupportDefinition(51, new[] { 8 }, new[] { 5 }),
                new MastSupportDefinition(58, new[] { 5 }, new[] { 57 }),
                new MastSupportDefinition(65, new[] { 8 }, new[] { 6 }),
            };

        // Installed mast ancestry: -1 marks a physical base section.
        private static Dictionary<int, int> MastParents() =>
            new Dictionary<int, int>
            {
                { 6, -1 },
                { 5, -1 },
                { 8, -1 },
                { 57, -1 },
                { 56, -1 },
                { 68, -1 },
            };

        // Installed donor directions in boat space; provenance and numeric fixtures
        // are documented in docs/DEVELOPMENT.md under shared winch placement.
        private static WinchMountDefinition[] WinchMounts() =>
            new[]
            {
                new WinchMountDefinition(5, WinchRole.Reef, new Vector3(0f, 1f, 0f), true, 5),
                new WinchMountDefinition(8, WinchRole.Reef, new Vector3(0f, 1f, 0f), true, 8),
                new WinchMountDefinition(
                    51,
                    WinchRole.Left,
                    new Vector3(0.758075f, 0f, -0.652167f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    51,
                    WinchRole.Right,
                    new Vector3(0.545723f, 0f, 0.837966f),
                    false,
                    -1
                ),
                new WinchMountDefinition(57, WinchRole.Reef, new Vector3(0f, 1f, 0f), true, 57),
                new WinchMountDefinition(
                    58,
                    WinchRole.Left,
                    new Vector3(0.758075f, 0f, -0.652167f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    58,
                    WinchRole.Right,
                    new Vector3(0.545723f, 0f, 0.837966f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    65,
                    WinchRole.Left,
                    new Vector3(0.758075f, 0f, -0.652167f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    65,
                    WinchRole.Right,
                    new Vector3(0.545723f, 0f, 0.837966f),
                    false,
                    -1
                ),
            };

        // Authored from installed Cog and Shipyard Expansion assets.
        // Endpoint vectors are local to the named physical mast section.
        private static FishermansStayGroupDefinition[] Stays() =>
            new[]
            {
                new FishermansStayGroupDefinition(
                    "Mainmast / mizzenmast",
                    new[]
                    {
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            128,
                            51,
                            "main mast 2 / mizzen mast",
                            5,
                            new Vector3(-0.00000f, 0.00000f, -4.77700f),
                            8,
                            new Vector3(-0.00000f, -0.00391f, 1.14600f),
                            false,
                            0,
                            new[] { 5, 8 },
                            new int[0]
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            129,
                            65,
                            "main mast 1 / mizzen mast",
                            6,
                            new Vector3(-0.00000f, 0.00000f, -3.33567f),
                            8,
                            new Vector3(-0.00000f, -0.00391f, 1.14600f),
                            false,
                            0,
                            new[] { 6, 8 },
                            new int[0]
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            130,
                            58,
                            "main mast 2 / mizzen mast 2",
                            5,
                            new Vector3(0.00000f, 0.00000f, -4.25708f),
                            57,
                            new Vector3(-0.00000f, -0.00391f, 1.14600f),
                            false,
                            0,
                            new[] { 5, 57 },
                            new int[0]
                        ),
                    }
                ),
            };
    }
}
