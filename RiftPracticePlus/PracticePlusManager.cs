using System;
using System.IO;
using RhythmRift;
using RiftEventCapture.Common;
using Shared;
using Shared.RhythmEngine;
using UnityEngine;
using Difficulty = Shared.Difficulty;

namespace RiftPracticePlus;

public class PracticePlusManager : MonoBehaviour {
    private const float HIDE_CURSOR_AFTER_TIME = 2f;

    private string currentChartName = string.Empty;
    private Difficulty currentChartDifficulty = Difficulty.None;
    private BeatmapPlayer beatmapPlayer;
    private ChartRenderData chartRenderData;
    private PracticePlusWindow practicePlusWindow;
    private float mouseMovedAt;
    private int firstBeatIndex = 1;
    private int firstNoteIndex;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.P)) {
            Plugin.ShowPracticePlusWindow.Value = !Plugin.ShowPracticePlusWindow.Value;
            Cursor.visible = Plugin.ShowPracticePlusWindow.Value;
            mouseMovedAt = Time.time;
        }

        if (!Plugin.ShowPracticePlusWindow.Value || !GameWindow.Instance._isFocused)
            return;

        if (Input.GetAxis("Mouse X") != 0f || Input.GetAxis("Mouse Y") != 0f) {
            Cursor.visible = true;
            mouseMovedAt = Time.time;
        }
        else if (Cursor.visible && Time.time > mouseMovedAt + HIDE_CURSOR_AFTER_TIME)
            Cursor.visible = false;
    }

    private void OnGUI() {
        float time = (float) (beatmapPlayer._activeSpeedAdjustment * beatmapPlayer.FmodTimeCapsule.Time + beatmapPlayer._musicInitialSkippedTimeInSeconds);
        var beatData = chartRenderData.BeatData;
        var notes = chartRenderData.Notes;

        while (beatData.GetTimeFromBeat(firstBeatIndex) < time - 0.1f)
            firstBeatIndex++;

        while (firstNoteIndex < notes.Count && notes[firstNoteIndex].EndTime < time)
            firstNoteIndex++;

        if (Plugin.ShowPracticePlusWindow.Value)
            practicePlusWindow.Render(new ChartRenderParams(time, firstBeatIndex, firstNoteIndex, chartRenderData));
    }

    public void Init(RRStageController rrStageController, PracticePlusWindow practicePlusWindow) {
        beatmapPlayer = rrStageController.BeatmapPlayer;
        this.practicePlusWindow = practicePlusWindow;
        firstBeatIndex = 1;
        firstNoteIndex = 0;
        enabled = true;

        for (int i = 1; i <= practicePlusWindow.StartingVibe; i++) {
            rrStageController._currentVibePower = Math.Min(i * rrStageController._vibeChainCompletePowerAdditive, rrStageController._maxVibePower);
            rrStageController.UpdateUI();
        }

        var stageContextInfo = rrStageController._stageFlowUiController._stageContextInfo;
        string chartName = stageContextInfo.StageDisplayName;
        var chartDifficulty = stageContextInfo.StageDifficulty;

        if (chartName == currentChartName && chartDifficulty == currentChartDifficulty)
            return;

        currentChartName = chartName;
        currentChartDifficulty = chartDifficulty;

        string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RiftEventCapture", "GoldenLute");

        if (!Directory.Exists(directory)) {
            Plugin.Logger.LogInfo("No event capture directory found");
            chartRenderData = null;

            return;
        }

        string path = Path.Combine(directory, $"{stageContextInfo.StageDisplayName}_{stageContextInfo.StageDifficulty}.bin");

        if (!File.Exists(path)) {
            Plugin.Logger.LogInfo("No event capture found for this chart");
            chartRenderData = null;

            return;
        }

        chartRenderData = ChartRenderData.CreateFromCaptureResult(CaptureResult.LoadFromFile(path));
    }
}