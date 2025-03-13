using System;
using System.Collections.Generic;
using System.Linq;
using RiftEventCapture.Common;

namespace RiftPracticePlus;

public static class Solver {
    private const double VIBE_LENGTH = 5d;

    public static Note[] Solve(CaptureResult captureResult) {
        var data = SolverData.CreateFromCaptureResult(captureResult);

        if (data.HitCount == 0)
            return Array.Empty<Note>();

        var singleVibeActivations = GetActivations(data, 1);
        var doubleVibeActivations = GetActivations(data, 2);
        var bestNextActivationsByFirstVibeIndex = GetBestNextActivationsByFirstVibeIndex(data, singleVibeActivations, doubleVibeActivations);
        var (optimalSingleVibeActivations, optimalDoubleVibeActivations) = GetOptimalActivations(data, singleVibeActivations, doubleVibeActivations, bestNextActivationsByFirstVibeIndex);

        return GetVibeRanges(singleVibeActivations, doubleVibeActivations, optimalSingleVibeActivations, optimalDoubleVibeActivations);
    }

    private static List<Activation> GetActivations(SolverData data, int vibesUsed) {
        var activations = new List<Activation>();
        int fromIndex = data.GetNextVibe(0);

        if (vibesUsed == 2)
            fromIndex = data.GetNextVibe(fromIndex + 1);

        if (fromIndex == data.HitCount)
            return activations;

        var hits = data.Hits;
        var currentSpan = GetNextSpanWithStartIndex(data, fromIndex + 1, data.GetHitTime(fromIndex), vibesUsed);
        int currentStartIndex = currentSpan.StartIndex;
        int currentEndIndex = currentSpan.StartIndex;
        int score = 0;

        while (currentEndIndex < currentSpan.EndIndex) {
            score += hits[currentEndIndex].Score;
            currentEndIndex++;
        }

        while (currentSpan.StartIndex < data.HitCount) {
            var spanWithNewStartIndex = GetNextSpanWithStartIndex(data, currentSpan.StartIndex + 1, data.GetHitTime(currentSpan.StartIndex), vibesUsed);
            var spanWithNewEndIndex = GetNextSpanWithEndIndex(data, currentSpan.EndIndex + 1, currentSpan.StartTime, vibesUsed);

            if (spanWithNewStartIndex.StartTime < spanWithNewEndIndex.StartTime)
                currentSpan = spanWithNewStartIndex;
            else
                currentSpan = spanWithNewEndIndex;

            while (currentStartIndex < currentSpan.StartIndex) {
                score -= hits[currentStartIndex].Score;
                currentStartIndex++;
            }

            while (currentEndIndex < currentSpan.EndIndex) {
                score += hits[currentEndIndex].Score;
                currentEndIndex++;
            }

            while (currentEndIndex > currentSpan.EndIndex) {
                currentEndIndex--;
                score -= hits[currentEndIndex].Score;
            }

            activations.Add(new Activation(currentSpan.StartTime, currentSpan.StartIndex, currentSpan.EndIndex, score));
        }

        return activations;
    }

    private static ActivationSpan GetNextSpanWithStartIndex(SolverData data, int startIndex, double startTime, int vibesUsed) {
        var beatData = data.BeatData;
        double currentTime = startTime;
        double vibeRemaining = vibesUsed * VIBE_LENGTH;
        double vibeZeroTime = currentTime + vibeRemaining;
        double endTime = GetEndTime();
        int nextVibeIndex = data.GetNextVibe(startIndex);

        while (nextVibeIndex < data.HitCount) {
            double nextVibeTime = data.GetHitTime(nextVibeIndex);

            if (nextVibeTime > endTime)
                break;

            vibeRemaining = Math.Max(VIBE_LENGTH, Math.Min(vibeRemaining - (nextVibeTime - currentTime) + VIBE_LENGTH, 2d * VIBE_LENGTH));
            currentTime = nextVibeTime;
            vibeZeroTime = currentTime + vibeRemaining;
            endTime = GetEndTime();
            nextVibeIndex = data.GetNextVibe(nextVibeIndex + 1);
        }

        return new ActivationSpan(startTime, startIndex, data.GetFirstIndexAfter(endTime));

        double GetEndTime() {
            double hitWindowInBeats = 0.175d / beatData.GetBeatLengthAtTime(vibeZeroTime);
            double maxEndBeat = beatData.GetBeatFromTime(vibeZeroTime) + hitWindowInBeats - Math.Floor(hitWindowInBeats * beatData.BeatDivisions) / beatData.BeatDivisions;

            return beatData.GetTimeFromBeat(maxEndBeat);
        }
    }

    private static ActivationSpan GetNextSpanWithEndIndex(SolverData data, int endIndex, double mustStartAfter, int vibesUsed) {
        var beatData = data.BeatData;
        double currentTime = data.GetHitTime(endIndex - 1);
        double endBeat = beatData.GetBeatFromTime(currentTime);
        double hitWindowInBeats = 0.175d / beatData.GetBeatLengthForBeat((int) endBeat);

        currentTime = beatData.GetTimeFromBeat(endBeat - hitWindowInBeats + Math.Floor(hitWindowInBeats * beatData.BeatDivisions) / beatData.BeatDivisions);
        currentTime = Math.Max(currentTime, data.GetHitTime(endIndex - 2));

        double vibeNeeded = 0d;
        double startTime = double.MaxValue;
        int previousVibeIndex = data.GetPreviousVibe(data.GetFirstIndexAfter(currentTime));

        do {
            double previousVibeTime = data.GetHitTime(previousVibeIndex);
            double possibleStartTime = currentTime - (vibesUsed * VIBE_LENGTH - vibeNeeded);
            double needFullVibeAt = currentTime - (2d * VIBE_LENGTH - vibeNeeded);

            if (possibleStartTime > previousVibeTime && possibleStartTime > mustStartAfter && possibleStartTime < startTime)
                startTime = possibleStartTime;

            if (previousVibeTime <= needFullVibeAt || previousVibeTime <= mustStartAfter)
                break;

            vibeNeeded = Math.Max(0d, Math.Min(vibeNeeded + (currentTime - previousVibeTime) - VIBE_LENGTH, 2d * VIBE_LENGTH));

            if (vibeNeeded <= 0d)
                break;

            currentTime = previousVibeTime;
            previousVibeIndex = data.GetPreviousVibe(previousVibeIndex);
        } while (previousVibeIndex >= 0);

        return new ActivationSpan(startTime, data.GetFirstIndexAfter(startTime), endIndex);
    }

