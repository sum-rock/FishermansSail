namespace FishermansSail
{
    internal static partial class BoatRigCatalog
    {
        private static readonly BoatRigDefinition Brig = new BoatRigDefinition(
            "BOAT medi medium (50)",
            new StayGroupDefinition(
                8,
                new StayVariantDefinition(15, 3, 5, false, 3, 3, new[] { 3 }, new[] { 5 }),
                new StayVariantDefinition(16, 3, 4, false, 3, 3, new[] { 3 }, new[] { 4 }),
                new StayVariantDefinition(18, 2, 5, false, 2, 2, new[] { 2 }, new[] { 5 }),
                new StayVariantDefinition(20, 2, 4, false, 2, 2, new[] { 2 }, new[] { 4 })
            ),
            new StayGroupDefinition(
                10,
                new StayVariantDefinition(22, 4, 7, true, 7, 7, new[] { 4 }, new[] { 7 }),
                new StayVariantDefinition(24, 4, 6, true, 6, 6, new[] { 4 }, new[] { 6 })
            ),
            new StayGroupDefinition(
                26,
                new StayVariantDefinition(61, 3, 56, false, 3, 3, new[] { 3 }, new[] { 56, 5 }),
                new StayVariantDefinition(62, 3, 58, false, 3, 3, new[] { 3 }, new[] { 58, 4 }),
                new StayVariantDefinition(63, 2, 56, false, 2, 2, new[] { 2 }, new[] { 56, 5 }),
                new StayVariantDefinition(64, 2, 58, false, 2, 2, new[] { 2 }, new[] { 58, 4 })
            ),
            new StayGroupDefinition(
                27,
                new StayVariantDefinition(65, 4, 59, true, 59, 59, new[] { 4 }, new[] { 59, 7 }),
                new StayVariantDefinition(66, 4, 60, true, 60, 60, new[] { 4 }, new[] { 60, 6 })
            )
        );
    }
}
