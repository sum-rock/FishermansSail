using UnityEngine;

namespace FishermansSail.BoatRigs
{
    internal static partial class BoatRigCatalog
    {
        // Authored from installed Leopard and Shipyard Expansion assets.
        // Endpoint vectors are local to the named physical mast section.
        private static FishermansStayGroupDefinition[] LeopardStays() =>
            new[]
            {
                new FishermansStayGroupDefinition(
                    "Foremast / mainmast",
                    new[]
                    {
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            128,
                            13,
                            "Foremast / Mainmast",
                            4,
                            new Vector3(-0.00000f, -0.00000f, -9.96491f),
                            7,
                            new Vector3(-0.00000f, -0.00000f, -4.26600f),
                            false,
                            0,
                            new[] { 4, 7 },
                            new[] { 5, 6, 8, 9 }
                        ),
                        // 56.21 degrees at aft mast; forward masthead fallback.
                        new FishermansStayVariantDefinition(
                            129,
                            13,
                            "Foremast / Mainmast Mid",
                            4,
                            new Vector3(0.00000f, 0.00000f, 0.00000f),
                            8,
                            new Vector3(-0.00000f, 0.00000f, -2.12849f),
                            false,
                            0,
                            new[] { 4, 7, 8 },
                            new[] { 5, 6, 9 }
                        ),
                        // 35.14 degrees at aft mast; forward masthead fallback.
                        new FishermansStayVariantDefinition(
                            130,
                            13,
                            "Foremast / Mainmast Top",
                            4,
                            new Vector3(0.00000f, 0.00000f, 0.00000f),
                            9,
                            new Vector3(-0.00000f, -0.00000f, -1.27499f),
                            false,
                            0,
                            new[] { 4, 7, 8, 9 },
                            new[] { 5, 6 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            131,
                            13,
                            "Foremast Mid / Mainmast",
                            4,
                            new Vector3(-0.00000f, -0.00000f, -9.96491f),
                            7,
                            new Vector3(-0.00000f, -0.00000f, -4.26600f),
                            false,
                            0,
                            new[] { 4, 5, 7 },
                            new[] { 6, 8, 9 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            132,
                            13,
                            "Foremast Mid / Mainmast Mid",
                            5,
                            new Vector3(-0.00000f, -0.00000f, -5.07507f),
                            8,
                            new Vector3(-0.00000f, 0.00000f, -2.12849f),
                            false,
                            0,
                            new[] { 4, 5, 7, 8 },
                            new[] { 6, 9 }
                        ),
                        // 50.95 degrees at aft mast; forward masthead fallback.
                        new FishermansStayVariantDefinition(
                            133,
                            13,
                            "Foremast Mid / Mainmast Top",
                            5,
                            new Vector3(-0.00000f, -0.00000f, -0.01445f),
                            9,
                            new Vector3(-0.00000f, -0.00000f, -1.27499f),
                            false,
                            0,
                            new[] { 4, 5, 7, 8, 9 },
                            new[] { 6 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            134,
                            13,
                            "Foremast Top / Mainmast",
                            4,
                            new Vector3(-0.00000f, -0.00000f, -9.96491f),
                            7,
                            new Vector3(-0.00000f, -0.00000f, -4.26600f),
                            false,
                            0,
                            new[] { 4, 5, 6, 7 },
                            new[] { 8, 9 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            135,
                            13,
                            "Foremast Top / Mainmast Mid",
                            5,
                            new Vector3(-0.00000f, -0.00000f, -5.07507f),
                            8,
                            new Vector3(-0.00000f, 0.00000f, -2.12849f),
                            false,
                            0,
                            new[] { 4, 5, 6, 7, 8 },
                            new[] { 9 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            136,
                            13,
                            "Foremast Top / Mainmast Top",
                            6,
                            new Vector3(-0.00000f, -0.00000f, -3.00593f),
                            9,
                            new Vector3(-0.00000f, -0.00000f, -1.27499f),
                            false,
                            0,
                            new[] { 4, 5, 6, 7, 8, 9 },
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
                            137,
                            18,
                            "Mainmast / Mizzenmast",
                            7,
                            new Vector3(-0.00000f, 0.00000f, -11.63731f),
                            10,
                            new Vector3(-0.00000f, 0.00000f, -3.05500f),
                            false,
                            0,
                            new[] { 7, 10 },
                            new[] { 8, 9, 11, 12 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            138,
                            18,
                            "Mainmast / Mizzenmast Mid",
                            7,
                            new Vector3(0.00000f, 0.00000f, -0.22779f),
                            11,
                            new Vector3(-0.00000f, -0.00000f, -1.47699f),
                            false,
                            0,
                            new[] { 7, 10, 11 },
                            new[] { 8, 9, 12 }
                        ),
                        // 39.95 degrees at aft mast; forward masthead fallback.
                        new FishermansStayVariantDefinition(
                            139,
                            18,
                            "Mainmast / Mizzenmast Top",
                            7,
                            new Vector3(0.00000f, 0.00000f, 0.00000f),
                            12,
                            new Vector3(-0.00000f, -0.00000f, -0.91439f),
                            false,
                            0,
                            new[] { 7, 10, 11, 12 },
                            new[] { 8, 9 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            140,
                            18,
                            "Mainmast Mid / Mizzenmast",
                            7,
                            new Vector3(-0.00000f, 0.00000f, -11.63731f),
                            10,
                            new Vector3(-0.00000f, 0.00000f, -3.05500f),
                            false,
                            0,
                            new[] { 7, 8, 10 },
                            new[] { 9, 11, 12 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            141,
                            18,
                            "Mainmast Mid / Mizzenmast Mid",
                            8,
                            new Vector3(0.00000f, 0.00000f, -13.78993f),
                            11,
                            new Vector3(-0.00000f, -0.00000f, -1.47699f),
                            false,
                            0,
                            new[] { 7, 8, 10, 11 },
                            new[] { 9, 12 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            142,
                            18,
                            "Mainmast Mid / Mizzenmast Top",
                            8,
                            new Vector3(-0.00000f, 0.00000f, -3.88556f),
                            12,
                            new Vector3(-0.00000f, -0.00000f, -0.91439f),
                            false,
                            0,
                            new[] { 7, 8, 10, 11, 12 },
                            new[] { 9 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            143,
                            18,
                            "Mainmast Top / Mizzenmast",
                            7,
                            new Vector3(-0.00000f, 0.00000f, -11.63731f),
                            10,
                            new Vector3(-0.00000f, 0.00000f, -3.05500f),
                            false,
                            0,
                            new[] { 7, 8, 9, 10 },
                            new[] { 11, 12 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            144,
                            18,
                            "Mainmast Top / Mizzenmast Mid",
                            8,
                            new Vector3(0.00000f, 0.00000f, -13.78993f),
                            11,
                            new Vector3(-0.00000f, -0.00000f, -1.47699f),
                            false,
                            0,
                            new[] { 7, 8, 9, 10, 11 },
                            new[] { 12 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            145,
                            18,
                            "Mainmast Top / Mizzenmast Top",
                            8,
                            new Vector3(-0.00000f, 0.00000f, -3.88556f),
                            12,
                            new Vector3(-0.00000f, -0.00000f, -0.91439f),
                            false,
                            0,
                            new[] { 7, 8, 9, 10, 11, 12 },
                            new int[0]
                        ),
                    }
                ),
            };
    }
}
