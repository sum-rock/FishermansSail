using FishermansSail.Sails.FishermansFlyingSail;
using UnityEngine;

internal static class HoistPose
{
    internal static Vector3 Corner(Vector3[] rest, int index, float unroll) =>
        FishermansFlyingSailMastInstallationGeometry.HoistCorner(
            rest[index],
            rest[0],
            rest[2] + Vector3.left * (-rest[0].z * 0.2f),
            unroll
        );
}