    private static Dictionary<int, BestNextActivations> GetBestNextActivationsByFirstVibeIndex(SolverData data, List<Activation> singleVibeActivations, List<Activation> doubleVibeActivations) {
        var bestNextActivationsByFirstVibeIndex = new Dictionary<int, BestNextActivations>();

        GetBestNextActivations(0);

        return bestNextActivationsByFirstVibeIndex;

        BestNextActivations GetBestNextActivations(int fromIndex) {
            int firstVibeIndex = data.GetNextVibe(fromIndex);

            if (bestNextActivationsByFirstVibeIndex.TryGetValue(firstVibeIndex, out var bestNextActivations))
                return bestNextActivations;

            int secondVibeIndex = data.GetNextVibe(firstVibeIndex + 1);
            int bestNextValue = 0;
            var bestNextSingleVibeActivations = new List<int>();
            var bestNextDoubleVibeActivations = new List<int>();

            for (int i = 0; i < singleVibeActivations.Count; i++) {
                var nextActivation = singleVibeActivations[i];

                if (nextActivation.StartIndex <= firstVibeIndex)
                    continue;

                if (nextActivation.StartIndex > secondVibeIndex)
                    break;

                int nextActivationValue = nextActivation.Score + GetBestNextActivations(nextActivation.EndIndex).BestNextValue;

                if (nextActivationValue > bestNextValue) {
                    bestNextValue = nextActivationValue;
                    bestNextSingleVibeActivations.Clear();
                }

                if (nextActivationValue == bestNextValue)
                    bestNextSingleVibeActivations.Add(i);
            }

            for (int i = 0; i < doubleVibeActivations.Count; i++) {
                var nextActivation = doubleVibeActivations[i];

                if (nextActivation.StartIndex <= secondVibeIndex)
                    continue;

                int nextActivationValue = nextActivation.Score + GetBestNextActivations(nextActivation.EndIndex).BestNextValue;

                if (nextActivationValue > bestNextValue) {
                    bestNextValue = nextActivationValue;
                    bestNextSingleVibeActivations.Clear();
                    bestNextDoubleVibeActivations.Clear();
                }

                if (nextActivationValue == bestNextValue)
                    bestNextDoubleVibeActivations.Add(i);
            }

            bestNextActivations = new BestNextActivations(bestNextValue, bestNextSingleVibeActivations, bestNextDoubleVibeActivations);
            bestNextActivationsByFirstVibeIndex.Add(firstVibeIndex, bestNextActivations);

            return bestNextActivations;
        }
    }

    private static (HashSet<int>, HashSet<int>) GetOptimalActivations(SolverData data, List<Activation> singleVibeActivations, List<Activation> doubleVibeActivations, Dictionary<int, BestNextActivations> bestNextActivationsByFirstVibeIndex) {
        var optimalSingleVibeActivations = new HashSet<int>();
        var optimalDoubleVibeActivations = new HashSet<int>();

        Traverse(0);

        return (optimalSingleVibeActivations, optimalDoubleVibeActivations);

        void Traverse(int fromIndex) {
            var bestNextActivations = bestNextActivationsByFirstVibeIndex[data.GetNextVibe(fromIndex)];

            foreach (int index in bestNextActivations.BestNextSingleVibeActivations) {
                if (optimalSingleVibeActivations.Contains(index))
                    continue;

                optimalSingleVibeActivations.Add(index);
                Traverse(singleVibeActivations[index].EndIndex);
            }

            foreach (int index in bestNextActivations.BestNextDoubleVibeActivations) {
                if (optimalDoubleVibeActivations.Contains(index))
                    continue;

                optimalDoubleVibeActivations.Add(index);
                Traverse(doubleVibeActivations[index].EndIndex);
            }
        }
    }

    private static Note[] GetVibeRanges(List<Activation> singleVibeActivations, List<Activation> doubleVibeActivations, HashSet<int> optimalSingleVibeActivations, HashSet<int> optimalDoubleVibeActivations) {
        var vibeRanges = new Note[optimalSingleVibeActivations.Count + optimalDoubleVibeActivations.Count];
        int i = 0;

        foreach (int index in optimalSingleVibeActivations) {
            vibeRanges[i] = new Note(
                (float) singleVibeActivations[index].MinStartTime,
                index < singleVibeActivations.Count - 1 ? (float) singleVibeActivations[index + 1].MinStartTime : float.MaxValue,
                1);
            i++;
        }

        foreach (int index in optimalDoubleVibeActivations) {
            vibeRanges[i] = new Note(
                (float) doubleVibeActivations[index].MinStartTime,
                index < doubleVibeActivations.Count - 1 ? (float) doubleVibeActivations[index + 1].MinStartTime : float.MaxValue,
                2);
            i++;
        }

        Array.Sort(vibeRanges);

        return vibeRanges;
    }
}