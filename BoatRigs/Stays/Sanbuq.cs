using UnityEngine;

namespace FishermansSail.BoatRigs
{
    internal static partial class BoatRigCatalog
    {
        // Authored from installed Sanbuq and Shipyard Expansion assets.
        // Endpoint vectors are local to the named physical mast section.
        private static FishermansStayGroupDefinition[] SanbuqStays() =>
            new[]
            {
                new FishermansStayGroupDefinition(
                    "Mainmast / mizzenmast",
                    new[]
                    {
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            128,
                            9,
                            "mast 1 / mizzen mast",
                            10,
                            new Vector3(0.00116f, 0.00513f, -4.94033f),
                            12,
                            new Vector3(0.00116f, 0.00513f, 1.48931f),
                            false,
                            0,
                            new[] { 10, 12 },
                            new[] { 13, 80 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            129,
                            9,
                            "mast 1 / mizzen topmast 3",
                            10,
                            new Vector3(0.00116f, 0.00513f, 0.97319f),
                            80,
                            new Vector3(-0.00000f, -0.00000f, 0.38279f),
                            false,
                            0,
                            new[] { 10, 12, 80 },
                            new[] { 13 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            130,
                            9,
                            "topmast 1 / mizzen mast",
                            10,
                            new Vector3(0.00116f, 0.00513f, -4.94033f),
                            12,
                            new Vector3(0.00116f, 0.00513f, 1.48931f),
                            false,
                            0,
                            new[] { 10, 12, 13 },
                            new[] { 80 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            131,
                            9,
                            "topmast 1 / mizzen topmast 3",
                            13,
                            new Vector3(-0.00000f, 0.00000f, -8.42753f),
                            80,
                            new Vector3(-0.00000f, -0.00000f, 0.38279f),
                            false,
                            0,
                            new[] { 10, 12, 13, 80 },
                            new int[0]
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            132,
                            54,
                            "mast 1 / mizzen mast 2",
                            10,
                            new Vector3(0.00116f, 0.00513f, -2.46726f),
                            55,
                            new Vector3(0.00116f, 0.00513f, 2.18743f),
                            false,
                            2,
                            new[] { 10, 55 },
                            new[] { 13, 59 }
                        ),
                        // 63.23 degrees at aft mast; forward masthead fallback.
                        new FishermansStayVariantDefinition(
                            133,
                            54,
                            "mast 1 / mizzen topmast 1",
                            10,
                            new Vector3(0.00116f, 0.00513f, 3.43000f),
                            59,
                            new Vector3(0.00000f, 0.00000f, 0.37763f),
                            false,
                            0,
                            new[] { 10, 55, 59 },
                            new[] { 13 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            134,
                            54,
                            "topmast 1 / mizzen mast 2",
                            10,
                            new Vector3(0.00116f, 0.00513f, -2.46726f),
                            55,
                            new Vector3(0.00116f, 0.00513f, 2.18743f),
                            false,
                            2,
                            new[] { 10, 13, 55 },
                            new[] { 59 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            135,
                            54,
                            "topmast 1 / mizzen topmast 1",
                            13,
                            new Vector3(0.00000f, 0.00000f, -4.27195f),
                            59,
                            new Vector3(0.00000f, 0.00000f, 0.37763f),
                            false,
                            0,
                            new[] { 10, 13, 55, 59 },
                            new int[0]
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            136,
                            66,
                            "mast 2 / mizzen mast 2",
                            11,
                            new Vector3(0.00116f, 0.00513f, -0.70023f),
                            55,
                            new Vector3(0.00116f, 0.00513f, 2.18743f),
                            false,
                            2,
                            new[] { 11, 55 },
                            new[] { 14, 59 }
                        ),
                        // 50.32 degrees at aft mast; forward masthead fallback.
                        new FishermansStayVariantDefinition(
                            137,
                            66,
                            "mast 2 / mizzen topmast 1",
                            11,
                            new Vector3(0.00116f, 0.00513f, 3.43000f),
                            59,
                            new Vector3(0.00000f, 0.00000f, 0.37763f),
                            false,
                            0,
                            new[] { 11, 55, 59 },
                            new[] { 14 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            138,
                            66,
                            "topmast 2 / mizzen mast 2",
                            11,
                            new Vector3(0.00116f, 0.00513f, -0.70023f),
                            55,
                            new Vector3(0.00116f, 0.00513f, 2.18743f),
                            false,
                            2,
                            new[] { 11, 14, 55 },
                            new[] { 59 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            139,
                            66,
                            "topmast 2 / mizzen topmast 1",
                            14,
                            new Vector3(0.00000f, 0.00000f, -2.50331f),
                            59,
                            new Vector3(0.00000f, 0.00000f, 0.37763f),
                            false,
                            0,
                            new[] { 11, 14, 55, 59 },
                            new int[0]
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            140,
                            61,
                            "mast 1 / mizzen mast 3",
                            10,
                            new Vector3(0.00116f, 0.00513f, -1.37176f),
                            69,
                            new Vector3(0.00116f, 0.00513f, 2.18743f),
                            false,
                            2,
                            new[] { 10, 69 },
                            new[] { 13, 70 }
                        ),
                        // 56.31 degrees at aft mast; forward masthead fallback.
                        new FishermansStayVariantDefinition(
                            141,
                            61,
                            "mast 1 / mizzen topmast 2",
                            10,
                            new Vector3(0.00116f, 0.00513f, 3.43000f),
                            70,
                            new Vector3(-0.00000f, -0.00000f, 0.38279f),
                            false,
                            0,
                            new[] { 10, 69, 70 },
                            new[] { 13 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            142,
                            61,
                            "topmast 1 / mizzen mast 3",
                            10,
                            new Vector3(0.00116f, 0.00513f, -1.37176f),
                            69,
                            new Vector3(0.00116f, 0.00513f, 2.18743f),
                            false,
                            2,
                            new[] { 10, 13, 69 },
                            new[] { 70 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            143,
                            61,
                            "topmast 1 / mizzen topmast 2",
                            13,
                            new Vector3(-0.00000f, -0.00000f, -3.17129f),
                            70,
                            new Vector3(-0.00000f, -0.00000f, 0.38279f),
                            false,
                            0,
                            new[] { 10, 13, 69, 70 },
                            new int[0]
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            144,
                            81,
                            "mast 2 / mizzen mast",
                            11,
                            new Vector3(0.00116f, 0.00513f, -3.67355f),
                            12,
                            new Vector3(0.00116f, 0.00513f, 1.48931f),
                            false,
                            0,
                            new[] { 11, 12 },
                            new[] { 14, 80 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            145,
                            81,
                            "mast 2 / mizzen topmast 3",
                            11,
                            new Vector3(0.00116f, 0.00513f, 2.23960f),
                            80,
                            new Vector3(-0.00000f, -0.00000f, 0.38279f),
                            false,
                            0,
                            new[] { 11, 12, 80 },
                            new[] { 14 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            146,
                            81,
                            "topmast 2 / mizzen mast",
                            11,
                            new Vector3(0.00116f, 0.00513f, -3.67355f),
                            12,
                            new Vector3(0.00116f, 0.00513f, 1.48931f),
                            false,
                            0,
                            new[] { 11, 12, 14 },
                            new[] { 80 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            147,
                            81,
                            "topmast 2 / mizzen topmast 3",
                            14,
                            new Vector3(-0.00000f, -0.00000f, -7.15996f),
                            80,
                            new Vector3(-0.00000f, -0.00000f, 0.38279f),
                            false,
                            0,
                            new[] { 11, 12, 14, 80 },
                            new int[0]
                        ),
                    }
                ),
                new FishermansStayGroupDefinition(
                    "Foremast / mainmast",
                    new[]
                    {
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            148,
                            57,
                            "foremast 1 / mast 2",
                            51,
                            new Vector3(0.00116f, 0.00513f, -0.79138f),
                            11,
                            new Vector3(0.00116f, 0.00513f, 2.18743f),
                            false,
                            2,
                            new[] { 11, 51 },
                            new[] { 14, 64 }
                        ),
                        // 51.31 degrees at aft mast; forward masthead fallback.
                        new FishermansStayVariantDefinition(
                            149,
                            57,
                            "foremast 1 / topmast 2",
                            51,
                            new Vector3(0.00116f, 0.00513f, 3.43000f),
                            14,
                            new Vector3(0.00000f, -0.00000f, 0.38279f),
                            false,
                            0,
                            new[] { 11, 14, 51 },
                            new[] { 64 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            150,
                            57,
                            "fore topmast / mast 2",
                            51,
                            new Vector3(0.00116f, 0.00513f, -0.79138f),
                            11,
                            new Vector3(0.00116f, 0.00513f, 2.18743f),
                            false,
                            2,
                            new[] { 11, 51, 64 },
                            new[] { 14 }
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            151,
                            57,
                            "fore topmast / topmast 2",
                            64,
                            new Vector3(-0.00000f, -0.00000f, -2.59788f),
                            14,
                            new Vector3(0.00000f, -0.00000f, 0.38279f),
                            false,
                            0,
                            new[] { 11, 14, 51, 64 },
                            new int[0]
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            152,
                            68,
                            "raked foremast / mast 2",
                            62,
                            new Vector3(0.00116f, 0.00513f, -1.39570f),
                            11,
                            new Vector3(0.00116f, 0.00513f, 2.18743f),
                            false,
                            2,
                            new[] { 11, 62 },
                            new[] { 14 }
                        ),
                        // 61.21 degrees at aft mast; forward masthead fallback.
                        new FishermansStayVariantDefinition(
                            153,
                            68,
                            "raked foremast / topmast 2",
                            62,
                            new Vector3(0.00116f, 0.00513f, 3.43000f),
                            14,
                            new Vector3(0.00000f, -0.00000f, 0.38279f),
                            false,
                            0,
                            new[] { 11, 14, 62 },
                            new int[0]
                        ),
                    }
                ),
            };
    }
}
