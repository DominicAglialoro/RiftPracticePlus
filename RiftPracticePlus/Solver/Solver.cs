using System;
using System.Collections.Generic;
using RiftEventCapture.Common;

namespace RiftPracticePlus;

public static class Solver {
    private const double VIBE_LENGTH = 5d;

    public static Note[] Solve(CaptureResult captureResult) {
        var data = SolverData.CreateFromCaptureResult(captureResult);

        if (data.HitCount == 0)
            return Array.Empty<Note>();

        var activations = new List<Activation>();

        GetActivations(activations, data, 1);
        GetActivations(activations, data, 2);
        activations.Sort();

        var bestFirstActivations = GetBestFirstActivations(activations, data);

        return GetVibeRanges(bestFirstActivations);
    }

    private static void GetActivations(List<Activation> activations, SolverData data, int vibesUsed) {
        int fromIndex = data.GetNextVibe(0);

        if (vibesUsed == 2)
            fromIndex = data.GetNextVibe(fromIndex + 1);

        if (fromIndex == data.HitCount)
            return;

        int firstNewIndex = activations.Count;
        var currentSpan = GetNextSpanWithStartIndex(data, fromIndex + 1, data.GetHitTime(fromIndex), vibesUsed);

        while (currentSpan.StartIndex < data.HitCount) {
            var spanWithNewStartIndex = GetNextSpanWithStartIndex(data, currentSpan.StartIndex + 1, data.GetHitTime(currentSpan.StartIndex), vibesUsed);
            var spanWithNewEndIndex = GetNextSpanWithEndIndex(data, currentSpan.EndIndex + 1, currentSpan.StartTime, vibesUsed);

            if (spanWithNewStartIndex.StartTime < spanWithNewEndIndex.StartTime)
                currentSpan = spanWithNewStartIndex;
            else
                currentSpan = spanWithNewEndIndex;

            activations.Add(new Activation(currentSpan.StartTime, currentSpan.StartIndex, currentSpan.EndIndex, vibesUsed));
        }

        if (activations.Count == firstNewIndex)
            return;

        var hits = data.Hits;
        int score = 0;
        int currentStartIndex = activations[0].StartIndex;
        int currentEndIndex = activations[0].StartIndex;

        for (int i = firstNewIndex; i < activations.Count; i++) {
            var activation = activations[i];

            while (currentStartIndex < activation.StartIndex) {
                score -= hits[currentStartIndex].Score;
                currentStartIndex++;
            }

            while (currentEndIndex < activation.EndIndex) {
                score += hits[currentEndIndex].Score;
                currentEndIndex++;
            }

            while (currentEndIndex > activation.EndIndex) {
                currentEndIndex--;
                score -= hits[currentEndIndex].Score;
            }

            activation.Score = score;

            if (i > firstNewIndex)
                activations[i - 1].MaxStartTime = activation.MinStartTime;
        }
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

    private static Activation[] GetBestFirstActivations(List<Activation> activations, SolverData data) {
        var bestNextActivations = new List<Activation>();

        for (int i = activations.Count - 1; i >= 0; i--) {
            var activation = activations[i];

            activation.BestNextActivations = GetBestNextActivations(activation.EndIndex, out int bestValue);
            activation.MaxValue = activation.Score + bestValue;
        }

        return GetBestNextActivations(0, out _);

        Activation[] GetBestNextActivations(int fromIndex, out int bestValue) {
            int firstVibeIndex = data.GetNextVibe(fromIndex);
            int secondVibeIndex = data.GetNextVibe(firstVibeIndex + 1);

            bestValue = 0;

            for (int i = activations.Count - 1; i >= 0; i--) {
                var activation = activations[i];

                if (activation.StartIndex <= firstVibeIndex)
                    break;

                if (activation.VibesUsed == (activation.StartIndex <= secondVibeIndex ? 1 : 2) && activation.MaxValue > bestValue)
                    bestValue = activation.MaxValue;
            }

            for (int i = activations.Count - 1; i >= 0; i--) {
                var activation = activations[i];

                if (activation.StartIndex <= firstVibeIndex)
                    break;

                if (activation.VibesUsed == (activation.StartIndex <= secondVibeIndex ? 1 : 2) && activation.MaxValue == bestValue)
                    bestNextActivations.Add(activation);
            }

            var result = bestNextActivations.ToArray();

            bestNextActivations.Clear();

            return result;
        }
    }

    private static Note[] GetVibeRanges(Activation[] bestFirstActivations) {
        var viableActivations = new HashSet<Activation>();

        foreach (var activation in bestFirstActivations)
            Recurse(activation);

        var vibeRanges = new Note[viableActivations.Count];
        int i = 0;

        foreach (var activation in viableActivations) {
            vibeRanges[i] = new Note((float) activation.MinStartTime, (float) activation.MaxStartTime, activation.VibesUsed);
            i++;
        }

        Array.Sort(vibeRanges);

        return vibeRanges;

        void Recurse(Activation activation) {
            if (viableActivations.Contains(activation))
                return;

            viableActivations.Add(activation);

            foreach (var nextActivation in activation.BestNextActivations)
                Recurse(nextActivation);
        }
    }
}