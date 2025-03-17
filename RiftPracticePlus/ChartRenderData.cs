using System;
using System.Collections.Generic;
using RiftEventCapture.Common;
using RiftVibeSolver.Common;

namespace RiftPracticePlus;

public class ChartRenderData {
    public readonly Note[] Notes;
    public readonly Note[] VibeRanges;
    public readonly BeatData BeatData;

    public ChartRenderData(Note[] notes, Note[] vibeRanges, BeatData beatData) {
        Notes = notes;
        VibeRanges = vibeRanges;
        BeatData = beatData;
    }

    public static ChartRenderData CreateFromCaptureResult(CaptureResult captureResult) {
        var notes = new List<Note>();
        var riftEvents = captureResult.RiftEvents;

        for (int i = 0; i < riftEvents.Count; i++) {
            var riftEvent = riftEvents[i];

            if (riftEvent.EventType != EventType.EnemyHit)
                continue;

            float startTime = (float) riftEvent.TargetTime.Time;
            int column = riftEvent.Column;

            if (riftEvent.EnemyType != EnemyType.Wyrm) {
                notes.Add(new Note(startTime, startTime, column));

                continue;
            }

            float endTime = startTime;

            for (int j = i + 1; j < riftEvents.Count; j++) {
                var endEvent = riftEvents[j];

                if (endEvent.EventType != EventType.HoldComplete || endEvent.Column != column || endEvent.TargetTime.Time <= startTime)
                    continue;

                endTime = (float) endEvent.TargetTime.Time;

                break;
            }

            notes.Add(new Note(startTime, endTime, column));
        }

        notes.Sort();

        var beatData = captureResult.BeatData;
        var hits = Hit.MergeHits(GetHitsFromCaptureResult(captureResult));
        var result = Solver.Solve(new SolverData(beatData.BPM, beatData.BeatDivisions, new List<double>(beatData.BeatTimings), hits));
        var singleVibeActivations = result.BestSingleVibeActivations;
        var doubleVibeActivations = result.BestDoubleVibeActivations;
        var allActivations = new Note[singleVibeActivations.Count + doubleVibeActivations.Count];
        int index = 0;

        foreach (var activation in singleVibeActivations) {
            allActivations[index] = new Note((float) activation.MinStartTime, (float) activation.MaxStartTime, 1);
            index++;
        }

        foreach (var activation in doubleVibeActivations) {
            allActivations[index] = new Note((float) activation.MinStartTime, (float) activation.MaxStartTime, 2);
            index++;
        }

        Array.Sort(allActivations);

        return new ChartRenderData(notes.ToArray(), allActivations, captureResult.BeatData);
    }

    private static IEnumerable<Hit> GetHitsFromCaptureResult(CaptureResult captureResult) {
        foreach (var riftEvent in captureResult.RiftEvents) {
            if (riftEvent.EventType is not (EventType.EnemyHit or EventType.VibeGained))
                continue;

            switch (riftEvent.EventType) {
                case EventType.EnemyHit:
                    yield return new Hit(riftEvent.TargetTime.Time, riftEvent.BaseMultiplier * riftEvent.BaseScore, false);
                    break;
                case EventType.VibeGained:
                    yield return new Hit(riftEvent.TargetTime.Time, 0, true);
                    break;
            }
        }
    }
}