using System;
using System.Collections.Generic;
using System.IO;
using RiftCommon;
using Shared.RhythmEngine;
using Shared.SceneLoading.Payloads;
using Shared.TrackData;
using UnityEngine;

namespace RiftPracticePlus;

public class ChartCaptureManager : MonoBehaviour {
    private BeatmapPlayer beatmapPlayer;
    private Beatmap beatmap;
    private BeatData beatData;
    private List<Hit> hits = new();

    public void Init(BeatmapPlayer beatmapPlayer) {
        this.beatmapPlayer = beatmapPlayer;
        beatmap = null;
        beatData = default;
        hits.Clear();
    }

    public void CaptureHit(float beat, float endBeat, EnemyType enemyType, int column, bool facingLeft, int score) {
        if (!EnsureBeatmap())
            return;

        double time = beatData.GetTimeFromBeat(beat);
        double endTime = beatData.GetTimeFromBeat(endBeat);

        hits.Add(new Hit(time, beat, endTime, endBeat, enemyType, column, facingLeft, score, false));
    }

    public void CaptureVibeGained(float beat) {
        if (!EnsureBeatmap())
            return;

        double time = beatData.GetTimeFromBeat(beat);

        hits.Add(new Hit(time, beat, time, beat, EnemyType.None, 0, false, 0, true));
    }

    public void Complete(RhythmRiftScenePayload payload, int actualMaxBaseScore, int actualMaxCombo) {
        var hitsArray = hits.ToArray();

        Array.Sort(hitsArray);

        int maxBaseScore = Util.ComputeMaxBaseScore(hitsArray);

        if (maxBaseScore != actualMaxBaseScore) {
            Plugin.Logger.LogWarning($"Score discrepancy found. Computed score is {maxBaseScore}. Actual score is {actualMaxBaseScore}");
            maxBaseScore = actualMaxBaseScore;
        }

        int maxCombo = Util.ComputeMaxCombo(hitsArray);

        if (maxCombo != actualMaxCombo) {
            Plugin.Logger.LogWarning($"Combo discrepancy found. Computed combo is {maxCombo}. Actual combo is {actualMaxCombo}");
            maxCombo = actualMaxCombo;
        }

        var vibeData = Solver.Solve(new SolverData(beatData, hitsArray));
        var chartData = new ChartData(
            payload.TrackName,
            payload.GetLevelId(),
            (Difficulty) payload.TrackDifficulty.Difficulty,
            payload.TrackDifficulty.Intensity ?? 0f,
            payload.TrackMetadata.Category.IsUgc(),
            maxBaseScore,
            maxCombo,
            beatData,
            hitsArray,
            vibeData);

        string path = Util.GetChartDataPath(payload.TrackMetadata, payload.TrackDifficulty.Difficulty);

        Directory.CreateDirectory(Path.GetDirectoryName(path));
        chartData.SaveToFile(path);
        Plugin.Logger.LogInfo($"Max base score is {maxBaseScore}");
        Plugin.Logger.LogInfo($"Max combo is {maxCombo}");
        Plugin.Logger.LogInfo($"Saved chart data to {path}");
        beatmap = null;
        beatData = default;
        hits.Clear();
    }

    private bool EnsureBeatmap() {
        if (beatmap != null)
            return true;

        beatmap = beatmapPlayer._activeBeatmap;

        if (beatmap == null)
            return false;

        int bpmChangeCount = 0;

        foreach (var beatmapEvent in beatmap.BeatmapEvents) {
            if (beatmapEvent.type == "AdjustBPM")
                bpmChangeCount++;
        }

        var bpmChanges = new BPMChange[bpmChangeCount];

        beatData = new BeatData(beatmap.bpm, beatmap.beatDivisions, bpmChanges, beatmap.BeatTimings.ToArray());

        int index = 0;

        foreach (var beatmapEvent in beatmap.BeatmapEvents) {
            if (beatmapEvent.type != "AdjustBPM")
                continue;

            double beat = beatmapEvent.startBeatNumber;

            bpmChanges[index] = new BPMChange(beatData.GetTimeFromBeat(beat), beat, beatmapEvent.GetFirstEventDataAsFloat("Bpm") ?? 0f);
            index++;
        }

        return true;
    }
}