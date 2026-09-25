using MoreSailwindSails.Sails.FishermansFlyingSail;
using UnityEngine;

namespace MoreSailwindSails.Tests.GeometryChecks.FishermansFlyingSail;

internal static class HoistPose
{
    internal static Vector3 Corner(Vector3[] rest, int index, float unroll) =>
        FishermansFlyingSailMastInstallationGeometry.HoistCorner(
            rest[index],
            rest[0],
            new Vector3(rest[3].x + rest[0].z * 0.2f, 0, rest[0].z),
            unroll
        );
}
