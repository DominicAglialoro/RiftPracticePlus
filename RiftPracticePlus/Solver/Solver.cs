using System;
using System.Collections.Generic;
using RiftCommon;

namespace RiftPracticePlus;

public static class Solver {
    private const double VIBE_LENGTH = 5d;

    public static VibeData Solve(SolverData data) {
        if (data.HitGroupCount == 0)
            return new VibeData(0, Array.Empty<Activation>(), Array.Empty<Activation>());

        var singleVibeActivationData = GetActivationData(data, 1);
        var doubleVibeActivationData = GetActivationData(data, 2);
        var singleVibeActivations = new Activation[singleVibeActivationData.Count];
        var doubleVibeActivations = new Activation[doubleVibeActivationData.Count];

        for (int i = 0; i < singleVibeActivationData.Count; i++)
            singleVibeActivations[i] = GetActivationFromData(data, singleVibeActivationData, i);

        for (int i = 0; i < doubleVibeActivationData.Count; i++)
            doubleVibeActivations[i] = GetActivationFromData(data, doubleVibeActivationData, i);

        var bestNextActivationsByFirstVibeIndex = GetBestNextActivationsByFirstVibeIndex(data, singleVibeActivationData, doubleVibeActivationData, out int totalScore);
        var (bestSingleVibeActivationIndices, bestDoubleVibeActivationIndices) = GetBestActivationIndices(data, singleVibeActivationData, doubleVibeActivationData, bestNextActivationsByFirstVibeIndex);

        foreach (int i in bestSingleVibeActivationIndices) {
            var activation = singleVibeActivations[i];

            singleVibeActivations[i] = new Activation(
                activation.MinStartTime,
                activation.MinStartBeat,
                activation.MaxStartTime,
                activation.MaxStartBeat,
                activation.LastHitTime,
                activation.LastHitBeat,
                activation.Score, true);
        }


        foreach (int i in bestDoubleVibeActivationIndices) {
            var activation = doubleVibeActivations[i];

            doubleVibeActivations[i] = new Activation(
                activation.MinStartTime,
                activation.MinStartBeat,
                activation.MaxStartTime,
                activation.MaxStartBeat,
                activation.LastHitTime,
                activation.LastHitBeat,
                activation.Score, true);
        }

        Array.Sort(singleVibeActivations);
        Array.Sort(doubleVibeActivations);

        return new VibeData(totalScore, singleVibeActivations, doubleVibeActivations);
    }

    private static List<ActivationData> GetActivationData(SolverData data, int vibesUsed) {
        var activations = new List<ActivationData>();
        int fromIndex = data.GetNextVibe(0);

        if (vibesUsed == 2)
            fromIndex = data.GetNextVibe(fromIndex + 1);

        if (fromIndex == data.HitGroupCount)
            return activations;

        var hits = data.HitGroups;
        var spans = new List<ActivationSpan>(2 * hits.Count);

        for (int i = fromIndex; i < hits.Count; i++)
            spans.Add(GetSpanStartingAfterHit(data, i, vibesUsed));

        for (int i = fromIndex; i < hits.Count; i++)
            GetSpansEndingOnHit(spans, data, i, vibesUsed);

        GetSpansEndingOnBeats(spans, data, vibesUsed);

        if (spans.Count == 0)
            return activations;

        spans.Sort();

        int currentStartIndex = spans[0].StartIndex;
        int currentEndIndex = spans[0].StartIndex;
        int score = 0;

        foreach (var span in spans) {
            if (span.StartIndex <= fromIndex)
                continue;

            while (currentStartIndex < span.StartIndex) {
                score -= hits[currentStartIndex].Score;
                currentStartIndex++;
            }

            while (currentEndIndex < span.EndIndex) {
                score += hits[currentEndIndex].Score;
                currentEndIndex++;
            }

            while (currentEndIndex > span.EndIndex) {
                currentEndIndex--;
                score -= hits[currentEndIndex].Score;
            }

            if (activations.Count > 0 && activations[activations.Count - 1].MinStartTime == span.StartTime) {
                if (activations[activations.Count - 1].EndIndex >= span.EndIndex)
                    continue;

                activations.RemoveAt(activations.Count - 1);
            }

            activations.Add(new ActivationData(span.StartTime, span.StartIndex, span.EndIndex, score));
        }

        return activations;
    }

