using System.IO;
using System.Linq;
using System.Reflection;

namespace FishermansSail.Tests.AssemblyChecks.Shared;

internal static class IlReader
{
    // Decode call operands without asking Harmony to create native patch stubs.
    internal static System.Collections.Generic.IEnumerable<MethodBase> CalledMethods(
        MethodInfo method
    )
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
}
