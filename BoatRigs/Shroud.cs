namespace FishermansSail.BoatRigs
{
    internal static partial class BoatRigCatalog
    {
        private static readonly BoatRigDefinition Shroud = new BoatRigDefinition(
            "BOAT Shroud Large",
            ShroudStays(),
            new MastSupportDefinition(25, new[] { 7 }, new[] { 9 })
        );
    }
}
