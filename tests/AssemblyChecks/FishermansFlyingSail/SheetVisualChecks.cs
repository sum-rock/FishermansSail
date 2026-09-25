using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using static MoreSailwindSails.Tests.AssemblyChecks.Shared.IlReader;

namespace MoreSailwindSails.Tests.AssemblyChecks.FishermansFlyingSail;

internal static class SheetVisualChecks
{
    internal static void Run(Assembly assembly)
    {
        const string prefix = "MoreSailwindSails.Sails.FishermansFlyingSail.";
        const BindingFlags methods =
            BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.NonPublic
            | BindingFlags.Public
            | BindingFlags.DeclaredOnly;
        var patch = assembly.GetType(prefix + "Patches.FishermansFlyingSailSheetVisualPatch", true);
        var marker = assembly.GetType(prefix + "FishermansFlyingSailNativeSheetVisual", true);
        var route = assembly.GetType(prefix + "FishermansFlyingSailSupportLine", true);
        var rig = assembly.GetType(prefix + "FishermansFlyingSailRig", true);
        var target = patch.GetCustomAttribute<HarmonyPatch>().info;
        var postfix = patch.GetMethod("Postfix", methods);
        if (
            target.declaringType.Name != "RopeEffect"
            || target.methodName != "LateUpdate"
            || !postfix.IsDefined(typeof(HarmonyPostfix))
            || patch.GetMethod("Prefix", methods) != null
        )
            throw new Exception("Sheet visuals must leave native RopeEffect.LateUpdate running.");
        if (
            !CalledMethods(postfix)
                .OfType<MethodInfo>()
                .Any(m =>
                    m.Name == "GetComponent"
                    && m.IsGenericMethod
                    && m.GetGenericArguments().Single() == marker
                )
            || !Instructions(postfix).Any(i => i.Code.FlowControl == FlowControl.Cond_Branch)
            || !CalledMethods(postfix).Any(m => m.DeclaringType == marker && m.Name == "Suppress")
        )
            throw new Exception(
                "Native visual suppression must be guarded by the owned Flying Sail marker."
            );
        foreach (string name in new[] { "lineRenderer", "clothRope" })
        {
            var field = target.declaringType.GetField(
                name,
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            if (
                field == null
                || !postfix
                    .GetParameters()
                    .Any(p => p.Name == "___" + name && p.ParameterType == field.FieldType)
            )
                throw new Exception(
                    "Sheet suppression no longer matches the installed native rope visuals."
                );
        }
        foreach (var type in new[] { patch, marker, route })
        foreach (var method in type.GetMethods(methods))
        {
            foreach (var instruction in Instructions(method))
                if (
                    instruction.Code == OpCodes.Stfld
                    && instruction.Operand is FieldInfo field
                    && field.DeclaringType == target.declaringType
                )
                    throw new Exception("Visual replacement must not modify native rope state.");
            foreach (var called in CalledMethods(method))
                if (
                    called.DeclaringType.FullName is "UnityEngine.Cloth" or "UnityEngine.HingeJoint"
                    || called.Name
                        is "set_sharedMesh"
                            or "set_bones"
                            or "set_bindposes"
                            or "set_rotation"
                            or "set_localRotation"
                            or "set_localScale"
                )
                    throw new Exception(
                        "Sheet visuals must not alter cloth topology, bone orientation or hinge physics."
                    );
        }
        foreach (var type in new[] { marker, route, rig })
            if (!CalledMethods(type.GetMethod("OnDisable", methods)).Any(m => m.Name == "Hide"))
                throw new Exception("Disabling a sail or its visual owner must hide its ropes.");
        var hide = CalledMethods(marker.GetMethod("Hide", methods)).ToArray();
        if (!hide.Any(m => m.Name == "set_enabled") || !hide.Any(m => m.Name == "SetActive"))
            throw new Exception("Both line and optional ClothRope visuals must be suppressed.");
        var lateCalls = CalledMethods(rig.GetMethod("LateUpdate", methods)).ToList();
        if (
            lateCalls.FindIndex(m => m.DeclaringType == route && m.Name == "Draw")
            <= lateCalls.FindIndex(m => m.Name == "UpdateShapeBones")
        )
            throw new Exception("Rope drawing must follow the final sail pose.");
        Console.WriteLine(
            "PASS (IL): scoped sheet visual postfix, installed line/ClothRope fields, preserved native rope state, post-pose drawing and disable cleanup. Rendering remains a runtime check."
        );
    }
}
