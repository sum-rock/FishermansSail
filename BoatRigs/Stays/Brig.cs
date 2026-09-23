using UnityEngine;

namespace FishermansSail.BoatRigs
{
    internal static partial class BoatRigCatalog
    {
        // Authored from installed Brig and Shipyard Expansion assets.
        // Endpoint vectors are local to the named physical mast section.
        private static FishermansStayGroupDefinition[] BrigStays() =>
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
