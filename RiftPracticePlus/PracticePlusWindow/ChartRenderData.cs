using RiftCommon;

namespace RiftPracticePlus;

public class ChartRenderData {
    public readonly BeatData BeatData;
    public readonly ChartRenderHit[] Hits;
    public readonly ChartRenderActivation[] Activations;

    public ChartRenderData(BeatData beatData, ChartRenderHit[] hits, ChartRenderActivation[] activations) {
        Hits = hits;
        Activations = activations;
        BeatData = beatData;
    }
}