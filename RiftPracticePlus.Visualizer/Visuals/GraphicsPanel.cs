using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RiftPracticePlus.Visualizer;

public class GraphicsPanel {
    private static readonly float SCROLL_SENSITIVITY = 0.5f;
    private static readonly float ZOOM_SENSITIVITY = 0.125f;
    private static readonly int MAX_ZOOM = 6;

    public event Action<double, float> OnClick;

    public event Action OnEnter;

    private double scroll;
    public double Scroll {
        get => scroll;
        set {
            scroll = value;
            RightBound = XToTime(panel.Width);
            Redraw();
        }
    }

    private float zoom;
    public float Zoom {
        get => zoom;
        set {
            zoom = value;
            zoomFactor = (float) Math.Pow(2f, zoom);
            horizontalScaleFactor = panel.Height * zoomFactor;
            RightBound = XToTime(panel.Width);
            Redraw();
        }
    }

    public double RightBound { get; private set; }

    public int PaddingX { get; }

    private int PaddingY { get; }

    public int Width => panel.Width - 2 * PaddingX;

    public int Height => panel.Height - 2 * PaddingY;

    private readonly Panel panel;

    private bool needsNewBuffer;
    private float zoomFactor;
    private float horizontalScaleFactor;
    private SortedDictionary<Drawable.DrawLayer, HashSet<Drawable>> layers;
    private BufferedGraphics buffer;

    public GraphicsPanel(Form1 form, Panel panel, int paddingX, int paddingY) {
        this.panel = panel;
        PaddingX = paddingX;
        PaddingY = paddingY;
        layers = new SortedDictionary<Drawable.DrawLayer, HashSet<Drawable>>();
        Zoom = 0f;

        panel.Paint += Panel_Paint;
        panel.MouseDown += Panel_Click;
        panel.MouseWheel += Panel_MouseWheel;
        panel.Resize += Panel_Resize;
        form.KeyDown += Form_KeyDown;
    }

    public void AddDrawable(Drawable drawable) {
        if (!layers.TryGetValue(drawable.Layer, out var drawables)) {
            drawables = new HashSet<Drawable>();
            layers.Add(drawable.Layer, drawables);
        }

        drawables.Add(drawable);
    }

    public void RemoveDrawable(Drawable drawable) {
        if (!layers.TryGetValue(drawable.Layer, out var drawables))
            return;

        drawables.Remove(drawable);

        if (drawables.Count == 0)
            layers.Remove(drawable.Layer);
    }

    public void Clear() {
        foreach (var layer in layers)
            layer.Value.Clear();

        layers.Clear();
    }

    public void Redraw() => panel.Invalidate();

    public float TimeToX(double time) => horizontalScaleFactor * (float) (time - scroll) + PaddingX;

    public double XToTime(float x) => (x - PaddingX) / horizontalScaleFactor + scroll;

    public float ValueToY(float value) => Height * value + PaddingY;

    public float YToValue(float y) => (y - PaddingY) / Height;

    private void Draw(Graphics graphics) {
        if (buffer == null || needsNewBuffer) {
            buffer?.Dispose();
            buffer = BufferedGraphicsManager.Current.Allocate(graphics, panel.Bounds);
            needsNewBuffer = false;
        }
        else {
            buffer.Render(graphics);
            buffer.Graphics.Clear(Color.Black);
        }

        foreach (var layer in layers) {
            foreach (var drawable in layer.Value) {
                if (drawable.Start < RightBound || drawable.End > Scroll)
                    drawable.Draw(this, buffer.Graphics);
            }
        }

        buffer.Render(graphics);
    }

    private void Panel_Paint(object sender, PaintEventArgs e) => Draw(e.Graphics);

    private void Panel_Click(object sender, MouseEventArgs e) => OnClick?.Invoke(XToTime(e.X), YToValue(e.Y));

    private void Panel_MouseWheel(object sender, MouseEventArgs e) {
        if (Control.ModifierKeys.HasFlag(Keys.Shift)) {
            float oldScaleFactor = horizontalScaleFactor;

            Zoom = Math.Max(-MAX_ZOOM, Math.Min(Zoom + ZOOM_SENSITIVITY * Math.Sign(e.Delta), MAX_ZOOM));
            Scroll = Math.Max(0f, Scroll + (e.X - PaddingX) / oldScaleFactor - (e.X - PaddingX) / horizontalScaleFactor);
        }
        else
            Scroll = Math.Max(0f, Scroll + SCROLL_SENSITIVITY * Math.Sign(e.Delta) / zoomFactor);
    }

    private void Form_KeyDown(object sender, KeyEventArgs e) {
        if (e.KeyCode == Keys.Enter)
            OnEnter?.Invoke();
    }

    private void Panel_Resize(object sender, EventArgs e) {
        needsNewBuffer = true;
        horizontalScaleFactor = Height * zoomFactor;
        RightBound = XToTime(Width);
        panel.Invalidate();
    }
}