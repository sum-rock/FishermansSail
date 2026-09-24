using System;
using System.IO;
using System.Linq;
using System.Reflection;
using static FishermansSail.Tests.AssemblyChecks.Shared.IlReader;

namespace FishermansSail.Tests.AssemblyChecks.FishermansStaysail;

internal static class SailInfoChecks
{
    internal static void Run(Assembly assembly, string gameDir)
    {
        const BindingFlags all =
            BindingFlags.Static
            | BindingFlags.Instance
            | BindingFlags.Public
            | BindingFlags.NonPublic;
        var patch = assembly.GetType(
            "FishermansSail.Sails.FishermansStaysail.Patches.FishermansStaysailSailInfoPatch",
            true
        );
        var find = patch.GetMethod("FindTarget", all);
        if (
            find.Invoke(null, new object[] { null }) != null
            || find.Invoke(null, new object[] { typeof(string) }) != null
        )
            throw new Exception("Absent/incompatible SailInfo must not require a patch.");
        var file = Path.Combine(gameDir, "BepInEx/plugins/SailInfo.dll");
        if (!File.Exists(file))
        {
            Console.WriteLine("SKIP: optional installed SailInfo signature check (DLL absent).");
            return;
        }
        var type = Assembly.LoadFrom(file).GetType("SailInfo.WinchInfoSail", true);
        var target = (MethodInfo)find.Invoke(null, new object[] { type });
        if (target == null || !CalledMethods(type.GetMethod("WinchHUD", all)).Contains(target))
            throw new Exception("Angle patch must target the actual installed SailInfo HUD call.");
        var prefix = patch.GetMethod("Prefix", all);
        foreach (var parameter in prefix.GetParameters())
        {
            var expected =
                parameter.Name == "__result"
                    ? target.ReturnType.MakeByRefType()
                    : type.GetField(parameter.Name.Substring(3), all)?.FieldType;
            if (expected != parameter.ParameterType)
                throw new Exception("Invalid optional SailInfo injection: " + parameter.Name);
        }
        var calls = CalledMethods(prefix).ToArray();
        if (!calls.Any(m => m.Name == "TrySheetAngle") || calls.Any(m => m.Name == "Clamp"))
            throw new Exception("SailInfo should report measured Mk.A travel without clipping it.");
        Console.WriteLine(
            "PASS: optional SailInfo absence handling, installed HUD angle target and injected fields."
        );
    }
}
