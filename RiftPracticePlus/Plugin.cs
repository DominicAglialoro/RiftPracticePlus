using System;
using System.Collections;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using RhythmRift;
using RiftEventCapture.Common;
using Shared.Pins;
using Shared.SceneLoading.Payloads;
using UnityEngine;

namespace RiftPracticePlus;

[BepInPlugin("programmatic.riftPracticePlus", "RiftPracticePlus", "1.1.0.0")]
public class Plugin : BaseUnityPlugin {
    private const int WINDOW_WIDTH = 200;
    private const int WINDOW_HEIGHT = 800;

    public static ConfigEntry<bool> ShowPracticePlusWindow { get; private set; }
    public static ConfigEntry<int> WindowPositionX { get; private set; }
    public static ConfigEntry<int> WindowPositionY { get; private set; }

    public new static ManualLogSource Logger { get; private set; }

    private static PracticePlusWindow practicePlusWindow;

    private void Awake() {
        ShowPracticePlusWindow = Config.Bind("General", "ShowPracticePlusWindow", true, "Whether or not to show the Practice Plus window when loading a chart in practice mode. Can be toggled by pressing P");
        WindowPositionX = Config.Bind("General", "Window Position X", 0, "The X position of the Practice Plus window");
        WindowPositionY = Config.Bind("General", "Window Position Y", 0, "The Y position of the Practice Plus window");
        practicePlusWindow = new PracticePlusWindow(WindowPositionX.Value, WindowPositionY.Value, WINDOW_WIDTH, WINDOW_HEIGHT);
        Logger = base.Logger;
        Logger.LogInfo("Loaded RiftPracticePlus");

        typeof(RRStageController).CreateMethodHook(nameof(RRStageController.PlayStageIntro), RRStageController_PlayStageIntro);
    }

    private static IEnumerator RRStageController_PlayStageIntro(Func<RRStageController, IEnumerator> playStageIntro, RRStageController rrStageController) {
        if (rrStageController._stageScenePayload is not RhythmRiftScenePayload payload || !payload.IsPracticeMode)
            return playStageIntro(rrStageController);

        var stageContextInfo = rrStageController._stageFlowUiController._stageContextInfo;

        var sessionInfo = new SessionInfo(
            stageContextInfo.StageDisplayName,
            payload.GetLevelId(),
            Util.GameDifficultyToCommonDifficulty(stageContextInfo.StageDifficulty),
            (string[]) PinsController.GetActivePins().Clone());

        string name = $"{sessionInfo.ChartName}_{sessionInfo.ChartDifficulty}";
        string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RiftEventCapture", "GoldenLute");

        if (!Directory.Exists(directory)) {
            Logger.LogInfo("No event capture directory found");

            return playStageIntro(rrStageController);
        }

        string path = Path.Combine(directory, $"{name}.bin");

        if (!File.Exists(path)) {
            Logger.LogInfo("No event capture found for this chart");

            return playStageIntro(rrStageController);
        }

        var captureResult = CaptureResult.LoadFromFile(path);
        var practicePlusManager = FindObjectOfType<PracticePlusManager>();

        if (practicePlusManager == null)
            practicePlusManager = new GameObject("Practice Plus Manager", typeof(PracticePlusManager)).GetComponent<PracticePlusManager>();

        practicePlusManager.Init(rrStageController.BeatmapPlayer, new ChartRenderData(captureResult), practicePlusWindow);

        for (int i = 1; i <= practicePlusWindow.StartingVibe; i++) {
            rrStageController._currentVibePower = Math.Min(i * rrStageController._vibeChainCompletePowerAdditive, rrStageController._maxVibePower);
            rrStageController.UpdateUI();
        }

        Logger.LogInfo($"Begin playing {rrStageController._stageFlowUiController._stageContextInfo.StageDisplayName}");

        return playStageIntro(rrStageController);
    }
}