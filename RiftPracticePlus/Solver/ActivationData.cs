using System;

namespace RiftPracticePlus;

public readonly struct ActivationData : IComparable<ActivationData> {
    public readonly double MinStartTime;
    public readonly int StartIndex;
    public readonly int EndIndex;
    public readonly int Score;

    public ActivationData(double minStartTime, int startIndex, int endIndex, int score) {
        MinStartTime = minStartTime;
        StartIndex = startIndex;
        EndIndex = endIndex;
        Score = score;
    }

    public int CompareTo(ActivationData other) => MinStartTime.CompareTo(other.MinStartTime);
}