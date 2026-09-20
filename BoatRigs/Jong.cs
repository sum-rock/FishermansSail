namespace FishermansSail
{
    internal static partial class BoatRigCatalog
    {
        private static readonly BoatRigDefinition Jong = new BoatRigDefinition(
            "BOAT junk large (70)",
            new StayGroupDefinition(
                7,
                new StayVariantDefinition(10, 1, 2, false, 1, 1, new[] { 1 }, new[] { 2 })
            ),
            new StayGroupDefinition(
                9,
                new StayVariantDefinition(12, 2, 3, true, 3, 3, new[] { 2 }, new[] { 3 }),
                new StayVariantDefinition(55, 51, 52, false, 51, 51, new[] { 51 }, new[] { 52 }),
                new StayVariantDefinition(58, 51, 3, false, 51, 51, new[] { 51 }, new[] { 3 }),
                new StayVariantDefinition(73, 1, 52, false, 1, 1, new[] { 1 }, new[] { 52 })
            ),
            new StayGroupDefinition(
                11,
                new StayVariantDefinition(71, 52, 4, true, 4, 4, new[] { 52 }, new[] { 4 })
            )
        );
    }
}
