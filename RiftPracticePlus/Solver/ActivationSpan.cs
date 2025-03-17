using System;

namespace RiftPracticePlus;

public readonly struct ActivationSpan : IComparable<ActivationSpan> {
    public readonly double StartTime;
    public readonly int StartIndex;
    public readonly int EndIndex;

    public ActivationSpan(double startTime, int startIndex, int endIndex) {
        StartTime = startTime;
        StartIndex = startIndex;
        EndIndex = endIndex;
    }

    public int CompareTo(ActivationSpan other) => StartTime.CompareTo(other.StartTime);
}