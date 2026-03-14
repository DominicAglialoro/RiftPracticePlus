using System;

namespace RiftCommon;

public record struct Hit(double Time, double Beat, double EndTime, double EndBeat, EnemyType EnemyType, int Column, bool FacingLeft, int Score, bool GivesVibe) : IComparable<Hit> {
    public int CompareTo(Hit other) {
        int timeComparison = Time.CompareTo(other.Time);

        if (timeComparison != 0)
            return timeComparison;

        if (GivesVibe && other.GivesVibe)
            return 0;

        int givesVibeComparison = GivesVibe.CompareTo(other.GivesVibe);

        if (givesVibeComparison != 0)
            return givesVibeComparison;

        return Column.CompareTo(other.Column);
    }
}