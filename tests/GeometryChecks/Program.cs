using MoreSailwindSails.Tests.GeometryChecks.FishermansFlyingSail;
using MoreSailwindSails.Tests.GeometryChecks.FishermansStay;

namespace MoreSailwindSails.Tests.GeometryChecks;

internal static class Program
{
    private static void Main()
    {
        RigChecks.Run();
        ProfileChecks.Run();
        StayChecks.Run();
        WinchChecks.Run();
        OrderTextChecks.Run();
        FlyingSailChecks.Run();
        RopeChecks.Run();
        TravelChecks.Run();
        MastInstallationChecks.Run();
        BillowChecks.Run();
        ShapingChecks.Run();
        AerodynamicChecks.Run();
        MeshChecks.Run();
        FishermansStaysail.MkA.CutChecks.Run();
        FishermansStaysail.MkB.CutChecks.Run();
        FishermansStaysail.MkC.CutChecks.Run();
        FishermansStaysail.CollisionChecks.Run();
        FishermansStaysail.ReefingChecks.Run();
        FishermansStaysail.SheetChecks.Run();
        FishermansStaysail.EdgeFitChecks.Run();
        FishermansStaysail.BillowChecks.Run();
        FishermansStaysail.FixedHeadChecks.Run();
    }
}
