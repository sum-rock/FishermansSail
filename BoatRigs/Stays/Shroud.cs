using UnityEngine;

namespace FishermansSail.BoatRigs
{
    internal static partial class BoatRigCatalog
    {
        // Authored from installed Shroud and Shipyard Expansion assets.
        // Endpoint vectors are local to the named physical mast section.
        private static FishermansStayGroupDefinition[] ShroudStays() =>
            new[]
            {
                new FishermansStayGroupDefinition(
                    "Foremast / mainmast",
                    new[]
                    {
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            128,
                            24,
                            "Foremast 1 / Mainmast 1",
                            6,
                            new Vector3(0.00000f, -0.00682f, -1.26976f),
                            8,
                            new Vector3(0.00000f, -0.00928f, -0.42400f),
                            false,
                            0,
                            new[] { 6, 8 },
                            new int[0]
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            129,
                            24,
                            "Foremast 1 Tall / Mainmast 1",
                            5,
                            new Vector3(-0.00291f, 0.00006f, -11.85458f),
                            8,
                            new Vector3(0.00000f, -0.00928f, -0.42400f),
                            false,
                            0,
                            new[] { 5, 8 },
                            new int[0]
                        ),
                        // 42.20 degrees at aft mast; forward masthead fallback.
                        new FishermansStayVariantDefinition(
                            130,
                            24,
                            "Foremast 1 / Mainmast 1 Tall",
                            6,
                            new Vector3(0.00000f, -0.00682f, 0.50000f),
                            7,
                            new Vector3(-0.00537f, -0.00391f, -0.46400f),
                            false,
                            0,
                            new[] { 6, 7 },
                            new int[0]
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            131,
                            24,
                            "Foremast 1 Tall / Mainmast 1 Tall",
                            5,
                            new Vector3(-0.00291f, 0.00006f, -2.44452f),
                            7,
                            new Vector3(-0.00537f, -0.00391f, -0.46400f),
                            false,
                            0,
                            new[] { 5, 7 },
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
                            132,
                            25,
                            "Mainmast 1 / Mizzen 1",
                            8,
                            new Vector3(0.00000f, -0.00928f, -6.67603f),
                            10,
                            new Vector3(0.00000f, -0.00365f, -0.51800f),
                            false,
                            0,
                            new[] { 8, 10 },
                            new int[0]
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            133,
                            25,
                            "Mainmast 1 Tall / Mizzen 1",
                            7,
                            new Vector3(-0.00537f, -0.00391f, -16.12609f),
                            10,
                            new Vector3(0.00000f, -0.00365f, -0.51800f),
                            false,
                            0,
                            new[] { 7, 10 },
                            new int[0]
                        ),
                        // 55.88 degrees at aft mast; forward masthead fallback.
                        new FishermansStayVariantDefinition(
                            134,
                            25,
                            "Mainmast 1 / Mizzenmast 1 Tall",
                            8,
                            new Vector3(0.00000f, -0.00928f, 0.50000f),
                            9,
                            new Vector3(0.00025f, -0.00391f, -0.12789f),
                            false,
                            0,
                            new[] { 8, 9 },
                            new int[0]
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            135,
                            25,
                            "Mainmast 1 Tall / Mizzenmast 1 Tall",
                            7,
                            new Vector3(-0.00537f, -0.00391f, -5.67900f),
                            9,
                            new Vector3(0.00025f, -0.00391f, -0.12789f),
                            false,
                            0,
                            new[] { 7, 9 },
                            new int[0]
                        ),
                    }
                ),
            };
    }
}
