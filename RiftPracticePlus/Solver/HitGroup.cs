using System;
using System.Collections.Generic;

namespace RiftPracticePlus;

public readonly struct HitGroup : IComparable<HitGroup> {
    public readonly double Time;
    public readonly double Beat;
    public readonly int Score;
    public readonly bool GivesVibe;

    public HitGroup(double time, double beat, int score, bool givesVibe) {
        Time = time;
        Beat = beat;
        Score = score;
        GivesVibe = givesVibe;
    }

    public int CompareTo(HitGroup other) => Time.CompareTo(other.Time);
}