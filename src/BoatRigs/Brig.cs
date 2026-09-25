using System.Collections.Generic;
using UnityEngine;

namespace FishermansSail.BoatRigs
{
    internal static class Brig
    {
        internal static readonly BoatRigDefinition Definition = new BoatRigDefinition(
            "BOAT medi medium (50)",
            Supports(),
            Stays(),
            MastParents(),
            WinchMounts()
        );

        private static MastSupportDefinition[] Supports() =>
            new[]
            {
                new MastSupportDefinition(15, new[] { 3 }, new[] { 5 }),
                new MastSupportDefinition(16, new[] { 3 }, new[] { 4 }),
                new MastSupportDefinition(18, new[] { 2 }, new[] { 5 }),
                new MastSupportDefinition(20, new[] { 2 }, new[] { 4 }),
                new MastSupportDefinition(22, new[] { 4 }, new[] { 7 }),
                new MastSupportDefinition(24, new[] { 4 }, new[] { 6 }),
                new MastSupportDefinition(61, new[] { 3 }, new[] { 56, 5 }),
                new MastSupportDefinition(62, new[] { 3 }, new[] { 58, 4 }),
                new MastSupportDefinition(63, new[] { 2 }, new[] { 56, 5 }),
                new MastSupportDefinition(64, new[] { 2 }, new[] { 58, 4 }),
                new MastSupportDefinition(65, new[] { 4 }, new[] { 59, 7 }),
                new MastSupportDefinition(66, new[] { 4 }, new[] { 60, 6 }),
            };

        // Installed mast ancestry: -1 marks a physical base section.
        private static Dictionary<int, int> MastParents() =>
            new Dictionary<int, int>
            {
                { 3, -1 },
                { 2, -1 },
                { 67, -1 },
                { 79, -1 },
                { 5, -1 },
                { 4, -1 },
                { 7, -1 },
                { 6, -1 },
                { 53, -1 },
                { 54, -1 },
                { 55, 3 },
                { 57, 2 },
                { 56, 5 },
                { 58, 4 },
                { 59, 7 },
                { 60, 6 },
            };

