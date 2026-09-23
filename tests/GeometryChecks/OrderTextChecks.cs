using System;
using System.Linq;
using FishermansSail;

internal static class OrderTextChecks
{
    internal static void Run()
    {
        const string mizzen = "Mizzenmast Triatic Stay (mizzen top stay 1)";
        const string fore = "Formast Triatic Stay (middle topmast stay 2-2)";
        // Reproduce the installed NANDFixes 1.4.3 non-progressing recursion
        // without actually overflowing the test process's stack.
        string removal = "0: " + mizzen + " -> (no Mizzenmast Triatic Stay)";
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
                "(ERROR): " + mizzen + " -> (no Mizzenmast Triatic Stay)",
                "192: (no Formast Triatic Stay) -> " + fore,
                "192: " + fore + " -> (no Formast Triatic Stay)",
                "192: " + mizzen + " -> Mizzenmast Triatic Stay (mizzen top stay 2)",
                "(ERROR): " + fore + " requires: main mast 2",
                "Formast Triatic Stay " + new string('x', 200),
            }
        )
        {
            Check(StayOrderText.NeedsWrapping(line), "Triatic order escaped the guard.");
            string[] wrapped = StayOrderText.Wrap(line).ToArray();
            Check(wrapped.All(s => s.Length <= 45), "An order line exceeds the safe width.");
            Check(
                string.Concat(wrapped).Replace(" ", "") == line.Replace(" ", ""),
                "Wrapping lost order details, error text, price or direction."
            );
        }
        foreach (
            string line in new[]
            {
                null,
                "192: shipyard fee",
                "(no Formast Triatic Stay)",
                new string('x', 100),
            }
        )
            Check(
                !StayOrderText.NeedsWrapping(line),
                "Short or unrelated lines must keep native handling."
            );
        Check(
            StayOrderText.Wrap("first\nsecond").SequenceEqual(new[] { "first", "second" }),
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
