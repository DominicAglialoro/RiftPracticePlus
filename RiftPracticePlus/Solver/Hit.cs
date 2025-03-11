using System;

namespace RiftPracticePlus;

public readonly struct Hit : IComparable<Hit> {
    public readonly double Time;
    public readonly int Score;
    public readonly bool GivesVibe;

    public Hit(double time, int score, bool givesVibe) {
        Time = time;
        Score = score;
        GivesVibe = givesVibe;
    }

    public int CompareTo(Hit other) => Time.CompareTo(other.Time);
}