    private static ActivationSpan GetSpanStartingAt(SolverData data, double startTime, int firstHitIndex, int vibesUsed) {
        var beatData = data.BeatData;
        double currentTime = startTime;
        double vibeRemaining = vibesUsed * VIBE_LENGTH;
        double vibeZeroTime = currentTime + vibeRemaining;
        double endTime = GetEndTime();
        int nextVibeIndex = data.GetNextVibe(firstHitIndex);

        while (nextVibeIndex < data.HitGroupCount) {
            double nextVibeTime = data.GetHitTime(nextVibeIndex);

            if (nextVibeTime > endTime)
                break;

            vibeRemaining = Math.Max(VIBE_LENGTH, Math.Min(vibeRemaining - (nextVibeTime - currentTime) + VIBE_LENGTH, 2d * VIBE_LENGTH));
            currentTime = nextVibeTime;
            vibeZeroTime = currentTime + vibeRemaining;
            endTime = GetEndTime();
            nextVibeIndex = data.GetNextVibe(nextVibeIndex + 1);
        }

        return new ActivationSpan(startTime, firstHitIndex, data.GetFirstHitIndexAfter(endTime));

        double GetEndTime() {
            double maxEndTime = beatData.GetTimeFromBeat(beatData.GetBeatFromTime(vibeZeroTime) + GetVibeExtensionForBeatLength(beatData.GetBeatLengthAtTime(vibeZeroTime), beatData.BeatDivisions));

            return Math.Min(maxEndTime, data.GetHitTime(data.GetFirstHitIndexAfter(vibeZeroTime)));
        }
    }

    private static ActivationSpan GetSpanStartingAfterHit(SolverData data, int hitIndex, int vibesUsed) => GetSpanStartingAt(data, data.GetHitTime(hitIndex), hitIndex + 1, vibesUsed);

    private static void GetSpansEndingOnHit(List<ActivationSpan> spans, SolverData data, int hitIndex, int vibesUsed) {
        var beatData = data.BeatData;
        double endTime = data.GetHitTime(hitIndex);
        double endBeat = beatData.GetBeatFromTime(endTime);
        double previousHitTime = data.GetHitTime(hitIndex - 1);
        int endIndex;

        if (data.HitGroups[hitIndex].GivesVibe)
            endIndex = GetSpanStartingAfterHit(data, hitIndex, 1).EndIndex;
        else
            endIndex = hitIndex + 1;

        if (!beatData.HasBeatTimings) {
            double vibeZeroTime = beatData.GetTimeFromBeat(endBeat - GetVibeExtensionForBeatLength(beatData.GetBeatLengthForBeat((int) endBeat), beatData.BeatDivisions));

            vibeZeroTime = Math.Max(vibeZeroTime, previousHitTime);
            GetSpansWhereVibeHitsZeroAt(spans, data, vibeZeroTime, endIndex, vibesUsed);

            return;
        }

        for (int i = (int) endBeat; i >= 1; i--) {
            double beatTime = beatData.GetTimeFromBeat(i);
            double nextBeatTime = beatData.GetTimeFromBeat(i + 1);
            double vibeZeroTime = beatData.GetTimeFromBeat(endBeat - GetVibeExtensionForBeatLength(nextBeatTime - beatTime, beatData.BeatDivisions));

            vibeZeroTime = Math.Max(vibeZeroTime, previousHitTime);

            if (vibeZeroTime > beatTime && vibeZeroTime < nextBeatTime)
                GetSpansWhereVibeHitsZeroAt(spans, data, vibeZeroTime, endIndex, vibesUsed);

            if (beatTime <= previousHitTime)
                break;
        }
    }

    private static void GetSpansEndingOnBeats(List<ActivationSpan> spans, SolverData data, int vibesUsed) {
        var beatData = data.BeatData;
        double[] beatTimings = beatData.BeatTimings;

        if (beatTimings.Length <= 1)
            return;

        for (int i = 1; i < beatTimings.Length - 1; i++) {
            double beatTime = beatTimings[i];
            double endTimeWithPreviousBeatLength = beatData.GetTimeFromBeat(i + 1 + GetVibeExtensionForBeatLength(beatTime - beatTimings[i - 1], beatData.BeatDivisions));
            double endTimeWithNextBeatLength = beatData.GetTimeFromBeat(i + 1 + GetVibeExtensionForBeatLength(beatTimings[i + 1] - beatTime, beatData.BeatDivisions));
            int nextHitIndex = data.GetFirstHitIndexAfter(beatTime);
            double nextHitTime = data.GetHitTime(nextHitIndex);

            if (!(endTimeWithPreviousBeatLength >= nextHitTime ^ endTimeWithNextBeatLength >= nextHitTime))
                continue;

            int hitIndex = nextHitIndex;

            if (endTimeWithNextBeatLength < nextHitTime)
                hitIndex--;

            int endIndex;

            if (hitIndex < data.HitGroupCount && data.HitGroups[hitIndex].GivesVibe)
                endIndex = GetSpanStartingAfterHit(data, hitIndex, 1).EndIndex;
            else
                endIndex = hitIndex + 1;

            GetSpansWhereVibeHitsZeroAt(spans, data, beatTime, endIndex, vibesUsed);
        }
    }

