using System.Drawing;

namespace RiftPracticePlus.Visualizer;

public class Label : Drawable {
    private static readonly Brush BRUSH = new SolidBrush(Color.Cyan);
    private static readonly Font FONT = new("Consolas", 12);

    private float x;
    private float y;
    private string label;

    public Label(float x, float y, string label) : base(0d, double.PositiveInfinity, DrawLayer.Label) {
        this.x = x;
        this.y = y;
        this.label = label;
    }

    public void SetLabel(string label) => this.label = label;

    public override void Draw(GraphicsPanel panel, Graphics graphics)
        => graphics.DrawString(label, FONT, BRUSH, x, y);
}