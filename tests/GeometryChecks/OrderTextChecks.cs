using System;
using System.Linq;
using FishermansSail;

internal static class OrderTextChecks
{
    internal static void Run()
    {
        const string large = "Fisherman's Sail Prototype (150% x 115%)";
        const string small = "Fisherman's Sail Prototype (65%)";
        // Reproduce the installed NANDFixes 1.4.3 non-progressing recursion
        // without actually overflowing the test process's stack.
        string removal = "0: " + large + " -> (no sail)";
        string recursive = removal.Substring(
            0,
            removal.IndexOf("->", StringComparison.Ordinal) + 2
        );
        Check(
            recursive.Length > 45
                && recursive.Substring(0, recursive.IndexOf("->", StringComparison.Ordinal) + 2)
                    == recursive,
            "The reported removal no longer reproduces the recursive wrapper's input."
        );
        foreach (
            string line in new[]
            {
                removal,
                "(ERROR): " + large + " -> (no sail)",
                "192: (no sail) -> " + small,
                "192: " + small + " -> (no sail)",
                "192: " + large + " -> " + small,
                "(ERROR): " + small + " requires: main mast 2",
                "Fisherman's Sail " + new string('x', 200),
                "Fisherman's Sail Prototype (REQUIRES AN ACTIVE AFT MAST WITH HALYARD GUIDES)",
            }
        )
        {
            Check(FishermanOrderText.NeedsWrapping(line), "Fisherman order escaped the guard.");
            string[] wrapped = FishermanOrderText.Wrap(line).ToArray();
            Check(wrapped.All(s => s.Length <= 45), "An order line exceeds the safe width.");
            Check(
                string.Concat(wrapped).Replace(" ", "") == line.Replace(" ", ""),
                "Wrapping lost order details, error text, price or direction."
            );
        }
        foreach (
            string line in new[] { null, "192: shipyard fee", "(no sail)", new string('x', 100) }
        )
            Check(
                !FishermanOrderText.NeedsWrapping(line),
                "Short or unrelated lines must keep native handling."
            );
        Check(
            FishermanOrderText.Wrap("first\nsecond").SequenceEqual(new[] { "first", "second" }),
            "Explicit line breaks were lost."
        );
        Console.WriteLine(
            "PASS: recursive order-text reproducer; removal, replacement, errors and bounded lossless wrapping."
        );
    }

    private static void Check(bool value, string message)
    {
        if (!value)
            throw new Exception(message);
    }
}
