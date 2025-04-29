using System;
using System.Collections;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RhythmRift;
using RhythmRift.Enemies;
using RiftCommon;
using Shared;
using Shared.Pins;
using Shared.RhythmEngine;
using Shared.SceneLoading.Payloads;
using Shared.TrackData;
using UnityEngine;

namespace RiftPracticePlus;

[BepInPlugin("programmatic.riftPracticePlus", "RiftPracticePlus", PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin {
    public const string PLUGIN_VERSION = "1.4.1";

    private const int WINDOW_WIDTH = 200;
    private const int WINDOW_HEIGHT = 800;

    public static ConfigEntry<bool> ShowPracticePlusWindow { get; private set; }
    public static ConfigEntry<int> WindowPositionX { get; private set; }
    public static ConfigEntry<int> WindowPositionY { get; private set; }

    public static string AssemblyPath { get; } = Path.GetDirectoryName(typeof(Plugin).Assembly.Location);
    public new static ManualLogSource Logger { get; private set; }

    private static PracticePlusWindow practicePlusWindow;
    private static ChartCaptureManager chartCaptureManager;

    private void Awake() {
        ShowPracticePlusWindow = Config.Bind("General", "ShowPracticePlusWindow", true, "Whether or not to show the Practice Plus window when loading a chart in practice mode. Can be toggled by pressing P");
        WindowPositionX = Config.Bind("General", "Window Position X", 0, "The X position of the Practice Plus window");
        WindowPositionY = Config.Bind("General", "Window Position Y", 0, "The Y position of the Practice Plus window");
        practicePlusWindow = new PracticePlusWindow(WindowPositionX.Value, WindowPositionY.Value, WINDOW_WIDTH, WINDOW_HEIGHT);
        Logger = base.Logger;
        Logger.LogInfo("Loaded RiftPracticePlus");

        typeof(RRStageController).CreateMethodHook(nameof(RRStageController.PlayStageIntro), RRStageController_PlayStageIntro);
        typeof(RRStageController).CreateMethodHook(nameof(RRStageController.ShowResultsScreen), RRStageController_ShowResultsScreen);
        typeof(RRStageController).CreateILHook(nameof(RRStageController.ProcessHitData), RRStageController_ProcessHitData_IL);
        typeof(RRStageController).CreateILHook(nameof(RRStageController.HandleKilledBoundEnemy), RRStageController_HandleKilledBoundEnemy_IL);
    }

    private static void OnRecordInput(RRStageController rrStageController, RREnemyController.EnemyHitData hitData, int inputScore, int perfectBonusScore) {
        if (!RiftUtilityHelper.IsInputSuccess(hitData.InputRating) || chartCaptureManager == null)
            return;

        var stageInputRecord = rrStageController._stageInputRecord;
        int baseMultiplier = stageInputRecord._stageScoringDefinition.GetComboMultiplier(stageInputRecord.CurrentComboCount);
        var enemy = hitData.Enemy;
        var enemyType = Util.EnemyIdToType(enemy.EnemyTypeId);
        float endBeat = hitData.TargetBeat;

        if (enemyType == EnemyType.Wyrm)
            endBeat += Math.Max(2, enemy.EnemyLength) - 1;

        chartCaptureManager.CaptureHit(hitData.TargetBeat, endBeat, enemyType, enemy.CurrentGridPosition.x, inputScore * baseMultiplier);
    }

    private static void OnVibeChainSuccessFromHit(RRStageController rrStageController, RREnemyController.EnemyHitData hitData)
        => OnVibeChainSuccess(hitData.TargetBeat);

    private static void OnVibeChainSuccessFromWyrmKill(RRStageController rrStageController, IRREnemyDataAccessor boundEnemy) {
        var enemy = (RREnemy) boundEnemy;
        float beat = enemy.TargetHitBeatNumber + Math.Max(0, enemy.EnemyLength - 1);

        OnVibeChainSuccess(beat);
    }

    private static void OnVibeChainSuccess(float targetBeat) {
        if (chartCaptureManager != null)
            chartCaptureManager.CaptureVibeGained(targetBeat);
    }

    private static IEnumerator RRStageController_PlayStageIntro(Func<RRStageController, IEnumerator> playStageIntro, RRStageController rrStageController) {
        chartCaptureManager = null;

        if (rrStageController._stageScenePayload is not RhythmRiftScenePayload payload || payload.IsChallenge || payload.IsDailyChallenge || payload.IsMicroRift || payload.IsStoryMode || payload.IsTutorial)
            return playStageIntro(rrStageController);

        if (payload.IsPracticeMode) {
            var practicePlusManager = FindObjectOfType<PracticePlusManager>();

            if (practicePlusManager == null)
                practicePlusManager = new GameObject("Practice Plus Manager", typeof(PracticePlusManager)).GetComponent<PracticePlusManager>();

            practicePlusManager.Init(rrStageController, payload, practicePlusWindow);
        }
        else if (PinsController.IsPinActive("GoldenLute") && payload.TrackMetadata.Category.IsUgc()) {
            Logger.LogInfo($"Begin capturing Name: {payload.TrackMetadata.TrackName}, ID: {payload.GetLevelId()}, Difficulty: {payload.TrackDifficulty.Difficulty}");
            chartCaptureManager = FindObjectOfType<ChartCaptureManager>();

            if (chartCaptureManager == null)
                chartCaptureManager = new GameObject("Chart Capture Manager", typeof(ChartCaptureManager)).GetComponent<ChartCaptureManager>();

            chartCaptureManager.Init(rrStageController.BeatmapPlayer);
        }

        return playStageIntro(rrStageController);
    }

    private static void RRStageController_ShowResultsScreen(Action<RRStageController, bool, float, int, bool, bool> showResultsScreen,
        RRStageController rrStageController, bool isNewHighScore, float trackProgressPercentage, int awardedDiamonds = 0,
        bool didNotFinish = false, bool cheatsDetected = false) {
        showResultsScreen(rrStageController, isNewHighScore, trackProgressPercentage, awardedDiamonds, didNotFinish, cheatsDetected);

        if (chartCaptureManager != null && !didNotFinish && rrStageController._stageScenePayload is RhythmRiftScenePayload payload && !payload.IsChallenge && !payload.IsDailyChallenge && !payload.IsMicroRift && !payload.IsStoryMode && !payload.IsTutorial && !payload.IsPracticeMode)
            chartCaptureManager.Complete(payload);

        chartCaptureManager = null;
    }

    private static void RRStageController_ProcessHitData_IL(ILContext il) {
        var cursor = new ILCursor(il);

        cursor.GotoNext(MoveType.After, instr => instr.MatchStloc(8));

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldloc_S, (byte) 6);
        cursor.Emit(OpCodes.Ldloc_S, (byte) 8);
        cursor.Emit(OpCodes.Ldloc_S, (byte) 7);
        cursor.EmitCall(OnRecordInput);

        cursor.GotoNext(MoveType.After, instr => instr.MatchCall<RRStageController>(nameof(RRStageController.VibeChainSuccess)));

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldloc_S, (byte) 22);
        cursor.EmitCall(OnVibeChainSuccessFromHit);
    }

    private static void RRStageController_HandleKilledBoundEnemy_IL(ILContext il) {
        var cursor = new ILCursor(il);

        cursor.GotoNext(MoveType.After, instr => instr.MatchCall<RRStageController>(nameof(RRStageController.VibeChainSuccess)));

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldarg_1);
        cursor.EmitCall(OnVibeChainSuccessFromWyrmKill);
    }
}