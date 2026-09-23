using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using FishermansSail.Tests.AssemblyChecks.Shared;
using FlyingSailChecks = FishermansSail.Tests.AssemblyChecks.FishermansFlyingSail.PatchChecks;
using StayChecks = FishermansSail.Tests.AssemblyChecks.FishermansStay.PatchChecks;

namespace FishermansSail.Tests.AssemblyChecks;

internal static class Program
{
    private static void Main(string[] args)
    {
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
        var assembly = Assembly.LoadFrom(
            Path.Combine(AppContext.BaseDirectory, "FishermansSail.dll")
        );

        HarmonySignatureChecks.Run(assembly);
        FlyingSailChecks.Run(assembly);
        StayChecks.Run(assembly);
    }
}
