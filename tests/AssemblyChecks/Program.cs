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
    Path.Combine(gameDir, "BepInEx/plugins/ShipyardExpansion"),
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
        if (
            !patch.IsDefined(typeof(HarmonyPrefix))
            && !patch.IsDefined(typeof(HarmonyPostfix))
            && !patch.IsDefined(typeof(HarmonyFinalizer))
        )
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
            else if (parameter.Name == "__exception")
                actual = typeof(Exception);
            else if (parameter.Name == "__state")
                actual = type.GetMethods(all)
                    .Single(m => m.IsDefined(typeof(HarmonyPrefix)))
                    .GetParameters()
                    .Single(p => p.Name == "__state")
                    .ParameterType;
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
if (count != 21)
    throw new Exception($"Expected all 21 patch classes, found {count}.");

// The installed SE save loader and texture button both go through this update.
// Guarding only the selector would allow an old saved pattern to reappear.
var textureTarget = assembly
    .GetType("FishermansSail.FishermanPlainTexturePatch")
    .GetCustomAttribute<HarmonyPatch>()
    .info;
foreach (var name in new[] { "SetTexture", "NextTexture" })
    if (
        !CalledMethods(textureTarget.declaringType.GetMethod(name))
            .Any(m =>
                m.DeclaringType == textureTarget.declaringType && m.Name == textureTarget.methodName
            )
    )
        throw new Exception($"SE {name} no longer passes through the plain-texture guard.");
var textureButtonPostfix = assembly
    .GetType("FishermansSail.FishermanTextureButtonPatch")
    .GetMethod("Postfix", BindingFlags.Static | BindingFlags.NonPublic);
if (
    !textureButtonPostfix
        .GetCustomAttribute<HarmonyAfter>()
        .info.after.Contains("com.nandbrew.shipyardexpansion")
)
    throw new Exception("Hide texture options after SE refreshes its button visibility.");
Console.WriteLine(
    "PASS: installed SE saved/cycled texture routes and texture-button patch ordering."
);

var travelPatch = assembly.GetType("FishermansSail.FishermanTravelPatch");
var travelTarget = travelPatch.GetCustomAttribute<HarmonyPatch>().info;
var travelPrefix = travelPatch.GetMethod("Prefix", BindingFlags.Static | BindingFlags.NonPublic);
var travelPostfix = travelPatch.GetMethod("Postfix", BindingFlags.Static | BindingFlags.NonPublic);
if (
    travelTarget.declaringType.Name != "JibAngleMaster"
    || travelTarget.methodName != "Update"
    || !travelPrefix.IsDefined(typeof(HarmonyPrefix))
    || !travelPostfix.IsDefined(typeof(HarmonyPostfix))
)
    throw new Exception("Travel protection must surround the native sheet and sway update.");
var prefixCalls = CalledMethods(travelPrefix).ToArray();
var postfixCalls = CalledMethods(travelPostfix).ToArray();
if (
    !prefixCalls.Any(m =>
        m.Name == "GetComponent"
        && m.IsGenericMethod
        && m.GetGenericArguments().Single().FullName == "FishermansSail.FishermanSailRig"
    )
    || !prefixCalls.Any(m =>
        m.DeclaringType.FullName == "FishermansSail.FishermanTravel" && m.Name == "Clamp"
    )
    || !postfixCalls.Any(m =>
        m.DeclaringType.FullName == "FishermansSail.FishermanTravel" && m.Name == "ConstrainHinge"
    )
    || !postfixCalls.Any(m =>
        m.DeclaringType.FullName == "UnityEngine.HingeJoint" && m.Name == "set_limits"
    )
)
    throw new Exception("Missing fisherman scoping or live hinge travel enforcement.");
foreach (var called in prefixCalls.Concat(postfixCalls))
    if (
        called.DeclaringType.FullName is "UnityEngine.Cloth" or "UnityEngine.Transform"
        || called.Name == "ResetHingeRestingRot"
    )
        throw new Exception("Travel protection must not reset cloth or snap the sail transform.");
Console.WriteLine(
    "PASS: fisherman travel wraps native sheet/sway Update and constrains the hinge without resetting cloth or transforms."
);

// Cloth mesh assignment belongs to inactive prefab construction. Replacing
// a live Cloth renderer's mesh caused the 0.7.11 detach/reset regression even
// though the two meshes passed all pure geometry checks.
var rigType = assembly.GetType("FishermansSail.FishermanSailRig");
foreach (
    var method in rigType.GetMethods(
        BindingFlags.Instance
            | BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.DeclaredOnly
    )
)
{
    foreach (var called in CalledMethods(method))
        if (
            called.Name == "set_sharedMesh"
            && called.DeclaringType.FullName == "UnityEngine.SkinnedMeshRenderer"
        )
            throw new Exception(
                $"Live cloth mesh replacement in {method.Name}; only prefab construction may assign it."
            );
}
Console.WriteLine("PASS: installed sail callbacks retain their initialized cloth mesh.");

