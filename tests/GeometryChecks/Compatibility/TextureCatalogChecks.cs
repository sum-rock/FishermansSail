using System;
using System.Collections.Generic;
using System.Linq;
using MoreSailwindSails.Compatibility;

namespace MoreSailwindSails.Tests.GeometryChecks.Compatibility;

internal static class TextureCatalogChecks
{
    internal static void Run()
    {
        // First-seen order captured by the 2026-09-25 runtime trace. SE Setup
        // appends unique textures, then assigns IndexOf and fixed option indices.
        string[] observed =
        {
            "medi small cloth paint Diffuse Color",
            "ParticleCloudWhite",
            "dhow small cloth paint",
            "junk small cloth paint Diffuse Color",
            "dhow medium cloth paint",
            "sail junk medium paint Diffuse Color",
            "medi medium sail paint Diffuse Color",
        };
        var catalog = new List<string>();
        foreach (string texture in observed.Concat(observed))
        {
            TextureCatalogOrder.SeedPlain(catalog, observed[1]);
            if (!catalog.Contains(texture))
                catalog.Add(texture);
        }
        var expected = new[] { observed[1], observed[0] }.Concat(observed.Skip(2));
        if (!catalog.SequenceEqual(expected))
            throw new Exception(
                "Saved plain=0 and SE's painted option indices must retain their meanings."
            );
        // The observed full gaff loaded index 0, while its white bundle used
        // ParticleCloudWhite. Both now resolve to the same texture.
        if (catalog[0] != observed[1] || catalog.IndexOf(observed[0]) != 1)
            throw new Exception("The captured full-gaff/plain-texture regression remains.");
        catalog.Add("other mod texture");
        var snapshot = catalog.ToArray();
        TextureCatalogOrder.SeedPlain(catalog, observed[1]);
        if (!catalog.SequenceEqual(snapshot))
            throw new Exception("Repeated setup must not move or drop existing texture indices.");
        var alreadyAssigned = observed.ToList();
        TextureCatalogOrder.SeedPlain(alreadyAssigned, observed[1]);
        if (!alreadyAssigned.SequenceEqual(observed))
            throw new Exception("Never renumber a catalog after indices have been assigned.");
        Console.WriteLine(
            "PASS: captured SE texture-order regression, saved plain selection, painted indices, repeated setup and existing catalog preservation."
        );
    }
}
