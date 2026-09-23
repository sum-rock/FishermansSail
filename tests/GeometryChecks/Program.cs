using FishermansSail.Tests.GeometryChecks.FishermansFlyingSail;
using FishermansSail.Tests.GeometryChecks.FishermansStay;

namespace FishermansSail.Tests.GeometryChecks;

internal static class Program
{
    private static void Main()
    {
        RigChecks.Run();
        StayChecks.Run();
        OrderTextChecks.Run();
        FlyingSailChecks.Run();
        TravelChecks.Run();
        MastInstallationChecks.Run();
        BillowChecks.Run();
        ShapingChecks.Run();
        AerodynamicChecks.Run();
        MeshChecks.Run();
    }
}
