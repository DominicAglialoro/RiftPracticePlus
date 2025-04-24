using System.Drawing;

namespace RiftPracticePlus.Visualizer;

public class HitMarker : Drawable {
    private readonly float y;
    private readonly bool givesVibe;
    private readonly Visualizer visualizer;

    public HitMarker(double x, float y, bool givesVibe, Visualizer visualizer) : base(x, x, DrawLayer.HitMarker) {
        this.y = y;
        this.givesVibe = givesVibe;
        this.visualizer = visualizer;
    }

    public override void Draw(GraphicsPanel panel, Graphics graphics) {
        float drawX = panel.TimeToX(Start);

        if (givesVibe)
            graphics.DrawLine(Pens.Gold, drawX, panel.ValueToY(1f), drawX, panel.ValueToY(0f));

        var path = visualizer.CurrentPath;
        bool isInSpan = path != null && Start >= path.StartTime && Start <= path.EndTime;

        if (y < 1f)
            graphics.FillRectangle(isInSpan ? Brushes.Red : Brushes.White, panel.TimeToX(Start) - 3, panel.ValueToY(y) - 3, 7, 7);
    }
}