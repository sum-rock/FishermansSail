namespace FishermansSail
{
    internal static partial class BoatRigCatalog
    {
        private static readonly BoatRigDefinition Sanbuq = new BoatRigDefinition(
            "BOAT dhow medium (20)",
            new StayGroupDefinition(
                19,
                new StayVariantDefinition(60, 10, 59, true, 59, 59, new[] { 10 }, new[] { 59, 55 }),
                new StayVariantDefinition(67, 11, 59, true, 59, 59, new[] { 11 }, new[] { 59, 55 }),
                new StayVariantDefinition(71, 10, 70, true, 70, 70, new[] { 10 }, new[] { 70, 69 }),
                new StayVariantDefinition(81, 11, 80, true, 80, 80, new[] { 11 }, new[] { 80, 12 }),
                new StayVariantDefinition(82, 10, 80, true, 80, 80, new[] { 10 }, new[] { 80, 12 })
            ),
            new StayGroupDefinition(
                21,
                new StayVariantDefinition(
                    58,
                    51,
                    14,
                    false,
                    51,
                    51,
                    new[] { 51 },
                    new[] { 14, 11 }
                ),
                new StayVariantDefinition(
                    68,
                    62,
                    14,
                    false,
                    62,
                    62,
                    new[] { 62 },
                    new[] { 14, 11 }
                ),
                new StayVariantDefinition(79, 51, 14, false, 51, 51, new[] { 51 }, new[] { 14, 11 })
            ),
            new StayGroupDefinition(
                8,
                new StayVariantDefinition(54, 10, 55, true, 55, 55, new[] { 10 }, new[] { 55 }),
                new StayVariantDefinition(61, 10, 69, true, 69, 69, new[] { 10 }, new[] { 69 }),
                new StayVariantDefinition(66, 11, 55, true, 55, 55, new[] { 11 }, new[] { 55 })
            )
        );
    }
}
