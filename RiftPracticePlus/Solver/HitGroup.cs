using System;
using System.Collections.Generic;

namespace RiftPracticePlus;

public readonly struct HitGroup : IComparable<HitGroup> {
    public readonly double Time;
    public readonly int Score;
    public readonly bool GivesVibe;

    public HitGroup(double time, int score, bool givesVibe) {
        Time = time;
        Score = score;
        GivesVibe = givesVibe;
    }

    public int CompareTo(HitGroup other) => Time.CompareTo(other.Time);
}