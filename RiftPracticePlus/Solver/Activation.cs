namespace RiftPracticePlus;

public readonly struct Activation {
    public readonly double MinStartTime;
    public readonly int StartIndex;
    public readonly int EndIndex;
    public readonly int Score;

    public Activation(double minStartTime, int startIndex, int endIndex, int score) {
        MinStartTime = minStartTime;
        StartIndex = startIndex;
        EndIndex = endIndex;
        Score = score;
    }
}