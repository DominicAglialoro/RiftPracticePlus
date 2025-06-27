namespace RiftCommon;

public readonly struct BPMChange {
    public readonly double Time;
    public readonly double Beat;
    public readonly float BPM;

    public BPMChange(double time, double beat, float bpm) {
        Time = time;
        Beat = beat;
        BPM = bpm;
    }
}