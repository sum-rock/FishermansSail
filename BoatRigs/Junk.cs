namespace FishermansSail
{
    internal static partial class BoatRigCatalog
    {
        private static readonly BoatRigDefinition Junk = new BoatRigDefinition(
            "BOAT junk medium (80)",
            new StayGroupDefinition(
                5,
                new StayVariantDefinition(16, 9, 10, false, 9, 9, new[] { 9 }, new[] { 10 }),
                new StayVariantDefinition(61, 58, 11, false, 58, 58, new[] { 58 }, new[] { 11 })
            ),
            new StayGroupDefinition(
                7,
                new StayVariantDefinition(5, 10, 12, true, 12, 12, new[] { 10 }, new[] { 12 }),
                new StayVariantDefinition(6, 11, 12, true, 12, 12, new[] { 11 }, new[] { 12 })
            ),
            new StayGroupDefinition(
                15,
                new StayVariantDefinition(65, 10, 53, true, 53, 53, new[] { 10 }, new[] { 53, 12 }),
                new StayVariantDefinition(66, 11, 53, true, 53, 53, new[] { 11 }, new[] { 53, 12 })
            )
        );
    }
}
