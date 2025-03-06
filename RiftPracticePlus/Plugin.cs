using System;
using System.Collections;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using RhythmRift;
using RiftEventCapture.Common;
using Shared.Pins;
using Shared.SceneLoading.Payloads;
using UnityEngine;

namespace RiftPracticePlus;

[BepInPlugin("programmatic.riftPracticePlus", "RiftPracticePlus", "1.0.0.0")]
public class Plugin : BaseUnityPlugin {
    public new static ManualLogSource Logger { get; private set; }

    private static ChartRenderer chartRenderer = new(200, 800);

    private void Awake() {
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
        var practicePlusWindow = FindObjectOfType<PracticePlusWindow>();

        if (practicePlusWindow == null)
            practicePlusWindow = new GameObject("Practice Plus Window", typeof(PracticePlusWindow)).GetComponent<PracticePlusWindow>();

        practicePlusWindow.Init(rrStageController.BeatmapPlayer, new RenderData(captureResult), chartRenderer);
        Logger.LogInfo($"Begin playing {rrStageController._stageFlowUiController._stageContextInfo.StageDisplayName}");

        return playStageIntro(rrStageController);
    }
}