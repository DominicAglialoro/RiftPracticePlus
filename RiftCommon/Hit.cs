using System;

namespace RiftCommon;

public readonly struct Hit : IComparable<Hit> {
    public readonly double Time;
    public readonly double Beat;
    public readonly double EndTime;
    public readonly double EndBeat;
    public readonly EnemyType EnemyType;
    public readonly int Column;
    public readonly bool FacingLeft;
    public readonly int Score;
    public readonly bool GivesVibe;

    public Hit(double time, double beat, double endTime, double endBeat, EnemyType enemyType, int column, bool facingLeft, int score, bool givesVibe) {
        Time = time;
        Beat = beat;
        EndTime = endTime;
        EndBeat = endBeat;
        EnemyType = enemyType;
        Column = column;
        FacingLeft = facingLeft;
        Score = score;
        GivesVibe = givesVibe;
    }

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