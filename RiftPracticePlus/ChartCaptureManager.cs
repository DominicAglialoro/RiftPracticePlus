using System;
using System.Collections.Generic;
using System.IO;
using RiftCommon;
using Shared.RhythmEngine;
using Shared.SceneLoading.Payloads;
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

    public void CaptureHit(float beat, float endBeat, EnemyType enemyType, int column, int score) {
        if (!EnsureBeatmap())
            return;

        double time = beatData.GetTimeFromBeat(beat);
        double endTime = beatData.GetTimeFromBeat(endBeat);

        hits.Add(new Hit(time, beat, endTime, endBeat, enemyType, column, score, false));
    }

    public void CaptureVibeGained(float beat) {
        if (!EnsureBeatmap())
            return;

        double time = beatData.GetTimeFromBeat(beat);

        hits.Add(new Hit(time, beat, time, beat, EnemyType.None, 0, 0, true));
    }

    public void Complete(RhythmRiftScenePayload payload) {
        var hitsArray = hits.ToArray();

        Array.Sort(hitsArray);

        var vibeData = Solver.Solve(new SolverData(beatData, hitsArray));

        var chartData = new ChartData(
            payload.TrackName,
            payload.GetLevelId(),
            (Difficulty) payload.TrackDifficulty.Difficulty,
            !string.IsNullOrWhiteSpace(payload.TrackDifficulty.BeatmapFilePath),
            beatData,
            hitsArray,
            vibeData);

        string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RiftPracticePlus");

        Directory.CreateDirectory(directory);

        string path = Path.Combine(directory, $"{Util.CleanFileName(Util.GetIdentifierFromPayload(payload))}.bin");

        chartData.SaveToFile(path);
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

        beatData = new BeatData(beatmap.bpm, beatmap.beatDivisions, beatmap.BeatTimings.ToArray());

        return true;
    }
}