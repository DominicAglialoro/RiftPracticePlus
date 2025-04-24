namespace RiftCommon;

public readonly struct Hit {
    public readonly double Time;
    public readonly double Beat;
    public readonly double EndTime;
    public readonly double EndBeat;
    public readonly EnemyType EnemyType;
    public readonly int Column;
    public readonly int Score;
    public readonly bool GivesVibe;

    public Hit(double time, double beat, double endTime, double endBeat, EnemyType enemyType, int column, int score, bool givesVibe) {
        Time = time;
        Beat = beat;
        EndTime = endTime;
        EndBeat = endBeat;
        EnemyType = enemyType;
        Column = column;
        Score = score;
        GivesVibe = givesVibe;
    }
}