    private static void GetSpansWhereVibeHitsZeroAt(List<ActivationSpan> spans, SolverData data, double vibeZeroTime, int endIndex, int vibesUsed) {
        double currentTime = vibeZeroTime;
        double vibeNeeded = 0d;
        int previousVibeIndex = data.GetPreviousVibe(data.GetFirstHitIndexAfter(currentTime));

        do {
            double previousVibeTime = data.GetHitTime(previousVibeIndex);
            double possibleStartTime = currentTime - (vibesUsed * VIBE_LENGTH - vibeNeeded);
            double needFullVibeAt = currentTime - (2d * VIBE_LENGTH - vibeNeeded);

            if (possibleStartTime > previousVibeTime)
                spans.Add(new ActivationSpan(possibleStartTime, data.GetFirstHitIndexAfter(possibleStartTime), endIndex));

            if (previousVibeTime < needFullVibeAt)
                break;

            vibeNeeded = Math.Min(vibeNeeded + (currentTime - previousVibeTime) - VIBE_LENGTH, 2d * VIBE_LENGTH);

            if (vibeNeeded <= 0d)
                break;

            currentTime = previousVibeTime;
            previousVibeIndex = data.GetPreviousVibe(previousVibeIndex);
        } while (previousVibeIndex >= 0);
    }

    private static double GetVibeExtensionForBeatLength(double beatLength, int beatDivisions) {
        double hitWindowInBeats = 0.175d / beatLength;

        return hitWindowInBeats - Math.Floor(hitWindowInBeats * beatDivisions) / beatDivisions;
    }

    private static Dictionary<int, BestNextActivations> GetBestNextActivationsByFirstVibeIndex(SolverData data, List<ActivationData> singleVibeActivations, List<ActivationData> doubleVibeActivations, out int totalScore) {
        var bestNextActivationsByFirstVibeIndex = new Dictionary<int, BestNextActivations>();

        totalScore = GetBestNextActivations(0).BestNextValue;

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

    private static (HashSet<int>, HashSet<int>) GetBestActivationIndices(SolverData data, List<ActivationData> singleVibeActivationData, List<ActivationData> doubleVibeActivationData, Dictionary<int, BestNextActivations> bestNextActivationsByFirstVibeIndex) {
        var bestSingleVibeActivationIndices = new HashSet<int>();
        var bestDoubleVibeActivationIndices = new HashSet<int>();

        Traverse(0);

        return (bestSingleVibeActivationIndices, bestDoubleVibeActivationIndices);

        void Traverse(int fromIndex) {
            var bestNextActivations = bestNextActivationsByFirstVibeIndex[data.GetNextVibe(fromIndex)];

            foreach (int index in bestNextActivations.BestNextSingleVibeActivations) {
                if (bestSingleVibeActivationIndices.Add(index))
                    Traverse(singleVibeActivationData[index].EndIndex);
            }

            foreach (int index in bestNextActivations.BestNextDoubleVibeActivations) {
                if (bestDoubleVibeActivationIndices.Add(index))
                    Traverse(doubleVibeActivationData[index].EndIndex);
            }
        }
    }

    private static Activation GetActivationFromData(SolverData data, List<ActivationData> activationData, int index) {
        var beatData = data.BeatData;
        var activation = activationData[index];
        double minStartTime = activation.MinStartTime;
        double maxStartTime = index < activationData.Count - 1 ? activationData[index + 1].MinStartTime : activation.MinStartTime + 1d;

        return new Activation(
            minStartTime,
            beatData.GetBeatFromTime(minStartTime),
            maxStartTime,
            beatData.GetBeatFromTime(maxStartTime),
            data.GetHitTime(activation.EndIndex - 1),
            data.GetHitBeat(activation.EndIndex - 1),
            activation.Score,
            false);
    }
}