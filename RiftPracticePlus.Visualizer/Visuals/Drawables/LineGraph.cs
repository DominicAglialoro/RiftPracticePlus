using System.Collections.Generic;
using System.Drawing;

namespace RiftPracticePlus.Visualizer;

public class LineGraph : Drawable {
    private readonly float bottom;
    private readonly float top;
    private readonly float min;
    private readonly float max;
    private readonly IList<PointD> data;

    public LineGraph(float bottom, float top, float min, float max, IList<PointD> data) : base(data[0].X, data[data.Count - 1].X, DrawLayer.LineGraph) {
        this.bottom = bottom;
        this.top = top;
        this.min = min;
        this.max = max;
        this.data = data;
    }

    public override void Draw(GraphicsPanel panel, Graphics graphics) {
        int first = 0;
        int last = data.Count - 1;

        for (int i = 0; i < data.Count; i++) {
            var point = data[i];

            if (point.X < panel.Scroll)
                first = i;
            else if (point.X > panel.RightBound) {
                last = i;

                break;
            }
        }

        var points = new PointF[last - first + 1];

        for (int i = first, j = 0; i <= last; i++, j++) {
            var point = data[i];

            points[j] = new PointF(panel.TimeToX(point.X), panel.ValueToY(bottom + (top - bottom) * ((float) point.Y - min) / (max - min)));
        }

        if (points.Length > 1)
            graphics.DrawLines(Pens.Red, points);
    }
}