using System.Collections.Generic;
using UnityEngine;

namespace MoreSailwindSails.BoatRigs
{
    internal static class Junk
    {
        internal static readonly BoatRigDefinition Definition = new BoatRigDefinition(
            "BOAT junk medium (80)",
            Supports(),
            Stays(),
            MastParents(),
            WinchMounts()
        );

        private static MastSupportDefinition[] Supports() =>
            new[]
            {
                new MastSupportDefinition(16, new[] { 9 }, new[] { 10 }),
                new MastSupportDefinition(61, new[] { 58 }, new[] { 11 }),
                new MastSupportDefinition(5, new[] { 10 }, new[] { 12 }),
                new MastSupportDefinition(6, new[] { 11 }, new[] { 12 }),
                new MastSupportDefinition(65, new[] { 10 }, new[] { 53, 12 }),
                new MastSupportDefinition(66, new[] { 11 }, new[] { 53, 12 }),
            };

        // Installed mast ancestry: -1 marks a physical base section.
        private static Dictionary<int, int> MastParents() =>
            new Dictionary<int, int>
            {
                { 8, -1 },
                { 9, -1 },
                { 58, -1 },
                { 10, -1 },
                { 11, -1 },
                { 12, -1 },
                { 13, -1 },
                { 57, -1 },
                { 53, 12 },
                { 62, 9 },
            };

