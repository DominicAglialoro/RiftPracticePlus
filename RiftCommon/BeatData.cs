using System;

namespace RiftCommon;

public readonly struct BeatData {
    public bool HasBeatTimings => BeatTimings.Length > 0;

    public readonly float BPM;
    public readonly int BeatDivisions;
    public readonly double[] BeatTimings;

    public BeatData(float bpm, int beatDivisions, double[] beatTimings) {
        BPM = bpm;
        BeatDivisions = beatDivisions;
        BeatTimings = beatTimings;
    }

    public double GetBeatFromTime(double time) {
        if (double.IsPositiveInfinity(time))
            return double.PositiveInfinity;

        if (double.IsNegativeInfinity(time))
            return double.NegativeInfinity;

        if (BeatTimings.Length <= 1)
            return time / (60d / Math.Max(1, BPM)) + 1d;

        int beatIndex = GetBeatIndexFromTime(time);
        double previous = BeatTimings[beatIndex];
        double next = BeatTimings[beatIndex + 1];

        return beatIndex + 1 + (time - previous) / (next - previous);
    }

    public double GetTimeFromBeat(double beat) {
        if (double.IsPositiveInfinity(beat))
            return double.PositiveInfinity;

        if (double.IsNegativeInfinity(beat))
            return double.NegativeInfinity;

        if (BeatTimings.Length <= 1)
            return 60d / Math.Max(1, BPM) * (beat - 1d);

        if (beat <= 1d) {
            double first = BeatTimings[0];
            double second = BeatTimings[1];

            return first - (second - first) * (1d - beat);
        }

        if (beat < BeatTimings.Length) {
            double previous = BeatTimings[(int) beat - 1];
            double next = BeatTimings[(int) beat];

            return previous + (next - previous) * (beat % 1d);
        }

        double last = BeatTimings[BeatTimings.Length - 1];
        double secondToLast = BeatTimings[BeatTimings.Length - 2];

        return last + (last - secondToLast) * (beat - BeatTimings.Length);
    }

    public double GetTimeFromBeat(int beat) {
        if (BeatTimings.Length <= 1)
            return 60d / Math.Max(1, BPM) * (beat - 1);

        if (beat < 1) {
            double first = BeatTimings[0];
            double second = BeatTimings[1];

            return first - (second - first) * (1 - beat);
        }

        if (beat <= BeatTimings.Length)
            return BeatTimings[beat - 1];

        double last = BeatTimings[BeatTimings.Length - 1];
        double secondToLast = BeatTimings[BeatTimings.Length - 2];

        return last + (last - secondToLast) * (beat - BeatTimings.Length);
    }

    public double GetBeatLengthAtTime(double time) {
        if (BeatTimings.Length <= 1)
            return 60d / Math.Max(1, BPM);

        int beatIndex = GetBeatIndexFromTime(time);

        return BeatTimings[beatIndex + 1] - BeatTimings[beatIndex];
    }

    public double GetBeatLengthForBeat(int beat) {
        if (BeatTimings.Length <= 1)
            return 60d / Math.Max(1, BPM);

        if (beat < 1)
            return BeatTimings[1] - BeatTimings[0];

        if (beat < BeatTimings.Length)
            return BeatTimings[beat] - BeatTimings[beat - 1];

        return BeatTimings[BeatTimings.Length - 1] - BeatTimings[BeatTimings.Length - 2];
    }

    private int GetBeatIndexFromTime(double time) {
        int min = 0;
        int max = BeatTimings.Length - 1;

        while (max >= min) {
            int mid = (min + max) / 2;

            if (BeatTimings[mid] > time)
                max = mid - 1;
            else
                min = mid + 1;
        }

        return Math.Max(0, Math.Min(max, BeatTimings.Length - 2));
    }
}