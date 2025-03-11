using System;

namespace RiftPracticePlus;

public readonly struct Note : IComparable<Note> {
    public readonly float StartTime;
    public readonly float EndTime;
    public readonly int Column;

    public Note(float startTime, float endTime, int column) {
        StartTime = startTime;
        EndTime = endTime;
        Column = column;
    }

    public int CompareTo(Note other) {
        int startTimeComparison = StartTime.CompareTo(other.StartTime);

        if (startTimeComparison != 0)
            return startTimeComparison;

        return Column.CompareTo(other.Column);
    }
}