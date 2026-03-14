using System;

namespace RiftPracticePlus;

public record struct ActivationData(double MinStartTime, int StartIndex, int EndIndex, int Score) : IComparable<ActivationData> {
    public int CompareTo(ActivationData other) => MinStartTime.CompareTo(other.MinStartTime);
}