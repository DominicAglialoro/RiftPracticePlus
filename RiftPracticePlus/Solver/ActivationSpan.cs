using System;

namespace RiftPracticePlus;

public record struct ActivationSpan(double StartTime, int StartIndex, int EndIndex) : IComparable<ActivationSpan> {
    public int CompareTo(ActivationSpan other) => StartTime.CompareTo(other.StartTime);
}