var shapeUpdate = rigType.GetMethod(
    "UpdateShapeBones",
    BindingFlags.Instance | BindingFlags.NonPublic
);
if (shapeUpdate == null)
    throw new Exception("Missing bone-driven billow update.");
foreach (var called in CalledMethods(shapeUpdate))
    if (
        called.DeclaringType.FullName == "UnityEngine.Cloth"
        || called.Name
            is "set_sharedMesh"
                or "set_bones"
                or "set_bindposes"
                or "set_localScale"
                or "set_localRotation"
                or "set_enabled"
    )
        throw new Exception(
            "Billow must move existing bones without resetting or replacing cloth or reflecting transforms."
        );
Console.WriteLine(
    "PASS: billow update only moves existing shaping transforms; cloth lifecycle and transform scales remain untouched."
);

// Run the actual text prefix without Unity objects. HarmonyX runs later
// prefixes even when this one returns false, so their input must be safe too.
var textPrefix = assembly
    .GetType("FishermansSail.FishermanOrderTextPatch")
    .GetMethod("Prefix", BindingFlags.Static | BindingFlags.NonPublic);
if (!textPrefix.GetCustomAttribute<HarmonyBefore>().info.before.Contains("com.nandbrew.nandfixes"))
    throw new Exception("Fisherman text protection must run before NANDFixes.");
var orderLines = new System.Collections.Generic.List<string> { "existing order line" };
object[] textArguments = { "0: Fisherman's Sail Prototype (150% x 115%) -> (no sail)", orderLines };
if (
    (bool)textPrefix.Invoke(null, textArguments)
    || (string)textArguments[0] != ""
    || orderLines.Count < 3
    || orderLines[0] != "existing order line"
    || orderLines.Any(line => line.Length > 45)
)
    throw new Exception("Text guard did not consume and append the removal order safely.");
textArguments[0] = "192: shipyard fee";
int previousCount = orderLines.Count;
if (
    !(bool)textPrefix.Invoke(null, textArguments)
    || orderLines.Count != previousCount
    || (string)textArguments[0] != "192: shipyard fee"
)
    throw new Exception("Text guard changed an unrelated order line.");
Console.WriteLine(
    "PASS: actual order-text prefix, NANDFixes ordering, safe input for later HarmonyX prefixes, and native-list preservation."
);

var controlsPatch = assembly.GetType("FishermansSail.FishermanControlsPatch");
if (
    !controlsPatch
        .GetMethod("Finalizer", BindingFlags.NonPublic | BindingFlags.Static)
        .IsDefined(typeof(HarmonyFinalizer))
)
    throw new Exception("Mast sail-list restoration must run even when native binding throws.");
Console.WriteLine(
    $"PASS: {count} Harmony targets and injected argument types; independent-control restoration and native mast integration."
);

// Decode call operands without asking Harmony to create native patch stubs.
static System.Collections.Generic.IEnumerable<MethodBase> CalledMethods(MethodInfo method)
{
    var bytes = method.GetMethodBody()?.GetILAsByteArray();
    if (bytes == null)
        yield break;
    var opcodes = typeof(System.Reflection.Emit.OpCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.FieldType == typeof(System.Reflection.Emit.OpCode))
        .Select(f => (System.Reflection.Emit.OpCode)f.GetValue(null))
        .ToDictionary(op => unchecked((ushort)op.Value));
    using var reader = new BinaryReader(new MemoryStream(bytes));
    while (reader.BaseStream.Position < bytes.Length)
    {
        ushort key = reader.ReadByte();
        if (key == 0xfe)
            key = (ushort)(0xfe00 | reader.ReadByte());
        var operand = opcodes[key].OperandType;
        if (operand == System.Reflection.Emit.OperandType.InlineMethod)
        {
            yield return method.Module.ResolveMethod(
                reader.ReadInt32(),
                method.DeclaringType.GetGenericArguments(),
                method.GetGenericArguments()
            );
            continue;
        }
        int size = operand switch
        {
            System.Reflection.Emit.OperandType.InlineNone => 0,
            System.Reflection.Emit.OperandType.ShortInlineBrTarget
            or System.Reflection.Emit.OperandType.ShortInlineI
            or System.Reflection.Emit.OperandType.ShortInlineVar => 1,
            System.Reflection.Emit.OperandType.InlineVar => 2,
            System.Reflection.Emit.OperandType.InlineI8
            or System.Reflection.Emit.OperandType.InlineR => 8,
            System.Reflection.Emit.OperandType.InlineSwitch => reader.ReadInt32() * 4,
            _ => 4,
        };
        reader.BaseStream.Position += size;
    }
}
