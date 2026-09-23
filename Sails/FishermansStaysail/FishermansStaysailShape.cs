using UnityEngine;

namespace FishermansSail.Sails.FishermansStaysail
{
    // Each mark supplies a cut and labels; mounting, reefing and sheeting stay
    // in the family rig. This component is copied with the native sail prefab.
    internal abstract class FishermansStaysailShape : MonoBehaviour
    {
        internal abstract string ObjectPrefix { get; }
        internal virtual float UpperCornerTrim => 0;
        internal abstract FishermansStaysailMeshData Create(float width, float headSlope);
    }
}
