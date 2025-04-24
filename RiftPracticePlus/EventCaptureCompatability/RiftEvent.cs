using System;
using RiftCommon;

namespace RiftPracticePlus;

public readonly struct RiftEvent : IComparable<RiftEvent> {
    public readonly EventType EventType;
    public readonly Timestamp Time;
    public readonly Timestamp TargetTime;
    public readonly EnemyType EnemyType;
    public readonly int Column;
    public readonly int TotalScore;
    public readonly int BaseScore;
    public readonly int BaseMultiplier;
    public readonly int VibeMultiplier;
    public readonly int PerfectBonus;
    public readonly bool VibeChain;

    public RiftEvent(EventType eventType, Timestamp time, Timestamp targetTime, EnemyType enemyType, int column, int totalScore, int baseScore, int baseMultiplier, int vibeMultiplier, int perfectBonus, bool vibeChain) {
        EventType = eventType;
        Time = time;
        TargetTime = targetTime;
        EnemyType = enemyType;
        Column = column;
        TotalScore = totalScore;
        BaseScore = baseScore;
        BaseMultiplier = baseMultiplier;
        VibeMultiplier = vibeMultiplier;
        PerfectBonus = perfectBonus;
        VibeChain = vibeChain;
    }

    public int CompareTo(RiftEvent other) {
        int timestampComparison = Time.CompareTo(other.Time);

        if (timestampComparison != 0)
            return timestampComparison;

        int eventTypeComparison = ((int) EventType).CompareTo((int) other.EventType);

        if (eventTypeComparison != 0)
            return eventTypeComparison;

        return Column.CompareTo(other.Column);
    }
}