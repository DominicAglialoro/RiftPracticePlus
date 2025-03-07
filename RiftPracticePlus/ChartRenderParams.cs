namespace RiftPracticePlus;

public readonly struct ChartRenderParams {
    public readonly float Time;
    public readonly int FirstBeatIndex;
    public readonly int FirstNoteIndex;
    public readonly ChartRenderData ChartRenderData;

    public ChartRenderParams(float time, int firstBeatIndex, int firstNoteIndex, ChartRenderData chartRenderData) {
        Time = time;
        FirstBeatIndex = firstBeatIndex;
        FirstNoteIndex = firstNoteIndex;
        ChartRenderData = chartRenderData;
    }
}