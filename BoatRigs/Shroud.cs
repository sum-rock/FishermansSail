namespace FishermansSail
{
    internal static partial class BoatRigCatalog
    {
        private static readonly BoatRigDefinition Shroud = new BoatRigDefinition(
            "BOAT Shroud Large",
            new StayGroupDefinition(
                21,
                new StayVariantDefinition(25, 7, 9, true, 9, 9, new[] { 7 }, new[] { 9 })
            )
        );
    }
}
