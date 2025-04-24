using System.Collections.Generic;

namespace RiftPracticePlus;

public class SessionInfo {
    public string ChartName { get; }
    public string ChartID { get; }
    public int ChartDifficulty { get; }
    public IReadOnlyList<string> Pins { get; }

    public SessionInfo(string chartName, string chartID, int chartDifficulty, IReadOnlyList<string> pins) {
        ChartName = chartName;
        ChartID = chartID;
        ChartDifficulty = chartDifficulty;
        Pins = pins;
    }
}