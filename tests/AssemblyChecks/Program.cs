using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using HarmonyLib;

// Validate the installed game's actual signatures without starting Unity or
// patching the user's game process. No game objects or saves are created.
string gameDir =
    args.Length > 0
        ? args[0]
        : Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".local/share/Steam/steamapps/common/Sailwind"
        );
string[] libraryDirs =
{
    Path.Combine(gameDir, "Sailwind_Data/Managed"),
    Path.Combine(gameDir, "BepInEx/core"),
};
AssemblyLoadContext.Default.Resolving += (context, name) =>
{
    foreach (var dir in libraryDirs)
    {
        var file = Path.Combine(dir, name.Name + ".dll");
        if (File.Exists(file))
            return context.LoadFromAssemblyPath(file);
    }
    return null;
};
var assembly = Assembly.LoadFrom(Path.Combine(AppContext.BaseDirectory, "FishermansSail.dll"));
int count = 0;
foreach (var type in assembly.GetTypes())
{
    var attribute = type.GetCustomAttributes<HarmonyPatch>().SingleOrDefault();
    if (attribute == null)
        continue;
    var info = attribute.info;
    const BindingFlags all =
        BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    MethodBase target =
        info.methodType == MethodType.Constructor
            ? info.declaringType.GetConstructor(all, null, Type.EmptyTypes, null)
            : info.declaringType.GetMethod(info.methodName, all);
    if (target == null)
        throw new Exception("Missing Harmony target: " + type.Name);
    foreach (
        var patch in type.GetMethods(
            BindingFlags.Static
                | BindingFlags.NonPublic
                | BindingFlags.Public
                | BindingFlags.DeclaredOnly
        )
    )
    {
        if (!patch.IsDefined(typeof(HarmonyPrefix)) && !patch.IsDefined(typeof(HarmonyPostfix)))
            continue;
        foreach (var parameter in patch.GetParameters())
        {
            Type actual = null;
            if (parameter.Name.StartsWith("___"))
                actual = info.declaringType.GetField(parameter.Name.Substring(3), all)?.FieldType;
            else if (parameter.Name == "__instance")
                actual = info.declaringType;
            else if (parameter.Name == "__result")
                actual = (target as MethodInfo)?.ReturnType;
            else
                actual = target
                    .GetParameters()
                    .SingleOrDefault(p => p.Name == parameter.Name)
                    ?.ParameterType;
            var expected = parameter.ParameterType;
            if (expected.IsByRef)
                expected = expected.GetElementType();
            if (actual != null && actual.IsByRef)
                actual = actual.GetElementType();
            if (actual == null || actual != expected)
                throw new Exception(
                    $"Invalid injection: {type.Name}.{patch.Name}({parameter.Name})."
                );
        }
    }
    count++;
}
if (count != 10)
    throw new Exception($"Expected all 10 patch classes, found {count}.");

var dataType = Assembly
    .LoadFrom(Path.Combine(libraryDirs[0], "Assembly-CSharp.dll"))
    .GetType("SaveBoatCustomizationData");
var expand = assembly
    .GetType("FishermansSail.StaySaveCapacityPatch")
    .GetMethod("Postfix", BindingFlags.Static | BindingFlags.NonPublic);
foreach (int length in new[] { 30, 128, 256, 384 })
{
    var data = Activator.CreateInstance(dataType);
    var flags = new bool[length];
    flags[5] = true;
    dataType.GetField("masts").SetValue(data, flags);
    expand.Invoke(null, new[] { data });
    var result = (bool[])dataType.GetField("masts").GetValue(data);
    if (result.Length != Math.Max(length, 256) || !result[5])
        throw new Exception("Save capacity changed existing flags or shrank another mod's array.");
}
Console.WriteLine(
    $"PASS: {count} Harmony targets and injected argument types; old and extended save-array capacity."
);
