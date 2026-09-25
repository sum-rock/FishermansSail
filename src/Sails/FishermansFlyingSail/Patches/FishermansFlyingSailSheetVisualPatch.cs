using HarmonyLib;
using UnityEngine;

namespace MoreSailwindSails.Sails.FishermansFlyingSail.Patches
{
    [HarmonyPatch(typeof(RopeEffect), "LateUpdate")]
    internal static class FishermansFlyingSailSheetVisualPatch
    {
        [HarmonyPostfix]
        private static void Postfix(
            RopeEffect __instance,
            LineRenderer ___lineRenderer,
            ClothRope ___clothRope
        )
        {
            var visual = __instance.GetComponent<FishermansFlyingSailNativeSheetVisual>();
            if (visual)
                visual.Suppress(___lineRenderer, ___clothRope);
        }
    }
}
