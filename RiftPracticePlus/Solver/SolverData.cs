using System.Collections.Generic;
using RiftCommon;

namespace RiftPracticePlus;

public class SolverData {
    public int HitGroupCount => HitGroups.Count;

    public readonly BeatData BeatData;
    public readonly List<HitGroup> HitGroups;

    private readonly int[] nextVibes;
    private readonly int[] previousVibes;

    public SolverData(BeatData beatData, Hit[] hits) {
        BeatData = beatData;
        HitGroups = new List<HitGroup>();

        double currentTime = double.MinValue;
        double currentBeat = double.MinValue;
        int currentScore = 0;
        bool currentGivesVibe = false;

        foreach (var hit in hits) {
            if (hit.Time > currentTime) {
                if (currentScore > 0 || currentGivesVibe)
                    HitGroups.Add(new HitGroup(currentTime, currentBeat, currentScore, currentGivesVibe));

                currentTime = hit.Time;
                currentBeat = hit.Beat;
                currentScore = 0;
                currentGivesVibe = false;
            }

            currentScore += hit.Score;
            currentGivesVibe |= hit.GivesVibe;
        }

        if (currentScore > 0 || currentGivesVibe)
            HitGroups.Add(new HitGroup(currentTime, currentBeat, currentScore, currentGivesVibe));

        nextVibes = new int[HitGroups.Count];

        int nextVibe = HitGroups.Count;

        for (int i = HitGroups.Count - 1; i >= 0; i--) {
            if (HitGroups[i].GivesVibe)
                nextVibe = i;

            nextVibes[i] = nextVibe;
        }

        previousVibes = new int[HitGroups.Count];

        int previousVibe = -1;

        for (int i = 0; i < HitGroups.Count; i++) {
            previousVibes[i] = previousVibe;

            if (HitGroups[i].GivesVibe)
                previousVibe = i;
        }
    }

    public double GetHitTime(int hitIndex) {
        if (hitIndex < 0)
            return double.NegativeInfinity;

        if (hitIndex >= HitGroups.Count)
            return double.PositiveInfinity;

        return HitGroups[hitIndex].Time;
    }

    public double GetHitBeat(int hitIndex) {
        if (hitIndex < 0)
            return double.NegativeInfinity;

        if (hitIndex >= HitGroups.Count)
            return double.PositiveInfinity;

        return HitGroups[hitIndex].Beat;
    }

    public int GetNextVibe(int hitIndex) {
        if (hitIndex < 0)
            hitIndex = 0;

        return hitIndex < nextVibes.Length ? nextVibes[hitIndex] : nextVibes.Length;
    }

    public int GetPreviousVibe(int hitIndex) {
        if (hitIndex >= previousVibes.Length)
            hitIndex = previousVibes.Length - 1;

        return hitIndex >= 0 ? previousVibes[hitIndex] : -1;
    }

    public int GetFirstHitIndexAfter(double time) {
        int min = 0;
        int max = HitGroups.Count;

        while (max >= min && min < HitGroups.Count) {
            int mid = (min + max) / 2;

            if (HitGroups[mid].Time > time)
                max = mid - 1;
            else
                min = mid + 1;
        }

        return min;
    }
}