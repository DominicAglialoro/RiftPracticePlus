using System;
using System.Collections.Generic;
using System.IO;
using RiftCommon;

namespace RiftPracticePlus;

public class CaptureResult {
    private const string HEADER = "RIFT_EVENT_CAPTURE";
    private const int FORMAT_VERSION = 0;

    public SessionInfo SessionInfo { get; }
    public BeatData BeatData { get; }
    public IReadOnlyList<RiftEvent> RiftEvents => riftEvents;

    private readonly RiftEvent[] riftEvents;

    public CaptureResult(SessionInfo sessionInfo, BeatData beatData, RiftEvent[] riftEvents) {
        SessionInfo = sessionInfo;
        BeatData = beatData;
        this.riftEvents = riftEvents;
    }

    public Hit[] GetHits() {
        var hits = new List<Hit>();

        for (int i = 0; i < riftEvents.Length; i++) {
            var riftEvent = riftEvents[i];
            double time = riftEvent.Time.Time;
            double beat = riftEvent.Time.Beat;

            switch (riftEvent.EventType) {
                case EventType.EnemyHit:
                    double endTime = time;
                    double endBeat = beat;

                    if (riftEvent.EnemyType == EnemyType.Wyrm) {
                        for (int j = i + 1; j < riftEvents.Length; j++) {
                            var otherEvent = riftEvents[j];

                            if (otherEvent.EventType != EventType.HoldComplete || otherEvent.Column != riftEvent.Column || otherEvent.Time.Beat <= beat)
                                continue;

                            endTime = otherEvent.Time.Time;
                            endBeat = otherEvent.Time.Beat;

                            break;
                        }
                    }

                    hits.Add(new Hit(time, beat, endTime, endBeat, riftEvent.EnemyType, riftEvent.Column, false, riftEvent.BaseMultiplier * riftEvent.BaseScore, false));
                    break;
                case EventType.VibeGained:
                    hits.Add(new Hit(time, beat, time, beat, EnemyType.None, 0, false, 0, true));
                    break;
            }
        }

        hits.Sort();

        return hits.ToArray();
    }

    public static CaptureResult LoadFromFile(string path) {
        using var reader = new BinaryReader(File.OpenRead(path));

        string header = reader.ReadString();

        if (header != HEADER)
            throw new InvalidOperationException("Not a valid capture file");

        int formatVersion = reader.ReadInt32();

        if (formatVersion < 0 || formatVersion > FORMAT_VERSION)
            throw new InvalidOperationException("Invalid version number");

        string chartName = reader.ReadString();
        string chartId = reader.ReadString();
        int chartDifficulty = reader.ReadInt32();
        int pinsCount = reader.ReadInt32();
        string[] pins = new string[pinsCount];

        for (int i = 0; i < pinsCount; i++)
            pins[i] = reader.ReadString();

        var sessionInfo = new SessionInfo(chartName, chartId, chartDifficulty, pins);
        int bpm = reader.ReadInt32();
        int beatDivisions = reader.ReadInt32();
        int beatTimingsCount = reader.ReadInt32();
        double[] beatTimings = new double[beatTimingsCount];

        for (int i = 0; i < beatTimingsCount; i++)
            beatTimings[i] = reader.ReadDouble();

        var beatData = new BeatData(bpm, beatDivisions, beatTimings);
        int riftEventsCount = reader.ReadInt32();
        var riftEvents = new RiftEvent[riftEventsCount];

        for (int i = 0; i < riftEventsCount; i++) {
            int eventType = reader.ReadInt32();
            double timeTime = reader.ReadDouble();
            double timeBeat = reader.ReadDouble();
            double targetTimeTime = reader.ReadDouble();
            double targetTimeBeat = reader.ReadDouble();
            int enemyType = reader.ReadInt32();
            int column = reader.ReadInt32();
            int totalScore = reader.ReadInt32();
            int baseScore = reader.ReadInt32();
            int baseMultiplier = reader.ReadInt32();
            int vibeMultiplier = reader.ReadInt32();
            int perfectBonus = reader.ReadInt32();
            bool vibeChain = reader.ReadBoolean();

            riftEvents[i] = new RiftEvent(
                (EventType) eventType,
                new Timestamp(timeTime, timeBeat),
                new Timestamp(targetTimeTime, targetTimeBeat),
                (EnemyType) enemyType,
                column,
                totalScore,
                baseScore,
                baseMultiplier,
                vibeMultiplier,
                perfectBonus,
                vibeChain);
        }

        return new CaptureResult(sessionInfo, beatData, riftEvents);
    }
}