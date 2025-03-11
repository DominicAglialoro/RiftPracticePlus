using System;

namespace RiftPracticePlus;

public class Activation : IComparable<Activation> {
    public readonly double MinStartTime;
    public readonly double MaxStartTime;
    public readonly int StartIndex;
    public readonly int EndIndex;
    public readonly int Score;
    public readonly int VibesUsed;

    public Activation(double minStartTime, double maxStartTime, int startIndex, int endIndex, int score, int vibesUsed) {
        MinStartTime = minStartTime;
        MaxStartTime = maxStartTime;
        Score = score;
        VibesUsed = vibesUsed;
        StartIndex = startIndex;
        EndIndex = endIndex;
    }

    public int CompareTo(Activation other) {
        int timeComparison = MinStartTime.CompareTo(other.MinStartTime);

        if (timeComparison != 0)
            return timeComparison;

        return VibesUsed.CompareTo(other.VibesUsed);
    }
}