using System;
using System.IO;

namespace RiftCommon;

public class ChartData {
    private const string HEADER = "RIFT_CHART_DATA";
    private const int FORMAT_VERSION = 0;

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
        bool isCustom = reader.ReadBoolean();
        float bpm = reader.ReadSingle();
        int beatDivisions = reader.ReadInt32();
        int beatTimingsCount = reader.ReadInt32();
        double[] beatTimings = new double[beatTimingsCount];

        for (int i = 0; i < beatTimingsCount; i++)
            beatTimings[i] = reader.ReadDouble();

        int hitsCount = reader.ReadInt32();
        var hits = new Hit[hitsCount];

        for (int i = 0; i < hitsCount; i++) {
            double time = reader.ReadDouble();
            double beat = reader.ReadDouble();
            double endTime = reader.ReadDouble();
            double endBeat = reader.ReadDouble();
            var enemyType = (EnemyType) reader.ReadInt32();
            int column = reader.ReadInt32();
            int score = reader.ReadInt32();
            bool givesVibe = reader.ReadBoolean();

            hits[i] = new Hit(time, beat, endTime, endBeat, enemyType, column, score, givesVibe);
        }

        int maxVibeBonus = reader.ReadInt32();
        int singleVibeActivationsCount = reader.ReadInt32();
        var singleVibeActivations = new Activation[singleVibeActivationsCount];

        for (int i = 0; i < singleVibeActivationsCount; i++) {
            double minStartTime = reader.ReadDouble();
            double maxStartTime = reader.ReadDouble();
            double lastHitTime = reader.ReadDouble();
            int score = reader.ReadInt32();
            bool isOptimal = reader.ReadBoolean();

            singleVibeActivations[i] = new Activation(minStartTime, maxStartTime, lastHitTime, score, isOptimal);
        }

        int doubleVibeActivationsCount = reader.ReadInt32();
        var doubleVibeActivations = new Activation[doubleVibeActivationsCount];

        for (int i = 0; i < doubleVibeActivationsCount; i++) {
            double minStartTime = reader.ReadDouble();
            double maxStartTime = reader.ReadDouble();
            double lastHitTime = reader.ReadDouble();
            int score = reader.ReadInt32();
            bool isOptimal = reader.ReadBoolean();

            doubleVibeActivations[i] = new Activation(minStartTime, maxStartTime, lastHitTime, score, isOptimal);
        }

        return new ChartData(name, id, difficulty, isCustom, new BeatData(bpm, beatDivisions, beatTimings), hits, new VibeData(maxVibeBonus, singleVibeActivations, doubleVibeActivations));
    }

    public readonly string Name;
    public readonly string ID;
    public readonly Difficulty Difficulty;
    public readonly bool IsCustom;
    public readonly BeatData BeatData;
    public readonly Hit[] Hits;
    public readonly VibeData VibeData;

    public ChartData(string name, string id, Difficulty difficulty, bool isCustom, BeatData beatData, Hit[] hits, VibeData vibeData) {
        Name = name;
        ID = id;
        Difficulty = difficulty;
        IsCustom = isCustom;
        BeatData = beatData;
        Hits = hits;
        VibeData = vibeData;
    }

    public void SaveToFile(string path) {
        using var writer = new BinaryWriter(File.Create(path));

        writer.Write(HEADER);
        writer.Write(FORMAT_VERSION);
        writer.Write(Name);
        writer.Write(ID);
        writer.Write((int) Difficulty);
        writer.Write(IsCustom);
        writer.Write(BeatData.BPM);
        writer.Write(BeatData.BeatDivisions);
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
            writer.Write(hit.Score);
            writer.Write(hit.GivesVibe);
        }

        writer.Write(VibeData.MaxVibeBonus);
        writer.Write(VibeData.SingleVibeActivations.Length);

        foreach (var activation in VibeData.SingleVibeActivations) {
            writer.Write(activation.MinStartTime);
            writer.Write(activation.MaxStartTime);
            writer.Write(activation.LastHitTime);
            writer.Write(activation.Score);
            writer.Write(activation.IsOptimal);
        }

        writer.Write(VibeData.DoubleVibeActivations.Length);

        foreach (var activation in VibeData.DoubleVibeActivations) {
            writer.Write(activation.MinStartTime);
            writer.Write(activation.MaxStartTime);
            writer.Write(activation.LastHitTime);
            writer.Write(activation.Score);
            writer.Write(activation.IsOptimal);
        }
    }
}