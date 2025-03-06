using UnityEngine;

namespace RiftPracticePlus;

public class ChartRenderer {
    private const int SIDE_PADDING = 8;
    private const int TOP_PADDING = 20;
    private const float BOTTOM_TIME = -0.1f;
    private const float TOP_TIME = 1f;
    private static readonly Color BACKING_COLOR = new(0f, 0f, 0f, 0.75f);
    private static readonly Color NOTE_COLOR = Color.white;
    private static readonly Color BEAT_GRID_COLOR = new(0.5f, 0.5f, 0.5f, 0.75f);
    private static readonly Color JUDGMENT_LINE_COLOR = Color.cyan;

    private Rect windowRect;

    private readonly RectInt canvasRect;
    private readonly Texture2D whiteTexture;

    public ChartRenderer(int windowWidth, int windowHeight) {
        windowRect = new Rect(0f, 0f, windowWidth, windowHeight);
        canvasRect = new RectInt(SIDE_PADDING, TOP_PADDING, windowWidth - 2 * SIDE_PADDING, windowHeight - TOP_PADDING - SIDE_PADDING);

        whiteTexture = new Texture2D(2, 2, TextureFormat.RGB24, false);
        whiteTexture.SetPixel(0, 0, Color.white);
        whiteTexture.SetPixel(1, 0, Color.white);
        whiteTexture.SetPixel(0, 1, Color.white);
        whiteTexture.SetPixel(1, 1, Color.white);
    }

    public void Render(RenderData renderData, RenderParams renderParams)
        => windowRect = GUI.Window(0, windowRect, _ => DrawWindow(renderData, renderParams), "Practice Plus");

    private void DrawWindow(RenderData renderData, RenderParams renderParams) {
        float time = renderParams.Time;

        DrawRect(0, 0, canvasRect.width, canvasRect.height, BACKING_COLOR);

        var beatData = renderData.BeatData;

        for (int i = renderParams.FirstBeatIndex;; i++) {
            double beatTime = beatData.GetTimeFromBeat(i);
            float timeDiff = (float) beatTime - time;

            if (timeDiff < BOTTOM_TIME)
                continue;

            if (timeDiff > TOP_TIME)
                break;

            DrawRect(0, TimeToY(timeDiff), canvasRect.width, 1, BEAT_GRID_COLOR);
        }

        DrawRect(0, TimeToY(0f), canvasRect.width, 1, JUDGMENT_LINE_COLOR);

        var notes = renderData.Notes;
        float maxTime = time + TOP_TIME;

        for (int i = renderParams.FirstNoteIndex; i < notes.Count; i++) {
            var note = notes[i];
            float timeDiff = note.StartTime - time;

            if (note.StartTime > maxTime)
                break;

            if (note.EndTime < time)
                continue;

            float x = note.Column / 3f;
            int startY = TimeToY(Mathf.Clamp(note.StartTime - time, 0f, TOP_TIME));

            DrawRect((int) (x * canvasRect.width) + 2, startY - 1, canvasRect.width / 3 - 4, 3, NOTE_COLOR);

            if (note.EndTime <= note.StartTime)
                continue;

            int endY = TimeToY(note.EndTime - time);

            DrawRect((int) (x * canvasRect.width + canvasRect.width / 6f) - 1, endY, 3, startY - endY, NOTE_COLOR);
        }

        GUI.DragWindow();
    }

    private void DrawRect(int x, int y, int width, int height, Color color)
        => GUI.DrawTexture(new Rect(x + canvasRect.x, y + canvasRect.y, width, height), whiteTexture, ScaleMode.StretchToFill, true, 0, color, 0, 0);

    private int TimeToY(float time) => Mathf.Clamp((int) ((time - TOP_TIME) / (BOTTOM_TIME - TOP_TIME) * canvasRect.height), 0, canvasRect.height);
}