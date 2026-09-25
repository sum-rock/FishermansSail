using cakeslice;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MoreSailwindSails.Sails
{
    internal static class FishermanWinchVisuals
    {
        internal static void ResetClonedOutline(GPButtonRopeWinch winch)
        {
            // A live donor already has an Outline from GoPointerButton.Start.
            // Remove that copy while the clone is inactive so Start can own one fresh outline.
            foreach (var outline in winch.GetComponents<Outline>())
            {
                outline.enabled = false;
                Object.Destroy(outline);
            }
            winch.enableRedOutline = false;
        }
    }
}
