using System;
using System.Collections.Generic;
using System.Drawing;

namespace RiftPracticePlus.Visualizer;

public class BarGraph : Drawable {
    private readonly float bottom;
    private readonly float top;
    private readonly float min;
    private readonly float max;
    private readonly Brush brush;
    private readonly IList<PointD> data;

    public BarGraph(float bottom, float top, float min, float max, Brush brush, IList<PointD> data) : base(data[0].X, data[data.Count - 1].X, DrawLayer.BarGraph) {
        this.bottom = bottom;
        this.top = top;
        this.min = min;
        this.max = max;
        this.brush = brush;
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

        var rects = new RectangleF[last - first];

        for (int i = first, j = 0; i < last; i++, j++) {
            var point = data[i];
            float startX = panel.TimeToX(point.X);
            float endX = panel.TimeToX(data[i + 1].X);
            float bottomY = panel.ValueToY(bottom);
            float topY = panel.ValueToY(bottom + (top - bottom) * ((float) point.Y - min) / (max - min));

            rects[j] = new RectangleF(startX, topY, Math.Max(1f, endX - startX), bottomY - topY);
        }

        if (rects.Length > 0)
            graphics.FillRectangles(brush, rects);
    }
}