namespace RiftPracticePlus;

public readonly struct ChartRenderParams {
    public readonly float Time;
    public readonly int FirstBeatIndex;
    public readonly int FirstNoteIndex;
    public readonly ChartRenderData RenderData;

    public ChartRenderParams(float time, int firstBeatIndex, int firstNoteIndex, ChartRenderData renderData) {
        Time = time;
        FirstBeatIndex = firstBeatIndex;
        FirstNoteIndex = firstNoteIndex;
        RenderData = renderData;
    }
}