namespace RiftPracticePlus;

public readonly struct RenderParams {
    public readonly float Time;
    public readonly int FirstBeatIndex;
    public readonly int FirstNoteIndex;

    public RenderParams(float time, int firstBeatIndex, int firstNoteIndex) {
        Time = time;
        FirstBeatIndex = firstBeatIndex;
        FirstNoteIndex = firstNoteIndex;
    }
}