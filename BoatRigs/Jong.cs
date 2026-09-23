namespace FishermansSail
{
    internal static partial class BoatRigCatalog
    {
        private static readonly BoatRigDefinition Jong = new BoatRigDefinition(
            "BOAT junk large (70)",
            new MastSupportDefinition(10, new[] { 1 }, new[] { 2 }),
            new MastSupportDefinition(12, new[] { 2 }, new[] { 3 }),
            new MastSupportDefinition(55, new[] { 51 }, new[] { 52 }),
            new MastSupportDefinition(58, new[] { 51 }, new[] { 3 }),
            new MastSupportDefinition(73, new[] { 1 }, new[] { 52 }),
            new MastSupportDefinition(71, new[] { 52 }, new[] { 4 })
        );
    }
}
