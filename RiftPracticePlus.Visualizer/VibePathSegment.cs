namespace RiftPracticePlus.Visualizer;

public readonly struct VibePathSegment {
    public readonly double StartTime;
    public readonly double EndTime;
    public readonly double StartVibe;
    public readonly double EndVibe;

    public VibePathSegment(double startTime, double endTime, double startVibe, double endVibe) {
        StartTime = startTime;
        EndTime = endTime;
        StartVibe = startVibe;
        EndVibe = endVibe;
    }
}