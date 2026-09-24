using UnityEngine;

namespace FishermansSail.Sails.FishermansStaysail.MkA
{
    internal static class FishermansStaysailMkA
    {
        // Stable across launches: Sailwind stores this index in boat saves.
        internal const int PrefabIndex = 401;
        internal const string DisplayName = "Fisherman's Staysail Mk.A";
        private static GameObject prefab;

        internal static void Register(PrefabsDirectory directory) =>
            prefab = FishermansStaysailPrefab.Register<FishermansStaysailMkAShape>(
                directory,
                prefab,
                PrefabIndex,
                DisplayName
            );

        internal static void AddToShipyard(Shipyard shipyard) =>
            FishermansStaysailPrefab.AddToShipyard(shipyard, prefab, DisplayName);
    }
}
