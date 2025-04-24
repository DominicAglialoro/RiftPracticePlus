using System;

namespace RiftCommon;

public readonly struct Activation : IComparable<Activation> {
    public readonly double MinStartTime;
    public readonly double MaxStartTime;
    public readonly double LastHitTime;
    public readonly int Score;
    public readonly bool IsOptimal;

    public Activation(double minStartTime, double maxStartTime, double lastHitTime, int score, bool isOptimal) {
        MinStartTime = minStartTime;
        MaxStartTime = maxStartTime;
        LastHitTime = lastHitTime;
        Score = score;
        IsOptimal = isOptimal;
    }

    public int CompareTo(Activation other) => MinStartTime.CompareTo(other.MinStartTime);
}