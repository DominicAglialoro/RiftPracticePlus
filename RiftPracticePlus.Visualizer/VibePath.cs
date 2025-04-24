using System.Collections.Generic;

namespace RiftPracticePlus.Visualizer;

public class VibePath {
    public double StartTime { get; }
    public double EndTime { get; }
    public int Score { get; }
    public IReadOnlyList<VibePathSegment> Segments { get; }

    public VibePath(double startTime, double endTime, int score, IReadOnlyList<VibePathSegment> segments) {
        StartTime = startTime;
        EndTime = endTime;
        Score = score;
        Segments = segments;
    }
}