        // Installed donor directions in boat space; provenance and numeric fixtures
        // are documented in docs/DEVELOPMENT.md under shared winch placement.
        private static WinchMountDefinition[] WinchMounts() =>
            new[]
            {
                new WinchMountDefinition(
                    5,
                    WinchRole.Left,
                    new Vector3(0.50333f, 0f, -0.864094f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    5,
                    WinchRole.Right,
                    new Vector3(0.482513f, 0f, 0.875889f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    6,
                    WinchRole.Left,
                    new Vector3(0.50333f, 0f, -0.864094f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    6,
                    WinchRole.Right,
                    new Vector3(0.482513f, 0f, 0.875889f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    7,
                    WinchRole.Left,
                    new Vector3(0.414732f, 0f, -0.909944f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    7,
                    WinchRole.Right,
                    new Vector3(0.68046f, 0f, 0.732785f),
                    false,
                    -1
                ),
                new WinchMountDefinition(9, WinchRole.Reef, new Vector3(0f, 1f, 0f), true, 9),
                new WinchMountDefinition(
                    10,
                    WinchRole.Reef,
                    new Vector3(-0.942643f, 0f, 0.333801f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    11,
                    WinchRole.Reef,
                    new Vector3(-0.942643f, 0f, 0.333801f),
                    false,
                    -1
                ),
                new WinchMountDefinition(12, WinchRole.Reef, new Vector3(0f, 1f, 0f), true, 12),
                new WinchMountDefinition(13, WinchRole.Reef, new Vector3(0f, 1f, 0f), true, 13),
                new WinchMountDefinition(
                    16,
                    WinchRole.Left,
                    new Vector3(-0.087094f, 0f, -0.9962f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    16,
                    WinchRole.Right,
                    new Vector3(0.149905f, 0f, 0.9887f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    52,
                    WinchRole.Left,
                    new Vector3(0.414732f, 0f, -0.909944f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    52,
                    WinchRole.Right,
                    new Vector3(0.680461f, 0f, 0.732785f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    53,
                    WinchRole.Reef,
                    new Vector3(-0.000349f, 1f, 0f),
                    true,
                    53
                ),
                new WinchMountDefinition(57, WinchRole.Reef, new Vector3(0f, 1f, 0f), true, 57),
                new WinchMountDefinition(
                    58,
                    WinchRole.Reef,
                    new Vector3(0f, 0.965926f, 0.258819f),
                    true,
                    58
                ),
                new WinchMountDefinition(
                    61,
                    WinchRole.Left,
                    new Vector3(-0.087092f, 0f, -0.9962f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    61,
                    WinchRole.Right,
                    new Vector3(0.149906f, 0f, 0.9887f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    65,
                    WinchRole.Left,
                    new Vector3(0.50333f, 0f, -0.864094f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    65,
                    WinchRole.Right,
                    new Vector3(0.482512f, 0f, 0.87589f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    66,
                    WinchRole.Left,
                    new Vector3(0.50333f, 0f, -0.864094f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    66,
                    WinchRole.Right,
                    new Vector3(0.482512f, 0f, 0.87589f),
                    false,
                    -1
                ),
            };

        // Authored from installed Junk and Shipyard Expansion assets.
        // Endpoint vectors are local to the named physical mast section.
        private static FishermansStayGroupDefinition[] Stays() =>
            new[]
            {
                new FishermansStayGroupDefinition(
                    "Foremast / mainmast",
                    new[]
                    {
                        // 59.94 degrees at aft mast; forward masthead fallback.
                        new FishermansStayVariantDefinition(
                            128,
                            16,
                            "foremast / main mast 1",
                            9,
                            new Vector3(0.00000f, 0.00049f, 2.69000f),
                            10,
                            new Vector3(0.00248f, -0.00146f, 2.30206f),
                            false,
                            0,
                            new[] { 9, 10 },
                            new[] { 62 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            129,
                            16,
                            "fore topmast / main mast 1",
                            62,
                            new Vector3(-0.00000f, -0.00000f, -0.75865f),
                            10,
                            new Vector3(0.00248f, -0.00146f, 2.30206f),
                            false,
                            0,
                            new[] { 9, 10, 62 },
                            new int[0]
                        ),
                        // 57.86 degrees at aft mast; forward masthead fallback.
                        new FishermansStayVariantDefinition(
                            130,
                            61,
                            "raked foremast / main mast 2",
                            58,
                            new Vector3(0.00000f, 0.00049f, 2.69000f),
                            11,
                            new Vector3(0.00248f, -0.00146f, 2.30206f),
                            false,
                            0,
                            new[] { 11, 58 },
                            new int[0]
                        ),
                    }
                ),
                new FishermansStayGroupDefinition(
                    "Mainmast / mizzenmast",
                    new[]
                    {
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            131,
                            5,
                            "main mast 1 / mizzen mast 1",
                            10,
                            new Vector3(0.00248f, -0.00146f, -7.52300f),
                            12,
                            new Vector3(0.00000f, 0.00049f, 1.57183f),
                            false,
                            0,
                            new[] { 10, 12 },
                            new[] { 53 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            132,
                            5,
                            "main mast 1 / mizzen topmast",
                            10,
                            new Vector3(0.00248f, -0.00146f, -2.59085f),
                            53,
                            new Vector3(-0.00000f, -0.00000f, 0.84800f),
                            false,
                            0,
                            new[] { 10, 12, 53 },
                            new int[0]
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            133,
                            6,
                            "main mast 2 / mizzen mast 1",
                            11,
                            new Vector3(0.00248f, -0.00146f, -8.89858f),
                            12,
                            new Vector3(0.00000f, 0.00049f, 1.57183f),
                            false,
                            0,
                            new[] { 11, 12 },
                            new[] { 53 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            134,
                            6,
                            "main mast 2 / mizzen topmast",
                            11,
                            new Vector3(0.00248f, -0.00146f, -3.96643f),
                            53,
                            new Vector3(-0.00000f, -0.00000f, 0.84800f),
                            false,
                            0,
                            new[] { 11, 12, 53 },
                            new int[0]
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            135,
                            7,
                            "main mast 2 / mizzen mast 2",
                            11,
                            new Vector3(0.00248f, -0.00146f, -7.08233f),
                            13,
                            new Vector3(0.00000f, 0.00049f, 1.57200f),
                            false,
                            0,
                            new[] { 11, 13 },
                            new int[0]
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            136,
                            52,
                            "main mast 2 / mizzen mast 3",
                            11,
                            new Vector3(0.00248f, -0.00146f, -2.19715f),
                            57,
                            new Vector3(0.00248f, -0.00146f, 2.30206f),
                            false,
                            0,
                            new[] { 11, 57 },
                            new int[0]
                        ),
                    }
                ),
            };
    }
}
