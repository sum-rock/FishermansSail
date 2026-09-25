using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using static MoreSailwindSails.Tests.AssemblyChecks.Shared.IlReader;

namespace MoreSailwindSails.Tests.AssemblyChecks.FishermansStay;

internal static class PatchChecks
{
    internal static void Run(Assembly assembly)
    {
        // Stay integration uses native save slots; inspect and exercise the pure parts
        // without constructing Unity objects or mutating an installed save.
        const string stayNamespace = "MoreSailwindSails.Stays.FishermansStay.";
        Type StayPatch(string name) =>
            assembly.GetType(stayNamespace + "Patches.FishermansStay" + name + "Patch", true);
        MethodInfo StayMethod(string name, string method) =>
            StayPatch(name)
                .GetMethod(
                    method,
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
                );
        foreach (string name in new[] { "Registration", "SaveCapacity", "MountButtons" })
            if (
                !StayMethod(name, "Postfix")
                    .GetCustomAttribute<HarmonyAfter>()
                    .info.after.Contains("com.nandbrew.shipyardexpansion")
            )
                throw new Exception("Stay integration must run after SE: " + name);
        if (
            !StayMethod("Load", "Prefix")
                .GetCustomAttribute<HarmonyBefore>()
                .info.before.Contains("com.nandbrew.shipyardexpansion")
        )
            throw new Exception("Stay snapshot preparation must precede SE load handling.");
        var capacity = StayMethod("SaveCapacity", "Postfix");
        var saveType = capacity.GetParameters()[0].ParameterType;
        var save = Activator.CreateInstance(saveType);
        var mastField = saveType.GetField("masts");
        var oldSlots = new bool[128];
        oldSlots[5] = true;
        mastField.SetValue(save, oldSlots);
        capacity.Invoke(null, new[] { save });
        var extended = (bool[])mastField.GetValue(save);
        if (extended.Length != 256 || !extended[5] || extended[128])
            throw new Exception("Stay save capacity lost old state or defaulted a new slot on.");
        var larger = new bool[512];
        larger[400] = true;
        mastField.SetValue(save, larger);
        capacity.Invoke(null, new[] { save });
        if (!ReferenceEquals(larger, mastField.GetValue(save)))
            throw new Exception("Stay capacity must preserve other mods' larger arrays.");
        if (!StayMethod("OrderRefresh", "Finalizer").IsDefined(typeof(HarmonyFinalizer)))
            throw new Exception("Stay preview state must be restored when native refresh throws.");
        if (!CalledMethods(StayMethod("OccupiedPreview", "Prefix")).Any(m => m.Name == "Protects"))
            throw new Exception("Stay previews must protect occupied mounts and their supports.");
        var loadCalls = CalledMethods(StayMethod("Load", "Prefix")).ToArray();
        if (
            !loadCalls.Any(m => m.DeclaringType == typeof(Array) && m.Name == "Resize")
            || !loadCalls.Any(m => m.Name == "PrepareSnapshot")
        )
            throw new Exception(
                "Stay load handling must prepare old snapshots before native sail removal."
            );
        var stayText = StayMethod("OrderText", "Prefix");
        if (
            !stayText
                .GetCustomAttribute<HarmonyBefore>()
                .info.before.Contains("com.nandbrew.nandfixes")
        )
            throw new Exception("Stay text protection must precede NANDFixes.");
        var stayLines = new System.Collections.Generic.List<string> { "existing" };
        object[] stayTextArguments =
        {
            "192: (ERROR): Fisherman's Stay (foremast 1 / main topmast 2) -> (no Fisherman's Stay)",
            stayLines,
        };
        if (
            (bool)stayText.Invoke(null, stayTextArguments)
            || (string)stayTextArguments[0] != ""
            || stayLines[0] != "existing"
            || stayLines.Count < 3
            || stayLines.Any(line => line.Length > 45)
        )
            throw new Exception(
                "Stay order guard failed to preserve safe input for later prefixes."
            );
        stayTextArguments[0] = "Fisherman's Flying Sail " + new string('x', 80);
        int lineCount = stayLines.Count;
        if (!(bool)stayText.Invoke(null, stayTextArguments) || stayLines.Count != lineCount)
            throw new Exception("Stay text guard intercepted another sail's order.");
        Console.WriteLine(
            "PASS: stay registration ordering, save capacity preservation, snapshot preparation, preview finalizer and scoped order-text protection."
        );
    }
}
