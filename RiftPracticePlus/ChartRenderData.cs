﻿using System.Collections.Generic;
using RiftEventCapture.Common;

namespace RiftPracticePlus;

public class ChartRenderData {
    public readonly List<Note> Notes;
    public readonly List<Note> VibeRanges;
    public readonly BeatData BeatData;

    public ChartRenderData(List<Note> notes, List<Note> vibeRanges, BeatData beatData) {
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

        var vibeRanges = Solver.Solve(captureResult);

        return new ChartRenderData(notes, vibeRanges, captureResult.BeatData);
    }
}