using UnityEngine;

namespace FishermansSail.BoatRigs
{
    internal static partial class BoatRigCatalog
    {
        // Authored from installed Cog and Shipyard Expansion assets.
        // Endpoint vectors are local to the named physical mast section.
        private static FishermansStayGroupDefinition[] CogStays() =>
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
