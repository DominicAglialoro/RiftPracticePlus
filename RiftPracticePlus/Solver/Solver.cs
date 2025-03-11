using System;
using System.Collections.Generic;
using RiftEventCapture.Common;

namespace RiftPracticePlus;

public static class Solver {
    private const double VIBE_LENGTH = 5d;

    public static List<Note> Solve(CaptureResult captureResult) {
        var data = SolverData.CreateFromCaptureResult(captureResult);

        if (data.HitCount == 0)
            return new List<Note>();

        var spans = new List<ActivationSpan>();
        var activations = new List<Activation>();

        GetActivations(activations, spans, data, 1);
        GetActivations(activations, spans, data, 2);
        activations.Sort();

        var strategies = GetBestStrategies(data, activations);

        return GetVibeRanges(strategies);
    }

    private static void GetActivations(List<Activation> activations, List<ActivationSpan> spans, SolverData data, int vibesUsed) {
        int fromIndex = data.GetNextVibe(0);

        if (vibesUsed == 2)
            fromIndex = data.GetNextVibe(fromIndex + 1);

        if (fromIndex == data.HitCount)
            return;

        var currentSpan = GetNextSpanWithStartIndex(data, fromIndex + 1, data.GetHitTime(fromIndex), vibesUsed);

        spans.Add(currentSpan);

        while (currentSpan.StartIndex < data.HitCount) {
            var spanWithNewStartIndex = GetNextSpanWithStartIndex(data, currentSpan.StartIndex + 1, data.GetHitTime(currentSpan.StartIndex), vibesUsed);
            var spanWithNewEndIndex = GetNextSpanWithEndIndex(data, currentSpan.EndIndex + 1, currentSpan.StartTime, vibesUsed);

            if (spanWithNewStartIndex.StartTime < spanWithNewEndIndex.StartTime)
                currentSpan = spanWithNewStartIndex;
            else
                currentSpan = spanWithNewEndIndex;

            spans.Add(currentSpan);
        }

        if (spans.Count == 0)
            return;

        spans.Sort();

        var hits = data.Hits;
        int score = 0;
        int currentStartIndex = spans[0].StartIndex;
        int currentEndIndex = spans[0].StartIndex;

        for (int i = 0; i < spans.Count - 1; i++) {
            var span = spans[i];

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

            activations.Add(new Activation(span.StartTime, spans[i + 1].StartTime, span.StartIndex, span.EndIndex, score, vibesUsed));
        }

        spans.Clear();
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
        double endBeat = beatData.GetBeatFromTime(data.GetHitTime(endIndex - 1));
        double hitWindowInBeats = 0.175d / beatData.GetBeatLengthForBeat((int) endBeat);
        double vibeZeroTime = beatData.GetTimeFromBeat(endBeat - hitWindowInBeats + Math.Floor(hitWindowInBeats * beatData.BeatDivisions) / beatData.BeatDivisions);

        vibeZeroTime = Math.Max(vibeZeroTime, data.GetHitTime(endIndex - 2));

        double currentTime = vibeZeroTime;
        double vibeNeeded = 0d;
        int previousVibeIndex = data.GetPreviousVibe(data.GetFirstIndexAfter(vibeZeroTime));
        double startTime = double.MaxValue;

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

    private static List<Strategy> GetBestStrategies(SolverData data, List<Activation> activations) {
        int firstVibeIndex = data.GetNextVibe(0);
        int secondVibeIndex = data.GetNextVibe(firstVibeIndex + 1);
        var strategies = new Strategy[activations.Count];
        var bestNextStrategies = new List<Strategy>();
        int bestScore = 0;

        for (int i = strategies.Length - 1; i >= 0; i--) {
            var activation = activations[i];
            var strategy = ComputeStrategy(activation, i);

            strategies[i] = strategy;

            if (activation.VibesUsed == (activation.StartIndex <= secondVibeIndex ? 1 : 2))
                bestScore = Math.Max(bestScore, strategy.Score);
        }

        var overallBestStrategies = new List<Strategy>();

        for (int i = strategies.Length - 1; i >= 0; i--) {
            var strategy = strategies[i];
            var activation = strategy.Activation;

            if (activation.VibesUsed == (activation.StartIndex <= secondVibeIndex ? 1 : 2) && strategy.Score == bestScore)
                overallBestStrategies.Add(strategy);
        }

        return overallBestStrategies;

        Strategy ComputeStrategy(Activation activation, int index) {
            int firstVibeIndex = data.GetNextVibe(activation.EndIndex);
            int secondVibeIndex = data.GetNextVibe(firstVibeIndex + 1);
            int bestNextScore = 0;

            for (int i = strategies.Length - 1; i > index; i--) {
                var nextStrategy = strategies[i];
                var nextActivation = nextStrategy.Activation;

                if (nextActivation.StartIndex <= firstVibeIndex)
                    break;

                if (nextStrategy.Activation.VibesUsed == (nextActivation.StartIndex <= secondVibeIndex ? 1 : 2))
                    bestNextScore = Math.Max(bestNextScore, nextStrategy.Score);
            }

            for (int i = strategies.Length - 1; i > index; i--) {
                var nextStrategy = strategies[i];
                var nextActivation = nextStrategy.Activation;

                if (nextActivation.StartIndex <= firstVibeIndex)
                    break;

                if (nextStrategy.Activation.VibesUsed == (nextActivation.StartIndex <= secondVibeIndex ? 1 : 2) && nextStrategy.Score == bestNextScore)
                    bestNextStrategies.Add(nextStrategy);
            }

            var strategy = new Strategy(activation.Score + bestNextScore, activation, bestNextStrategies.ToArray());

            bestNextStrategies.Clear();

            return strategy;
        }
    }

    private static List<Note> GetVibeRanges(List<Strategy> strategies) {
        var viableStrategies = new HashSet<Strategy>();

        foreach (var strategy in strategies)
            Recurse(strategy);

        var vibeRanges = new List<Note>();

        foreach (var strategy in viableStrategies) {
            var activation = strategy.Activation;

            vibeRanges.Add(new Note((float) activation.MinStartTime, (float) activation.MaxStartTime, 0));
        }

        vibeRanges.Sort();

        return vibeRanges;

        void Recurse(Strategy strategy) {
            if (viableStrategies.Contains(strategy))
                return;

            viableStrategies.Add(strategy);

            foreach (var nextStrategy in strategy.NextStrategies)
                Recurse(nextStrategy);
        }
    }
}