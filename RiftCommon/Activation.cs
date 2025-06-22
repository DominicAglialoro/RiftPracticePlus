using System;

namespace RiftCommon;

public readonly struct Activation : IComparable<Activation> {
    public readonly double MinStartTime;
    public readonly double MinStartBeat;
    public readonly double MaxStartTime;
    public readonly double MaxStartBeat;
    public readonly double LastHitTime;
    public readonly double LastHitBeat;
    public readonly int Score;
    public readonly bool IsOptimal;

    public Activation(double minStartTime, double minStartBeat, double maxStartTime, double maxStartBeat, double lastHitTime, double lastHitBeat, int score, bool isOptimal) {
        MinStartTime = minStartTime;
        MinStartBeat = minStartBeat;
        MaxStartTime = maxStartTime;
        MaxStartBeat = maxStartBeat;
        LastHitTime = lastHitTime;
        LastHitBeat = lastHitBeat;
        Score = score;
        IsOptimal = isOptimal;
    }

    public int CompareTo(Activation other) => MinStartTime.CompareTo(other.MinStartTime);
}