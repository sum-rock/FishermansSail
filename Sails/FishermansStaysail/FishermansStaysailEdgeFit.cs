using UnityEngine;

namespace FishermansSail.Sails.FishermansStaysail
{
    internal static class FishermansStaysailEdgeFit
    {
        // Shape the free leech within both edge budgets. The fixed head is an
        // input only; infeasible spans keep the tension solver's finite fallback.
        internal static bool Fit(
            Vector3 head,
            Vector3 clew,
            Vector3 tack,
            Vector3 normal,
            Vector3 down,
            float width,
            float load,
            float unroll,
            float leechLength,
            float footLength,
            Vector3[] points
        )
        {
            float deployment = FishermansStaysailBillow.Deployment(unroll);
            var bow = FishermansStaysailBillow.SupportBow(clew, head, normal, down, width, load);
            return FishermansStaysailTension.Fit(
                clew,
                head,
                tack,
                bow * deployment,
                leechLength,
                footLength,
                deployment,
                points
            );
        }
    }
}
