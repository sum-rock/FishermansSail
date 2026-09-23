namespace FishermansSail
{
    internal static partial class BoatRigCatalog
    {
        private static readonly BoatRigDefinition Leopard = new BoatRigDefinition(
            "BOAT LEOPARD (207)",
            new StayGroupDefinition(
                22,
                new StayVariantDefinition(18, 7, 12, true, 12, 12, new[] { 7 }, new[] { 12 })
            ),
            new StayGroupDefinition(
                21,
                new StayVariantDefinition(17, 8, 12, true, 12, 12, new[] { 8 }, new[] { 12 })
            ),
            new StayGroupDefinition(
                23,
                new StayVariantDefinition(19, 7, 11, true, 11, 11, new[] { 7 }, new[] { 11 })
            )
        );
    }
}
