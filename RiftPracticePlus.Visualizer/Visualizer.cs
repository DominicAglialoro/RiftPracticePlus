using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using RiftCommon;

namespace RiftPracticePlus.Visualizer;

public class Visualizer {
    private const double VIBE_LENGTH = 5d;
    private static readonly Brush ONE_VIBE_BRUSH = new SolidBrush(Color.FromArgb(96, 160, 0));
    private static readonly Brush TWO_VIBE_BRUSH = new SolidBrush(Color.FromArgb(0, 128, 128));

    private readonly GraphicsPanel panel;
    private readonly List<Drawable> vibePathDrawables = new();

    public VibePath CurrentPath { get; private set; }

    private readonly OpenFileDialog openFileDialog;
    private readonly Label currentSpanLabel = new(0f, 20f, "");

    private ChartData data;

    public Visualizer(GraphicsPanel panel) {
        this.panel = panel;
        openFileDialog = new OpenFileDialog();
        panel.OnClick += (time, value) => DrawVibePath(time, value > 0.5f ? 1 : 2);
        panel.OnEnter += ShowFileDialog;
    }

    public void Start() {
        string[] args = Environment.GetCommandLineArgs();

        if (args.Length < 2)
            ShowFileDialog();
        else
            LoadEvents(args[1]);
    }

    private void ShowFileDialog() {
        if (openFileDialog.ShowDialog() == DialogResult.OK)
            LoadEvents(openFileDialog.FileName);
    }

    private void DrawVibePath(double time, int vibesUsed) {
        if (data == null)
            return;

        var path = GetVibePath(data, time, vibesUsed);

        if (path == null)
            return;

        foreach (var drawable in vibePathDrawables)
            panel.RemoveDrawable(drawable);

        vibePathDrawables.Clear();
        CurrentPath = path;

        currentSpanLabel.SetLabel($"Beat {data.BeatData.GetBeatFromTime(CurrentPath.StartTime):F}, {CurrentPath.Score} points");

        var points = new List<PointD>();

        foreach (var segment in CurrentPath.Segments) {
            points.Add(new PointD(segment.StartTime, segment.StartVibe));
            points.Add(new PointD(segment.EndTime, segment.EndVibe));
        }

        var graph = new LineGraph(0f, 1f, 10f, 0f, points);

        vibePathDrawables.Add(graph);
        panel.AddDrawable(graph);
        panel.Redraw();
    }

    private void LoadEvents(string path) {
        if (!File.Exists(path))
            return;

        data = ChartData.LoadFromFile(path);
        DrawEvents();
    }

    private void DrawEvents() {
        var beatData = data.BeatData;

        CurrentPath = null;
        currentSpanLabel.SetLabel("");
        vibePathDrawables.Clear();
        panel.Clear();
        panel.AddDrawable(currentSpanLabel);
        panel.AddDrawable(new BeatGrid(0f, 1f, 7, 4, 60d / beatData.BPM, beatData.BeatTimings));

        var hits = data.Hits;
        double currentTime = double.MinValue;
        int currentScore = 0;
        bool currentGivesVibe = false;

        foreach (var hit in hits) {
            if (hit.Time > currentTime) {
                if (currentScore > 0 || currentGivesVibe)
                    panel.AddDrawable(new HitMarker(currentTime, 1f - currentScore / 6660f, currentGivesVibe, this));

                currentTime = hit.Time;
                currentScore = 0;
                currentGivesVibe = false;
            }

            currentScore += hit.Score;
            currentGivesVibe |= hit.GivesVibe;
        }

        if (currentScore > 0 || currentGivesVibe)
            panel.AddDrawable(new HitMarker(currentTime, 1f - currentScore / 6660f, currentGivesVibe, this));

        var singleVibeActivations = data.VibeData.SingleVibeActivations;
        var doubleVibeActivations = data.VibeData.DoubleVibeActivations;

        if (singleVibeActivations.Length == 0 && doubleVibeActivations.Length == 0) {
            panel.Redraw();

            return;
        }

        int maxScore = 0;
        var pairs = new List<(PointD, double)>();
        var bestActivations = new List<Activation>();

        foreach (var activation in singleVibeActivations) {
            maxScore = Math.Max(maxScore, activation.Score);

            if (activation.IsOptimal)
                bestActivations.Add(activation);
        }

        foreach (var activation in doubleVibeActivations) {
            maxScore = Math.Max(maxScore, activation.Score);

            if (activation.IsOptimal)
                bestActivations.Add(activation);
        }

        bestActivations.Sort();

        foreach (var activation in bestActivations)
            pairs.Add((new PointD(activation.MinStartTime, activation.Score), activation.MaxStartTime));

        var points = new List<PointD>();

        foreach (var activation in doubleVibeActivations)
            points.Add(new PointD(activation.MinStartTime, activation.Score));

        panel.AddDrawable(new BarGraph(1f, 0f, 0f, maxScore, TWO_VIBE_BRUSH, points.ToArray()));
        points.Clear();

        foreach (var activation in singleVibeActivations)
            points.Add(new PointD(activation.MinStartTime, activation.Score));

        panel.AddDrawable(new BarGraph(1f, 0f, 0f, maxScore, ONE_VIBE_BRUSH, points.ToArray()));
        panel.AddDrawable(new Label(0f, 0f, $"Optimal bonus: {data.VibeData.MaxVibeBonus}"));

        panel.AddDrawable(new OptimalActivationMarkers(1f, 0f, 0f, maxScore, pairs));
        panel.Redraw();
    }

    private static VibePath GetVibePath(ChartData data, double startTime, int vibesUsed) {
        var hits = data.Hits;

        if (!data.VibeData.TryGetActivationAt(startTime, vibesUsed == 2, out var activation))
            return null;

        var segments = new List<VibePathSegment>();
        double currentTime = startTime;
        double vibeRemaining = vibesUsed * VIBE_LENGTH;
        int startIndex = 0;

        while (startIndex < hits.Length && hits[startIndex].Time < startTime)
            startIndex++;

        for (int i = startIndex; i < hits.Length; i++) {
            var hit = hits[i];

            if (hit.Time > activation.LastHitTime)
                break;

            if (!hit.GivesVibe)
                continue;

            segments.Add(new VibePathSegment(currentTime, hit.Time, vibeRemaining, Math.Max(0d, vibeRemaining - (hit.Time - currentTime))));
            vibeRemaining = Math.Max(VIBE_LENGTH, Math.Min(vibeRemaining - (hit.Time - currentTime) + VIBE_LENGTH, 2d * VIBE_LENGTH));
            currentTime = hit.Time;
        }

        segments.Add(new VibePathSegment(currentTime, currentTime + vibeRemaining, vibeRemaining, 0d));

        return new VibePath(startTime, activation.LastHitTime, activation.Score, segments);
    }
}