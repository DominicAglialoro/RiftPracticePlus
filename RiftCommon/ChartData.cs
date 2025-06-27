using System;
using System.IO;

namespace RiftCommon;

public class ChartData {
    private const string HEADER = "RIFT_CHART_DATA";
    private const int FORMAT_VERSION = 4;

    public static ChartData LoadFromFile(string path) {
        using var reader = new BinaryReader(File.OpenRead(path));

        string header = reader.ReadString();

        if (header != HEADER)
            throw new InvalidOperationException("Not a valid chart data file");

        int formatVersion = reader.ReadInt32();

        if (formatVersion < 0 || formatVersion > FORMAT_VERSION)
            throw new InvalidOperationException("Invalid version number");

        string name = reader.ReadString();
        string id = reader.ReadString();
        var difficulty = (Difficulty) reader.ReadInt32();
        float intensity = formatVersion >= 1 ? reader.ReadSingle() : 0f;
        bool isCustom = reader.ReadBoolean();
        int maxBaseScore = formatVersion >= 3 ? reader.ReadInt32() : 0;
        int maxCombo = formatVersion >= 4 ? reader.ReadInt32() : 0;
        float bpm = reader.ReadSingle();
        int beatDivisions = reader.ReadInt32();
        int bpmChangesCount = formatVersion >= 2 ? reader.ReadInt32() : 0;
        var bpmChanges = new BPMChange[bpmChangesCount];

        for (int i = 0; i < bpmChangesCount; i++) {
            double time = reader.ReadDouble();
            double beat = reader.ReadDouble();
            float changeBpm = reader.ReadSingle();

            bpmChanges[i] = new BPMChange(time, beat, changeBpm);
        }

        int beatTimingsCount = reader.ReadInt32();
        double[] beatTimings = new double[beatTimingsCount];

        for (int i = 0; i < beatTimingsCount; i++)
            beatTimings[i] = reader.ReadDouble();

        var beatData = new BeatData(bpm, beatDivisions, bpmChanges, beatTimings);
        int hitsCount = reader.ReadInt32();
        var hits = new Hit[hitsCount];

        for (int i = 0; i < hitsCount; i++) {
            double time = reader.ReadDouble();
            double beat = reader.ReadDouble();
            double endTime = reader.ReadDouble();
            double endBeat = reader.ReadDouble();
            var enemyType = (EnemyType) reader.ReadInt32();
            int column = reader.ReadInt32();
            bool facingLeft = formatVersion >= 1 && reader.ReadBoolean();
            int score = reader.ReadInt32();
            bool givesVibe = reader.ReadBoolean();

            hits[i] = new Hit(time, beat, endTime, endBeat, enemyType, column, facingLeft, score, givesVibe);
        }

        int maxVibeBonus = reader.ReadInt32();
        int singleVibeActivationsCount = reader.ReadInt32();
        var singleVibeActivations = new Activation[singleVibeActivationsCount];

        for (int i = 0; i < singleVibeActivationsCount; i++) {
            double minStartTime = reader.ReadDouble();
            double minStartBeat = formatVersion >= 1 ? reader.ReadDouble() : beatData.GetBeatFromTime(minStartTime);
            double maxStartTime = reader.ReadDouble();
            double maxStartBeat = formatVersion >= 1 ? reader.ReadDouble() : beatData.GetBeatFromTime(maxStartTime);
            double lastHitTime = reader.ReadDouble();
            double lastHitBeat = formatVersion >= 1 ? reader.ReadDouble() : beatData.GetBeatFromTime(lastHitTime);
            int score = reader.ReadInt32();
            bool isOptimal = reader.ReadBoolean();

            singleVibeActivations[i] = new Activation(minStartTime, minStartBeat, maxStartTime, maxStartBeat, lastHitTime, lastHitBeat, score, isOptimal);
        }

        int doubleVibeActivationsCount = reader.ReadInt32();
        var doubleVibeActivations = new Activation[doubleVibeActivationsCount];

        for (int i = 0; i < doubleVibeActivationsCount; i++) {
            double minStartTime = reader.ReadDouble();
            double minStartBeat = formatVersion >= 1 ? reader.ReadDouble() : beatData.GetBeatFromTime(minStartTime);
            double maxStartTime = reader.ReadDouble();
            double maxStartBeat = formatVersion >= 1 ? reader.ReadDouble() : beatData.GetBeatFromTime(maxStartTime);
            double lastHitTime = reader.ReadDouble();
            double lastHitBeat = formatVersion >= 1 ? reader.ReadDouble() : beatData.GetBeatFromTime(lastHitTime);
            int score = reader.ReadInt32();
            bool isOptimal = reader.ReadBoolean();

            doubleVibeActivations[i] = new Activation(minStartTime, minStartBeat, maxStartTime, maxStartBeat, lastHitTime, lastHitBeat, score, isOptimal);
        }

        return new ChartData(name, id, difficulty, intensity, isCustom, maxBaseScore, maxCombo, beatData, hits, new VibeData(maxVibeBonus, singleVibeActivations, doubleVibeActivations));
    }

