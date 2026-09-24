namespace FishermansSail.BoatRigs
{
    internal static partial class BoatRigCatalog
    {
        private static readonly BoatRigDefinition Brig = new BoatRigDefinition(
            "BOAT medi medium (50)",
            BrigStays(),
            new MastSupportDefinition(15, new[] { 3 }, new[] { 5 }),
            new MastSupportDefinition(16, new[] { 3 }, new[] { 4 }),
            new MastSupportDefinition(18, new[] { 2 }, new[] { 5 }),
            new MastSupportDefinition(20, new[] { 2 }, new[] { 4 }),
            new MastSupportDefinition(22, new[] { 4 }, new[] { 7 }),
            new MastSupportDefinition(24, new[] { 4 }, new[] { 6 }),
            new MastSupportDefinition(61, new[] { 3 }, new[] { 56, 5 }),
            new MastSupportDefinition(62, new[] { 3 }, new[] { 58, 4 }),
            new MastSupportDefinition(63, new[] { 2 }, new[] { 56, 5 }),
            new MastSupportDefinition(64, new[] { 2 }, new[] { 58, 4 }),
            new MastSupportDefinition(65, new[] { 4 }, new[] { 59, 7 }),
            new MastSupportDefinition(66, new[] { 4 }, new[] { 60, 6 })
        );
    }
}
