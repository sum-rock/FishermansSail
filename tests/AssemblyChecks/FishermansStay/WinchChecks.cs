using System;
using System.Linq;
using System.Reflection;
using static FishermansSail.Tests.AssemblyChecks.Shared.IlReader;

namespace FishermansSail.Tests.AssemblyChecks.FishermansStay;

internal static class WinchChecks
{
    internal static void Run(Assembly assembly)
    {
        const BindingFlags all =
            BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Static
            | BindingFlags.Instance;
        var manager = assembly.GetType("FishermansSail.Controls.FishermanWinchControls", true);
        var owned = manager.GetNestedType("OwnedWinch", BindingFlags.NonPublic);
        var constructor = owned.GetConstructors(all).Single();
        var cloneCalls = CalledMethods(constructor).ToArray();
        if (
            !cloneCalls.Any(m => m.Name == "ResetClonedOutline")
            || !cloneCalls.Any(m => m.Name == "SetActive")
            || !cloneCalls.Any(m => m.Name == "Instantiate")
        )
            throw new Exception(
                "Common winch construction must initialize inactive clones and reset outlines."
            );
        var placement = CalledMethods(owned.GetMethod("Position", all)).ToArray();
        if (placement.Any(m => m.Name is "set_localRotation" or "set_localEulerAngles" or "Rotate"))
            throw new Exception("Mount refresh must not overwrite native winch input rotation.");
        var dispose = CalledMethods(owned.GetMethod("Dispose", all)).ToArray();
        if (
            !dispose.Any(m => m.Name == "Suspend")
            || !dispose.Any(m => m.Name == "SetParent")
            || !dispose.Any(m => m.Name == "Destroy")
        )
            throw new Exception(
                "Winch disposal must release its slot, preserve the native controller, and destroy owned objects."
            );
        var suspend = CalledMethods(owned.GetMethod("Suspend", all)).ToArray();
        if (!suspend.Any(m => m.Name == "Release") || !suspend.Any(m => m.Name == "SetActive"))
            throw new Exception("Unused controls must be inactive and release allocation.");
        foreach (string family in new[] { "FishermansFlyingSail", "FishermansStaysail" })
        {
            var type = assembly.GetType($"FishermansSail.Sails.{family}.{family}Rigging", true);
            if (
                !CalledMethods(type.GetMethod("AttachControls", all))
                    .Any(m => m.DeclaringType == manager && m.Name == "Reconcile")
                || !CalledMethods(type.GetMethod("OnDestroy", all))
                    .Any(m => m.DeclaringType == owned && m.Name == "Dispose")
            )
                throw new Exception(family + " bypasses shared control ownership.");
        }
        var stay = assembly.GetType("FishermansSail.Stays.FishermansStay.FishermansStay", true);
        if (
            !CalledMethods(stay.GetMethod("CloneWinches", all))
                .Any(m => m.DeclaringType == manager && m.Name == "Create")
        )
            throw new Exception("Stay-owned vanilla controls bypass the shared clone factory.");
        Console.WriteLine(
            "PASS: structural winch checks for common clone setup, mounting/input separation and all three ownership paths; Unity activation, handles and outlines are not executed."
        );
    }
}
