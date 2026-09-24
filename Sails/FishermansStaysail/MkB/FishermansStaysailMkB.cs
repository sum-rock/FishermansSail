using UnityEngine;

namespace FishermansSail.Sails.FishermansStaysail.MkB
{
    internal static class FishermansStaysailMkB
    {
        // Keep Mk.A at 401 so existing sails retain their save identity.
        internal const int PrefabIndex = 402;
        internal const string DisplayName = "Fisherman's Staysail Mk.B";
        private static GameObject prefab;

        internal static void Register(PrefabsDirectory directory) =>
            prefab = FishermansStaysailPrefab.Register<FishermansStaysailMkBShape>(
                directory,
                prefab,
                MkA.FishermansStaysailMkA.SourceIndex,
                PrefabIndex,
                DisplayName,
                "FishermansStaysailMkB",
                width => FishermansStaysailMkBGeometry.Create(width)
            );

        internal static void AddToShipyard(Shipyard shipyard) =>
            FishermansStaysailPrefab.AddToShipyard(shipyard, prefab, DisplayName);
    }
}
