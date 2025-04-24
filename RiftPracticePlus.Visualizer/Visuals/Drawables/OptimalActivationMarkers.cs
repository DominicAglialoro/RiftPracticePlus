using System.Collections.Generic;
using System.Drawing;

namespace RiftPracticePlus.Visualizer;

public class OptimalActivationMarkers : Drawable {
    private static readonly Brush BRUSH = new SolidBrush(Color.Gold);

    private readonly float bottom;
    private readonly float top;
    private readonly float min;
    private readonly float max;

    private IList<(PointD, double)> data;

    public OptimalActivationMarkers(float bottom, float top, float min, float max, IList<(PointD, double)> data) : base(data[0].Item1.X - data[0].Item2, data[data.Count - 1].Item1.X, DrawLayer.LineGraph) {
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
            (var point, double endTime) = data[i];

            if (point.X < panel.Scroll)
                first = i;
            else if (endTime > panel.RightBound) {
                last = i;

                break;
            }
        }

        var rects = new RectangleF[last - first + 1];

        for (int i = first, j = 0; i <= last; i++, j++) {
            (var point, double endTime) = data[i];
            float startX = panel.TimeToX(point.X);
            float width = panel.TimeToX(endTime) - startX;
            float y = panel.ValueToY(bottom + (top - bottom) * ((float) point.Y - min) / (max - min));

            if (width < 9f) {
                startX -= 0.5f * (9f - width);
                width = 9f;
            }

            rects[j] = new RectangleF(startX, y, width, 9f);
        }

        graphics.FillRectangles(BRUSH, rects);
    }
}