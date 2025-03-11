using System;

namespace RiftPracticePlus;

public class Activation : IComparable<Activation> {
    public double MaxStartTime;
    public int Score;
    public int MaxValue;
    public Activation[] BestNextActivations;

    public readonly double MinStartTime;
    public readonly int StartIndex;
    public readonly int EndIndex;
    public readonly int VibesUsed;

    public Activation(double minStartTime, int startIndex, int endIndex, int vibesUsed) {
        MinStartTime = minStartTime;
        MaxStartTime = double.MaxValue;
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