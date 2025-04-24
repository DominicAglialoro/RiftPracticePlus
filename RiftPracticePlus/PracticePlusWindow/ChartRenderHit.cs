using System;

namespace RiftPracticePlus;

public readonly struct ChartRenderHit : IComparable<ChartRenderHit> {
    public readonly float StartTime;
    public readonly float EndTime;
    public readonly int Column;

    public ChartRenderHit(float startTime, float endTime, int column) {
        StartTime = startTime;
        EndTime = endTime;
        Column = column;
    }

    public int CompareTo(ChartRenderHit other) {
        int startTimeComparison = StartTime.CompareTo(other.StartTime);

        if (startTimeComparison != 0)
            return startTimeComparison;

        return Column.CompareTo(other.Column);
    }
}