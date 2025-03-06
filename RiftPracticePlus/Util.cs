using System;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RiftEventCapture.Common;

namespace RiftPracticePlus;

internal static class Util {
    private static readonly BindingFlags ALL_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    public static Hook CreateMethodHook(this Type type, string name, Delegate method)
        => new(type.GetMethod(name, ALL_FLAGS), method);

    public static ILHook CreateILHook(this Type type, string name, ILContext.Manipulator manipulator)
        => new(type.GetMethod(name, ALL_FLAGS), manipulator);

    public static void EmitCall(this ILCursor cursor, Delegate d) => cursor.Emit(OpCodes.Call, d.Method);

    public static Difficulty GameDifficultyToCommonDifficulty(Shared.Difficulty difficulty) => difficulty switch {
        Shared.Difficulty.Easy => Difficulty.Easy,
        Shared.Difficulty.Medium => Difficulty.Medium,
        Shared.Difficulty.Hard => Difficulty.Hard,
        Shared.Difficulty.Impossible => Difficulty.Impossible,
        _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null)
    };
}