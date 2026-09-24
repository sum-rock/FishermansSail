using UnityEngine;

namespace FishermansSail.BoatRigs
{
    internal static partial class BoatRigCatalog
    {
        // Authored from installed Junk and Shipyard Expansion assets.
        // Endpoint vectors are local to the named physical mast section.
        private static FishermansStayGroupDefinition[] JunkStays() =>
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