    public readonly string Name;
    public readonly string ID;
    public readonly Difficulty Difficulty;
    public readonly float Intensity;
    public readonly bool IsCustom;
    public readonly int MaxBaseScore;
    public readonly int MaxCombo;
    public readonly BeatData BeatData;
    public readonly Hit[] Hits;
    public readonly VibeData VibeData;

    public ChartData(string name, string id, Difficulty difficulty, float intensity, bool isCustom, int maxBaseScore, int maxCombo, BeatData beatData, Hit[] hits, VibeData vibeData) {
        Name = name;
        ID = id;
        Difficulty = difficulty;
        Intensity = intensity;
        IsCustom = isCustom;
        BeatData = beatData;
        Hits = hits;
        VibeData = vibeData;
        MaxCombo = maxCombo;
        MaxBaseScore = maxBaseScore;
    }

    public void SaveToFile(string path) {
        using var writer = new BinaryWriter(File.Create(path));

        writer.Write(HEADER);
        writer.Write(FORMAT_VERSION);
        writer.Write(Name);
        writer.Write(ID);
        writer.Write((int) Difficulty);
        writer.Write(Intensity);
        writer.Write(IsCustom);
        writer.Write(MaxBaseScore);
        writer.Write(MaxCombo);
        writer.Write(BeatData.BPM);
        writer.Write(BeatData.BeatDivisions);
        writer.Write(BeatData.BPMChanges.Length);

        foreach (var bpmChange in BeatData.BPMChanges) {
            writer.Write(bpmChange.Time);
            writer.Write(bpmChange.Beat);
            writer.Write(bpmChange.BPM);
        }

        writer.Write(BeatData.BeatTimings.Length);

        foreach (double beatTiming in BeatData.BeatTimings)
            writer.Write(beatTiming);

        writer.Write(Hits.Length);

        foreach (var hit in Hits) {
            writer.Write(hit.Time);
            writer.Write(hit.Beat);
            writer.Write(hit.EndTime);
            writer.Write(hit.EndBeat);
            writer.Write((int) hit.EnemyType);
            writer.Write(hit.Column);
            writer.Write(hit.FacingLeft);
            writer.Write(hit.Score);
            writer.Write(hit.GivesVibe);
        }

        writer.Write(VibeData.MaxVibeBonus);
        writer.Write(VibeData.SingleVibeActivations.Length);

        foreach (var activation in VibeData.SingleVibeActivations) {
            writer.Write(activation.MinStartTime);
            writer.Write(activation.MinStartBeat);
            writer.Write(activation.MaxStartTime);
            writer.Write(activation.MaxStartBeat);
            writer.Write(activation.LastHitTime);
            writer.Write(activation.LastHitBeat);
            writer.Write(activation.Score);
            writer.Write(activation.IsOptimal);
        }

        writer.Write(VibeData.DoubleVibeActivations.Length);

        foreach (var activation in VibeData.DoubleVibeActivations) {
            writer.Write(activation.MinStartTime);
            writer.Write(activation.MinStartBeat);
            writer.Write(activation.MaxStartTime);
            writer.Write(activation.MaxStartBeat);
            writer.Write(activation.LastHitTime);
            writer.Write(activation.LastHitBeat);
            writer.Write(activation.Score);
            writer.Write(activation.IsOptimal);
        }
    }
}