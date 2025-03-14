using System;
using System.Collections.Generic;
using RiftEventCapture.Common;

namespace RiftPracticePlus;

public class SolverData {
    public readonly BeatData BeatData;
    public readonly List<Hit> Hits;

    public int HitCount => Hits.Count;

    private readonly int[] nextVibes;
    private readonly int[] previousVibes;

    public SolverData(BeatData beatData, List<Hit> hits, int[] nextVibes, int[] previousVibes) {
        BeatData = beatData;
        Hits = hits;
        this.nextVibes = nextVibes;
        this.previousVibes = previousVibes;
    }

    public static SolverData CreateFromCaptureResult(CaptureResult captureResult) {
        var beatData = captureResult.BeatData;
        var hits = new List<Hit>();
        var riftEvents = captureResult.RiftEvents;

        if (riftEvents.Count == 0)
            return new SolverData(beatData, hits, Array.Empty<int>(), Array.Empty<int>());

        double currentTime = double.MinValue;
        int currentScore = 0;
        bool currentGivesVibe = false;

        foreach (var riftEvent in riftEvents) {
            if (riftEvent.EventType is not (EventType.EnemyHit or EventType.VibeGained))
                continue;

            if (riftEvent.TargetTime.Time > currentTime) {
                if (currentScore > 0 || currentGivesVibe)
                    hits.Add(new Hit(currentTime, currentScore, currentGivesVibe));

                currentTime = riftEvent.TargetTime.Time;
                currentScore = 0;
                currentGivesVibe = false;
            }

            switch (riftEvent.EventType) {
                case EventType.EnemyHit:
                    currentScore += riftEvent.BaseMultiplier * riftEvent.BaseScore;
                    break;
                case EventType.VibeGained:
                    currentGivesVibe = true;
                    break;
            }
        }

        if (currentScore > 0 || currentGivesVibe)
            hits.Add(new Hit(currentTime, currentScore, currentGivesVibe));

        int[] nextVibes = new int[hits.Count];
        int nextVibe = hits.Count;

        for (int i = hits.Count - 1; i >= 0; i--) {
            if (hits[i].GivesVibe)
                nextVibe = i;

            nextVibes[i] = nextVibe;
        }

        int[] previousVibes = new int[hits.Count];
        int previousVibe = -1;

        for (int i = 0; i < hits.Count; i++) {
            previousVibes[i] = previousVibe;

            if (hits[i].GivesVibe)
                previousVibe = i;
        }

        return new SolverData(beatData, hits, nextVibes, previousVibes);
    }

    public double GetHitTime(int index) {
        if (index < 0)
            return double.NegativeInfinity;

        if (index >= Hits.Count)
            return double.PositiveInfinity;

        return Hits[index].Time;
    }

    public int GetNextVibe(int index) {
        if (index < 0)
            index = 0;

        return index < nextVibes.Length ? nextVibes[index] : nextVibes.Length;
    }

    public int GetPreviousVibe(int index) {
        if (index >= previousVibes.Length)
            index = previousVibes.Length - 1;

        return index >= 0 ? previousVibes[index] : -1;
    }

    public int GetFirstIndexAfter(double endTime) {
        int min = 0;
        int max = Hits.Count;

        while (max >= min) {
            int mid = (min + max) / 2;

            if (Hits[mid].Time > endTime)
                max = mid - 1;
            else
                min = mid + 1;
        }

        return min;
    }
}