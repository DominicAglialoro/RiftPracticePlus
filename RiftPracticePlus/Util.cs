﻿using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RiftCommon;
using Shared.SceneLoading.Payloads;
using Shared.TrackData;

namespace RiftPracticePlus;

internal static class Util {
    private static readonly BindingFlags ALL_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    public static Hook CreateMethodHook(this Type type, string name, Delegate method)
        => new(type.GetMethod(name, ALL_FLAGS), method);

    public static ILHook CreateILHook(this Type type, string name, ILContext.Manipulator manipulator)
        => new(type.GetMethod(name, ALL_FLAGS), manipulator);

    public static void EmitCall(this ILCursor cursor, Delegate d) => cursor.Emit(OpCodes.Call, d.Method);

    public static EnemyType EnemyIdToType(int id) => id switch {
        1722 => EnemyType.SlimeGreen,
        4355 => EnemyType.SlimeBlue,
        9189 => EnemyType.SlimeYellow,
        8675309 => EnemyType.BatBlue,
        717 => EnemyType.BatYellow,
        911 => EnemyType.BatRed,
        1234 => EnemyType.ZombieGreen,
        1235 => EnemyType.ZombieBlue,
        1236 => EnemyType.ZombieRed,
        2202 => EnemyType.SkeletonWhite,
        1911 => EnemyType.SkeletonWhiteShield,
        6471 => EnemyType.SkeletonWhiteDoubleShield,
        6803 => EnemyType.SkeletonYellow,
        4871 => EnemyType.SkeletonYellowShield,
        2716 => EnemyType.SkeletonBlack,
        3307 => EnemyType.SkeletonBlackShield,
        7831 => EnemyType.ArmadilloBlue,
        1707 => EnemyType.ArmadilloRed,
        6311 => EnemyType.ArmadilloYellow,
        7794 => EnemyType.Wyrm,
        8079 => EnemyType.Wyrm,
        9888 => EnemyType.Wyrm,
        8519 => EnemyType.HarpyGreen,
        8156 => EnemyType.HarpyBlue,
        3826 => EnemyType.HarpyRed,
        929 => EnemyType.Blademaster,
        3685 => EnemyType.BlademasterBlue,
        7288 => EnemyType.BlademasterYellow,
        4601 => EnemyType.SkullWhite,
        3543 => EnemyType.SkullBlue,
        7685 => EnemyType.SkullRed,
        7358 => EnemyType.Apple,
        2054 => EnemyType.Cheese,
        1817 => EnemyType.Drumstick,
        3211 => EnemyType.Ham,
        _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
    };

    public static string GetChartDataPath(RhythmRiftScenePayload payload) {
        string directory;

        if (payload.TrackMetadata.Category.IsUgc())
            directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RiftPracticePlus");
        else
            directory = Path.Combine(Plugin.AssemblyPath, "ChartData");

        return Path.Combine(directory, $"{GetFileNameFromPayload(payload)}.bin");
    }

    private static string GetFileNameFromPayload(RhythmRiftScenePayload payload) => Regex.Replace($"{payload.TrackMetadata.TrackName}_{payload.GetLevelId()}_{payload.TrackDifficulty.Difficulty}", @"[^\w\s`~!@#$%^&()-=+\[\]\{\};',]", "-", RegexOptions.Compiled);
}