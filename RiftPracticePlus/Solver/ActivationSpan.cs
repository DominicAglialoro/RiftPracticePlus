namespace RiftPracticePlus;

public readonly struct ActivationSpan {
    public readonly double StartTime;
    public readonly int StartIndex;
    public readonly int EndIndex;

    public ActivationSpan(double startTime, int startIndex, int endIndex) {
        StartTime = startTime;
        StartIndex = startIndex;
        EndIndex = endIndex;
    }
}