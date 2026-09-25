using UnityEngine;

namespace MoreSailwindSails.Sails.FishermansStaysail.MkC
{
    internal static class FishermansStaysailMkC
    {
        // Stable across launches; preserve the existing marks at 401 and 402.
        internal const int PrefabIndex = 403;
        internal const string DisplayName = "Fisherman's Staysail Mk.C";
        private static GameObject prefab;

        internal static void Register(PrefabsDirectory directory) =>
            prefab = FishermansStaysailPrefab.Register<FishermansStaysailMkCShape>(
                directory,
                prefab,
                PrefabIndex,
                DisplayName
            );

        internal static void AddToShipyard(Shipyard shipyard) =>
            FishermansStaysailPrefab.AddToShipyard(shipyard, prefab, DisplayName);
    }
}
