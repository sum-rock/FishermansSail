using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using static MoreSailwindSails.Tests.AssemblyChecks.Shared.IlReader;

namespace MoreSailwindSails.Tests.AssemblyChecks.Compatibility;

internal static class TextureCatalogChecks
{
    internal static void Run(Assembly assembly)
    {
        const BindingFlags all = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        var patch = assembly.GetType(
            "MoreSailwindSails.Compatibility.Patches.ShipyardExpansionTextureCatalogPatch"
        );
        var target = patch.GetCustomAttribute<HarmonyPatch>().info;
        var prefix = patch.GetMethod("Prefix", all);
        if (target.methodName != "Setup" || !prefix.IsDefined(typeof(HarmonyPrefix)))
            throw new Exception("Seed the catalog before SE assigns prefab texture indices.");
        var setupCalls = CalledMethods(target.declaringType.GetMethod("Setup")).ToArray();
        foreach (string method in new[] { "Contains", "Add", "IndexOf" })
            if (!setupCalls.Any(m => m.Name == method && m.DeclaringType.Name == "List`1"))
                throw new Exception(
                    "Installed SE texture discovery changed; review catalog compatibility."
                );
        var options = (IDictionary)target.declaringType.GetField("allowedTexMap").GetValue(null);
        foreach (var pair in new[] { (12, 1), (6, 2), (17, 3) })
            if (!((int[])options[pair.Item1]).SequenceEqual(new[] { 0, pair.Item2 }))
                throw new Exception("Installed SE fixed texture option indices changed.");

        var compatibility = assembly.GetType(
            "MoreSailwindSails.Compatibility.ShipyardExpansionTextureCatalog"
        );
        if (
            !CalledMethods(prefix)
                .Any(m => m.DeclaringType == compatibility && m.Name == "SeedPlain")
        )
            throw new Exception("Missing early texture catalog seeding.");
        var calls = CalledMethods(compatibility.GetMethod("SeedPlain", all)).ToArray();
        if (!calls.Any(m => m.Name == "get_sharedMaterial"))
            throw new Exception(
                "Resolve the native plain texture through a non-instantiating material read."
            );
        if (
            calls.Any(m =>
                m.Name.StartsWith("set_")
                || m.Name is "get_material" or "SetTexture" or "Clear" or "Insert"
            )
        )
            throw new Exception(
                "Catalog seeding must not rewrite materials or renumber assigned entries."
            );
        foreach (string family in new[] { "FishermansFlyingSail", "FishermansStaysail" })
        {
            var appearance = assembly.GetType(
                $"MoreSailwindSails.Sails.{family}.{family}Appearance"
            );
            if (
                !CalledMethods(appearance.GetMethod("Configure", all))
                    .Any(m => m.DeclaringType == compatibility && m.Name == "get_HasPlainFirst")
            )
                throw new Exception(
                    "Custom appearance must verify texture zero is actually plain."
                );
        }
        Console.WriteLine(
            "PASS: installed SE discovery/options contract, early catalog patch and guarded custom appearance; Unity initialization requires runtime validation."
        );
    }
}
