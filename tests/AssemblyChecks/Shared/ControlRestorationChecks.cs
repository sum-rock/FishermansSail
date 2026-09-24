using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace FishermansSail.Tests.AssemblyChecks.Shared;

internal static class ControlRestorationChecks
{
    internal static void Run(Assembly assembly, string patchName, string feature)
    {
        var patch = assembly.GetType(patchName, true);
        var target = patch.GetCustomAttribute<HarmonyPatch>().info;
        var finalizer = patch.GetMethod("Finalizer", BindingFlags.NonPublic | BindingFlags.Static);
        var sails = target.declaringType.GetField("sails");
        var parameters = finalizer?.GetParameters();
        if (
            target.declaringType.FullName != "Mast"
            || target.methodName != "UpdateControllerAttachments"
            || finalizer == null
            || !finalizer.IsDefined(typeof(HarmonyFinalizer))
            || finalizer.ReturnType != typeof(void)
            || parameters.Length != 3
            || parameters[0].Name != "__instance"
            || parameters[0].ParameterType != target.declaringType
            || parameters[1].Name != "__state"
            || parameters[1].ParameterType != sails.FieldType
            || parameters[2].Name != "__exception"
            || parameters[2].ParameterType != typeof(Exception)
        )
            throw new Exception(feature + " control finalizer target or signature changed.");

        var instructions = IlReader
            .Instructions(finalizer)
            .Where(i => i.Code != OpCodes.Nop)
            .ToArray();
        int store = Array.FindIndex(
            instructions,
            i => i.Code == OpCodes.Stfld && Equals(i.Operand, sails)
        );
        int refresh = Array.FindIndex(
            instructions,
            i =>
                i.Operand is MethodBase method
                && method.DeclaringType == target.declaringType
                && method.Name == "UpdateSailOrder"
        );
        // Verify the actual assignment's source and destination, not merely
        // the presence of a finalizer attribute or an unrelated field store.
        if (
            store < 2
            || instructions[store - 2].Code != OpCodes.Ldarg_0
            || instructions[store - 1].Code != OpCodes.Ldarg_1
            || refresh <= store
        )
            throw new Exception(
                feature
                    + " finalizer must assign __state to __instance.sails before refreshing order."
            );

        // UpdateSailOrder sorts live Transform positions and calls GetComponent.
        // This standalone check inspects IL; it does not execute Unity recovery.
        Console.WriteLine(
            $"PASS (structural): {feature} control finalizer target/signature, saved-list assignment and subsequent native order refresh; exception recovery in Unity not executed."
        );
    }
}
