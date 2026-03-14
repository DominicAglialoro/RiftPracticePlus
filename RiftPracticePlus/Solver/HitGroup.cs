using System;

namespace RiftPracticePlus;

public record struct HitGroup(double Time, double Beat, int Score, bool GivesVibe) : IComparable<HitGroup> {
    public int CompareTo(HitGroup other) => Time.CompareTo(other.Time);
}