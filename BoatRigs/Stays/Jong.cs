using UnityEngine;

namespace FishermansSail.BoatRigs
{
    internal static partial class BoatRigCatalog
    {
        // Authored from installed Jong and Shipyard Expansion assets.
        // Endpoint vectors are local to the named physical mast section.
        private static FishermansStayGroupDefinition[] JongStays() =>
            new[]
            {
                new FishermansStayGroupDefinition(
                    "Foremast / mainmast 1",
                    new[]
                    {
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            128,
                            10,
                            "foremast / main mast 1",
                            1,
                            new Vector3(0.00000f, -0.00000f, 1.13012f),
                            2,
                            new Vector3(-0.00000f, -0.00000f, 0.05000f),
                            false,
                            0,
                            new[] { 1, 2 },
                            new int[0]
                        ),
                    }
                ),
                new FishermansStayGroupDefinition(
                    "Mainmast 1 / mainmast 2",
                    new[]
                    {
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            129,
                            12,
                            "main mast 1 / main mast 2",
                            2,
                            new Vector3(0.00000f, -0.00000f, -2.22414f),
                            3,
                            new Vector3(-0.00000f, -0.00000f, 0.09200f),
                            false,
                            0,
                            new[] { 2, 3 },
                            new int[0]
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            130,
                            55,
                            "main mast 1 fore / main mast 2 fore",
                            51,
                            new Vector3(0.00000f, -0.00000f, -2.22315f),
                            52,
                            new Vector3(0.00000f, -0.00000f, 0.09200f),
                            false,
                            0,
                            new[] { 51, 52 },
                            new int[0]
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            131,
                            58,
                            "main mast 1 fore / main mast 2",
                            51,
                            new Vector3(0.00000f, -0.00000f, -3.69245f),
                            3,
                            new Vector3(-0.00000f, -0.00000f, 0.09200f),
                            false,
                            0,
                            new[] { 3, 51 },
                            new int[0]
                        ),
                    }
                ),
                new FishermansStayGroupDefinition(
                    "Foremast / mainmast 2",
                    new[]
                    {
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            132,
                            73,
                            "foremast / main mast 2 fore",
                            1,
                            new Vector3(-0.00000f, -0.00000f, 0.32528f),
                            52,
                            new Vector3(0.00000f, -0.00000f, 0.09200f),
                            false,
                            0,
                            new[] { 1, 52 },
                            new int[0]
                        ),
                    }
                ),
                new FishermansStayGroupDefinition(
                    "Mainmast 2 / mizzenmast",
                    new[]
                    {
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            133,
                            13,
                            "main mast 2 / mizzen mast",
                            3,
                            new Vector3(0.00000f, -0.00000f, -7.77488f),
                            4,
                            new Vector3(-0.00000f, -0.00000f, 0.15000f),
                            false,
                            0,
                            new[] { 3, 4 },
                            new int[0]
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            134,
                            56,
                            "main mast 2 fore / mizzen mast 2",
                            52,
                            new Vector3(0.00000f, -0.00000f, -7.77379f),
                            53,
                            new Vector3(0.00000f, -0.00000f, 0.15000f),
                            false,
                            0,
                            new[] { 52, 53 },
                            new int[0]
                        ),
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            135,
                            71,
                            "main mast 2 fore / mizzen mast",
                            52,
                            new Vector3(0.00000f, -0.00000f, -9.24418f),
                            4,
                            new Vector3(-0.00000f, -0.00000f, 0.15000f),
                            false,
                            0,
                            new[] { 4, 52 },
                            new int[0]
                        ),
                    }
                ),
                new FishermansStayGroupDefinition(
                    "Mainmast 1 / mizzenmast",
                    new[]
                    {
                        // 70.00 degrees at aft mast.
                        new FishermansStayVariantDefinition(
                            136,
                            75,
                            "main mast 1 / mizzen mast 2",
                            2,
                            new Vector3(0.00000f, -0.00000f, -8.62064f),
                            53,
                            new Vector3(0.00000f, -0.00000f, 0.15000f),
                            false,
                            0,
                            new[] { 2, 53 },
                            new int[0]
                        ),
                    }
                ),
            };
    }
}
