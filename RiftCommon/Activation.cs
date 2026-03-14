using System;

namespace RiftCommon;

public record struct Activation(double MinStartTime, double MinStartBeat, double MaxStartTime, double MaxStartBeat, double LastHitTime, double LastHitBeat, int Score, bool IsOptimal) : IComparable<Activation> {

    public int CompareTo(Activation other) => MinStartTime.CompareTo(other.MinStartTime);
}