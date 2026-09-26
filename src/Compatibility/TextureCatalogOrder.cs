using System;
using System.Collections.Generic;

namespace MoreSailwindSails.Compatibility
{
    internal static class TextureCatalogOrder
    {
        // SE persists numeric indices and hard-codes plain as zero in its options.
        // Seed before discovery; never renumber a catalog with assigned indices.
        internal static void SeedPlain<T>(List<T> textures, T plain)
            where T : class
        {
            if (plain == null)
                throw new ArgumentNullException(nameof(plain));
            if (textures.Count == 0)
                textures.Add(plain);
        }
    }
}
