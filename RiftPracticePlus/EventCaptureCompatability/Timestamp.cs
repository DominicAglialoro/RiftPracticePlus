using System;

namespace RiftPracticePlus;

public readonly struct Timestamp : IComparable<Timestamp> {
    public readonly double Time;
    public readonly double Beat;

    public Timestamp(double time, double beat) {
        Time = time;
        Beat = beat;
    }

    public int CompareTo(Timestamp other) => Time.CompareTo(other.Time);
}