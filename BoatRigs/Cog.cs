namespace FishermansSail.BoatRigs
{
    internal static partial class BoatRigCatalog
    {
        private static readonly BoatRigDefinition Cog = new BoatRigDefinition(
            "BOAT medi small (40)",
            new MastSupportDefinition(51, new[] { 8 }, new[] { 5 }),
            new MastSupportDefinition(58, new[] { 5 }, new[] { 57 }),
            new MastSupportDefinition(65, new[] { 8 }, new[] { 6 })
        );
    }
}
