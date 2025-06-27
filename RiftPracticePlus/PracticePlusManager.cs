using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using RhythmRift;
using RiftCommon;
using Shared;
using Shared.RhythmEngine;
using Shared.SceneLoading.Payloads;
using UnityEngine;

namespace RiftPracticePlus;

public class PracticePlusManager : MonoBehaviour {
    private const float HIDE_CURSOR_AFTER_TIME = 2f;

    private static bool TryGetChartData(string path, out ChartData chartData) {
        if (!File.Exists(path)) {
            Plugin.Logger.LogInfo("No chart data found for this chart");
            chartData = null;

            return false;
        }

        chartData = ChartData.LoadFromFile(path);

        return true;
    }

    private string currentChartDataPath;
    private BeatmapPlayer beatmapPlayer;
    private ChartRenderData chartRenderData;
    private PracticePlusWindow practicePlusWindow;
    private float mouseMovedAt;
    private float skipTime;
    private int firstBeatIndex = 1;
    private int firstHitIndex;

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
        float time = (float) (beatmapPlayer._activeSpeedAdjustment * beatmapPlayer.FmodTimeCapsule.Time + skipTime);

        if (chartRenderData != null) {
            var beatData = chartRenderData.BeatData;

            while (beatData.GetTimeFromBeat(firstBeatIndex) < time - 0.1f)
                firstBeatIndex++;

            var hits = chartRenderData.Hits;

            while (firstHitIndex < hits.Length && hits[firstHitIndex].EndTime < time)
                firstHitIndex++;
        }

        if (Plugin.ShowPracticePlusWindow.Value)
            practicePlusWindow.Render(new ChartRenderParams(time, firstBeatIndex, firstHitIndex, chartRenderData));
    }

    public void Init(RRStageController rrStageController, RhythmRiftScenePayload payload, PracticePlusWindow practicePlusWindow) {
        beatmapPlayer = rrStageController.BeatmapPlayer;
        this.practicePlusWindow = practicePlusWindow;
        practicePlusWindow.OpenVisualizerClicked = OpenVisualizerClicked;
        skipTime = Mathf.Max(0f, rrStageController.ComputeSkipTime(
            rrStageController._practiceModeStartBeatmapIndex,
            rrStageController._practiceModeStartBeatNumber,
            rrStageController._practiceModeTotalBeatsSkippedBeforeStartBeatmap));
        firstBeatIndex = 1;
        firstHitIndex = 0;
        enabled = true;

        for (int i = 1; i <= practicePlusWindow.StartingVibe; i++) {
            rrStageController._currentVibePower = Math.Min(i * rrStageController._vibeChainCompletePowerAdditive, rrStageController._maxVibePower);
            rrStageController.UpdateUI();
        }

        string chartDataPath = Util.GetChartDataPath(payload.TrackMetadata, payload.TrackDifficulty.Difficulty);

        if (chartDataPath == currentChartDataPath)
            return;

        currentChartDataPath = chartDataPath;

        if (!TryGetChartData(chartDataPath, out var chartData)) {
            chartRenderData = null;

            return;
        }

        var hits = new List<ChartRenderHit>();

        foreach (var hit in chartData.Hits) {
            if (hit.EnemyType != EnemyType.None)
                hits.Add(new ChartRenderHit((float) hit.Time, (float) hit.EndTime, hit.Column));
        }

        hits.Sort();

        var activations = new List<ChartRenderActivation>();

        foreach (var activation in chartData.VibeData.SingleVibeActivations) {
            if (activation.IsOptimal)
                activations.Add(new ChartRenderActivation((float) activation.MinStartTime, (float) activation.MaxStartTime, false));
        }

        foreach (var activation in chartData.VibeData.DoubleVibeActivations) {
            if (activation.IsOptimal)
                activations.Add(new ChartRenderActivation((float) activation.MinStartTime, (float) activation.MaxStartTime, true));
        }

        activations.Sort();
        chartRenderData = new ChartRenderData(chartData.BeatData, hits.ToArray(), activations.ToArray());
    }

    private void OpenVisualizerClicked() {
        if (string.IsNullOrWhiteSpace(currentChartDataPath))
            return;

        if (!File.Exists(currentChartDataPath)) {
            Plugin.Logger.LogInfo("No chart data found for this chart");

            return;
        }

        string executable = Path.Combine(Plugin.AssemblyPath, "RiftPracticePlus.Visualizer.exe");

        if (!File.Exists(executable)) {
            Plugin.Logger.LogWarning("Could not find visualizer executable");

            return;
        }

        Process.Start(executable, $"\"{currentChartDataPath}\"");
    }
}