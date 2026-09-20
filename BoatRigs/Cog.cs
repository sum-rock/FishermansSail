namespace FishermansSail
{
    internal static partial class BoatRigCatalog
    {
        private static readonly BoatRigDefinition Cog = new BoatRigDefinition(
            "BOAT medi small (40)",
            new StayGroupDefinition(
                6,
                new StayVariantDefinition(51, 8, 5, true, 5, 5, new[] { 8 }, new[] { 5 }),
                new StayVariantDefinition(58, 5, 57, true, 57, 57, new[] { 5 }, new[] { 57 }),
                new StayVariantDefinition(65, 8, 6, true, 6, 6, new[] { 8 }, new[] { 6 })
            )
        );
    }
}
