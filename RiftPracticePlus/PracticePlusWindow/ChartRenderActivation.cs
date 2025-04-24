using System;

namespace RiftPracticePlus;

public struct ChartRenderActivation : IComparable<ChartRenderActivation> {
    public float StartTime;
    public float EndTime;
    public bool IsDouble;

    public ChartRenderActivation(float startTime, float endTime, bool isDouble) {
        StartTime = startTime;
        EndTime = endTime;
        IsDouble = isDouble;
    }

    public int CompareTo(ChartRenderActivation other) {
        int startTimeComparison = StartTime.CompareTo(other.StartTime);

        if (startTimeComparison != 0)
            return startTimeComparison;

        return IsDouble.CompareTo(other.IsDouble);
    }
}