        // Installed donor directions in boat space; provenance and numeric fixtures
        // are documented in docs/DEVELOPMENT.md under shared winch placement.
        private static WinchMountDefinition[] WinchMounts() =>
            new[]
            {
                new WinchMountDefinition(2, WinchRole.Reef, new Vector3(0f, 1f, 0f), true, 2),
                new WinchMountDefinition(3, WinchRole.Reef, new Vector3(0f, 1f, 0f), true, 3),
                new WinchMountDefinition(4, WinchRole.Reef, new Vector3(0f, 1f, 0f), true, 4),
                new WinchMountDefinition(5, WinchRole.Reef, new Vector3(0f, 1f, 0f), true, 5),
                new WinchMountDefinition(6, WinchRole.Reef, new Vector3(0f, 1f, 0f), true, 6),
                new WinchMountDefinition(7, WinchRole.Reef, new Vector3(0f, 1f, 0f), true, 7),
                new WinchMountDefinition(
                    15,
                    WinchRole.Left,
                    new Vector3(0.992723f, 0f, 0.120421f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    15,
                    WinchRole.Right,
                    new Vector3(0.992723f, 0f, 0.120421f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    16,
                    WinchRole.Left,
                    new Vector3(0.992723f, 0f, 0.120421f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    16,
                    WinchRole.Right,
                    new Vector3(0.992723f, 0f, 0.120421f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    18,
                    WinchRole.Left,
                    new Vector3(0.992723f, 0f, 0.120421f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    18,
                    WinchRole.Right,
                    new Vector3(0.992723f, 0f, 0.120421f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    20,
                    WinchRole.Left,
                    new Vector3(0.992723f, 0f, 0.120421f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    20,
                    WinchRole.Right,
                    new Vector3(0.992723f, 0f, 0.120421f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    22,
                    WinchRole.Left,
                    new Vector3(0.943135f, 0f, 0.332409f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    22,
                    WinchRole.Right,
                    new Vector3(0.992723f, 0f, 0.120421f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    24,
                    WinchRole.Left,
                    new Vector3(0.943135f, 0f, 0.332409f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    24,
                    WinchRole.Right,
                    new Vector3(0.992723f, 0f, 0.120421f),
                    false,
                    -1
                ),
                new WinchMountDefinition(56, WinchRole.Reef, new Vector3(0f, 1f, 0f), true, 5),
                new WinchMountDefinition(58, WinchRole.Reef, new Vector3(0f, 1f, 0f), true, 58),
                new WinchMountDefinition(59, WinchRole.Reef, new Vector3(0f, 1f, 0f), true, 59),
                new WinchMountDefinition(60, WinchRole.Reef, new Vector3(0f, 1f, 0f), true, 60),
                new WinchMountDefinition(
                    61,
                    WinchRole.Left,
                    new Vector3(0.549092f, 0f, -0.835762f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    61,
                    WinchRole.Right,
                    new Vector3(0.992722f, 0f, 0.120428f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    62,
                    WinchRole.Left,
                    new Vector3(0.549092f, 0f, -0.835762f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    62,
                    WinchRole.Right,
                    new Vector3(0.992722f, 0f, 0.120428f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    63,
                    WinchRole.Left,
                    new Vector3(0.549092f, 0f, -0.835762f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    63,
                    WinchRole.Right,
                    new Vector3(0.992722f, 0f, 0.120428f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    64,
                    WinchRole.Left,
                    new Vector3(0.549092f, 0f, -0.835762f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    64,
                    WinchRole.Right,
                    new Vector3(0.992722f, 0f, 0.120428f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    65,
                    WinchRole.Left,
                    new Vector3(0.943135f, 0f, 0.332409f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    65,
                    WinchRole.Right,
                    new Vector3(0.992722f, 0f, 0.120426f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    66,
                    WinchRole.Left,
                    new Vector3(0.943135f, 0f, 0.332409f),
                    false,
                    -1
                ),
                new WinchMountDefinition(
                    66,
                    WinchRole.Right,
                    new Vector3(0.992722f, 0f, 0.120426f),
                    false,
                    -1
                ),
            };

        // Authored from installed Brig and Shipyard Expansion assets.
        // Endpoint vectors are local to the named physical mast section.
        private static FishermansStayGroupDefinition[] Stays() =>
            new[]
            {
                new FishermansStayGroupDefinition(
                    "Foremast / mainmast",
                    new[]
                    {
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            128,
                            15,
                            "foremast 1 / main mast 1",
                            3,
                            new Vector3(0.00000f, -0.00000f, -2.39540f),
                            5,
                            new Vector3(0.00000f, -0.00000f, 0.93843f),
                            false,
                            0,
                            new[] { 3, 5 },
                            new[] { 55, 56 }
                        ),
                        // 61.15 degrees at aft mast; forward masthead fallback.
                        new FishermansStayVariantDefinition(
                            129,
                            15,
                            "foremast 1 / main topmast 1",
                            3,
                            new Vector3(0.00000f, -0.00000f, 3.08320f),
                            56,
                            new Vector3(0.00000f, -0.00000f, 0.75600f),
                            false,
                            0,
                            new[] { 3, 5, 56 },
                            new[] { 55 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            130,
                            15,
                            "fore topmast 1 / main mast 1",
                            3,
                            new Vector3(0.00000f, -0.00000f, -2.39540f),
                            5,
                            new Vector3(0.00000f, -0.00000f, 0.93843f),
                            false,
                            0,
                            new[] { 3, 5, 55 },
                            new[] { 56 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            131,
                            15,
                            "fore topmast 1 / main topmast 1",
                            55,
                            new Vector3(0.00000f, -0.00000f, -2.44076f),
                            56,
                            new Vector3(0.00000f, -0.00000f, 0.75600f),
                            false,
                            0,
                            new[] { 3, 5, 55, 56 },
                            new int[0]
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            132,
                            16,
                            "foremast 1 / main mast 2",
                            3,
                            new Vector3(0.00000f, -0.00000f, -0.64295f),
                            4,
                            new Vector3(0.00000f, -0.00000f, 0.93843f),
                            false,
                            0,
                            new[] { 3, 4 },
                            new[] { 55, 58 }
                        ),
                        // 46.74 degrees at aft mast; forward masthead fallback.
                        new FishermansStayVariantDefinition(
                            133,
                            16,
                            "foremast 1 / main topmast 2",
                            3,
                            new Vector3(0.00000f, -0.00000f, 3.08320f),
                            58,
                            new Vector3(0.00000f, -0.00000f, 0.75600f),
                            false,
                            0,
                            new[] { 3, 4, 58 },
                            new[] { 55 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            134,
                            16,
                            "fore topmast 1 / main mast 2",
                            3,
                            new Vector3(0.00000f, -0.00000f, -0.64295f),
                            4,
                            new Vector3(0.00000f, -0.00000f, 0.93843f),
                            false,
                            0,
                            new[] { 3, 4, 55 },
                            new[] { 58 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            135,
                            16,
                            "fore topmast 1 / main topmast 2",
                            55,
                            new Vector3(0.00000f, -0.00000f, -0.72370f),
                            58,
                            new Vector3(0.00000f, -0.00000f, 0.75600f),
                            false,
                            0,
                            new[] { 3, 4, 55, 58 },
                            new int[0]
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            136,
                            18,
                            "foremast 2 / main mast 1",
                            2,
                            new Vector3(0.00000f, -0.00000f, -2.75301f),
                            5,
                            new Vector3(0.00000f, -0.00000f, 0.93843f),
                            false,
                            0,
                            new[] { 2, 5 },
                            new[] { 56, 57 }
                        ),
                        // 64.44 degrees at aft mast; forward masthead fallback.
                        new FishermansStayVariantDefinition(
                            137,
                            18,
                            "foremast 2 / main topmast 1",
                            2,
                            new Vector3(0.00000f, -0.00000f, 3.08320f),
                            56,
                            new Vector3(0.00000f, -0.00000f, 0.75600f),
                            false,
                            0,
                            new[] { 2, 5, 56 },
                            new[] { 57 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            138,
                            18,
                            "fore topmast 2 / main mast 1",
                            2,
                            new Vector3(0.00000f, -0.00000f, -2.75301f),
                            5,
                            new Vector3(0.00000f, -0.00000f, 0.93843f),
                            false,
                            0,
                            new[] { 2, 5, 57 },
                            new[] { 56 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            139,
                            18,
                            "fore topmast 2 / main topmast 1",
                            57,
                            new Vector3(-0.01325f, 0.00000f, -2.71105f),
                            56,
                            new Vector3(0.00000f, -0.00000f, 0.75600f),
                            false,
                            0,
                            new[] { 2, 5, 56, 57 },
                            new int[0]
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            140,
                            20,
                            "foremast 2 / main mast 2",
                            2,
                            new Vector3(0.00000f, -0.00000f, -1.00056f),
                            4,
                            new Vector3(0.00000f, -0.00000f, 0.93843f),
                            false,
                            0,
                            new[] { 2, 4 },
                            new[] { 57, 58 }
                        ),
                        // 55.52 degrees at aft mast; forward masthead fallback.
                        new FishermansStayVariantDefinition(
                            141,
                            20,
                            "foremast 2 / main topmast 2",
                            2,
                            new Vector3(0.00000f, -0.00000f, 3.08320f),
                            58,
                            new Vector3(0.00000f, -0.00000f, 0.75600f),
                            false,
                            0,
                            new[] { 2, 4, 58 },
                            new[] { 57 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            142,
                            20,
                            "fore topmast 2 / main mast 2",
                            2,
                            new Vector3(0.00000f, -0.00000f, -1.00056f),
                            4,
                            new Vector3(0.00000f, -0.00000f, 0.93843f),
                            false,
                            0,
                            new[] { 2, 4, 57 },
                            new[] { 58 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            143,
                            20,
                            "fore topmast 2 / main topmast 2",
                            57,
                            new Vector3(-0.01325f, 0.00000f, -0.99399f),
                            58,
                            new Vector3(0.00000f, -0.00000f, 0.75600f),
                            false,
                            0,
                            new[] { 2, 4, 57, 58 },
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
                            144,
                            22,
                            "main mast 2 / mizzen mast 1",
                            4,
                            new Vector3(0.00000f, -0.00000f, -6.01788f),
                            7,
                            new Vector3(0.00000f, 0.00391f, 1.37491f),
                            false,
                            0,
                            new[] { 4, 7 },
                            new[] { 58, 59 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            145,
                            22,
                            "main mast 2 / mizzen topmast 1",
                            4,
                            new Vector3(0.00000f, -0.00000f, 0.16585f),
                            59,
                            new Vector3(0.00000f, -0.00000f, 0.75600f),
                            false,
                            0,
                            new[] { 4, 7, 59 },
                            new[] { 58 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            146,
                            22,
                            "main topmast 2 / mizzen mast 1",
                            4,
                            new Vector3(0.00000f, -0.00000f, -6.01788f),
                            7,
                            new Vector3(0.00000f, 0.00391f, 1.37491f),
                            false,
                            0,
                            new[] { 4, 7, 58 },
                            new[] { 59 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            147,
                            22,
                            "main topmast 2 / mizzen topmast 1",
                            4,
                            new Vector3(0.00000f, -0.00000f, 0.16585f),
                            59,
                            new Vector3(0.00000f, -0.00000f, 0.75600f),
                            false,
                            0,
                            new[] { 4, 7, 58, 59 },
                            new int[0]
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            148,
                            24,
                            "main mast 2 / mizzen mast 2",
                            4,
                            new Vector3(0.00000f, -0.00000f, -5.12171f),
                            6,
                            new Vector3(0.00000f, 0.00391f, 1.37491f),
                            false,
                            0,
                            new[] { 4, 6 },
                            new[] { 58, 60 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            149,
                            24,
                            "main mast 2 / mizzen topmast 2",
                            4,
                            new Vector3(0.00000f, -0.00000f, 1.05939f),
                            60,
                            new Vector3(0.00000f, -0.00000f, 0.75600f),
                            false,
                            0,
                            new[] { 4, 6, 60 },
                            new[] { 58 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            150,
                            24,
                            "main topmast 2 / mizzen mast 2",
                            4,
                            new Vector3(0.00000f, -0.00000f, -5.12171f),
                            6,
                            new Vector3(0.00000f, 0.00391f, 1.37491f),
                            false,
                            0,
                            new[] { 4, 6, 58 },
                            new[] { 60 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            151,
                            24,
                            "main topmast 2 / mizzen topmast 2",
                            58,
                            new Vector3(0.00000f, -0.00000f, -6.71642f),
                            60,
                            new Vector3(0.00000f, -0.00000f, 0.75600f),
                            false,
                            0,
                            new[] { 4, 6, 58, 60 },
                            new int[0]
                        ),
                    }
                ),
            };
    }
}
