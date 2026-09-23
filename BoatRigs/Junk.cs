namespace FishermansSail
{
    internal static partial class BoatRigCatalog
    {
        private static readonly BoatRigDefinition Junk = new BoatRigDefinition(
            "BOAT junk medium (80)",
            new MastSupportDefinition(16, new[] { 9 }, new[] { 10 }),
            new MastSupportDefinition(61, new[] { 58 }, new[] { 11 }),
            new MastSupportDefinition(5, new[] { 10 }, new[] { 12 }),
            new MastSupportDefinition(6, new[] { 11 }, new[] { 12 }),
            new MastSupportDefinition(65, new[] { 10 }, new[] { 53, 12 }),
            new MastSupportDefinition(66, new[] { 11 }, new[] { 53, 12 })
        );
    }
}
