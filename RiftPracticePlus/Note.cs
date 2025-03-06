namespace RiftPracticePlus;

public readonly struct Note {
    public readonly float StartTime;
    public readonly float EndTime;
    public readonly int Column;

    public Note(float startTime, float endTime, int column) {
        StartTime = startTime;
        EndTime = endTime;
        Column = column;
    }
}