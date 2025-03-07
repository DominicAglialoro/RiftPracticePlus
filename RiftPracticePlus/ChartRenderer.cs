using UnityEngine;

namespace RiftPracticePlus;

public class ChartRenderer {
    private const float BOTTOM_TIME = -0.1f;
    private const float TOP_TIME = 1f;
    private static readonly Color BACKING_COLOR = new(0f, 0f, 0f, 0.75f);
    private static readonly Color NOTE_COLOR = Color.white;
    private static readonly Color BEAT_GRID_COLOR = new(0.5f, 0.5f, 0.5f, 0.75f);
    private static readonly Color JUDGMENT_LINE_COLOR = Color.cyan;

    private readonly RectInt rect;
    private readonly Texture2D whiteTexture;

    public ChartRenderer(RectInt rect) {
        this.rect = rect;
        whiteTexture = new Texture2D(2, 2, TextureFormat.RGB24, false);
        whiteTexture.SetPixel(0, 0, Color.white);
        whiteTexture.SetPixel(1, 0, Color.white);
        whiteTexture.SetPixel(0, 1, Color.white);
        whiteTexture.SetPixel(1, 1, Color.white);
    }

    public void Render(ChartRenderParams chartRenderParams) {
        var chartRenderData = chartRenderParams.ChartRenderData;
        float time = chartRenderParams.Time;

        DrawRect(0, 0, rect.width, rect.height, BACKING_COLOR);

        var beatData = chartRenderData.BeatData;

        for (int i = chartRenderParams.FirstBeatIndex;; i++) {
            double beatTime = beatData.GetTimeFromBeat(i);
            float timeDiff = (float) beatTime - time;

            if (timeDiff < BOTTOM_TIME)
                continue;

            if (timeDiff > TOP_TIME)
                break;

            DrawRect(0, TimeToY(timeDiff), rect.width, 1, BEAT_GRID_COLOR);
        }

        DrawRect(0, TimeToY(0f), rect.width, 1, JUDGMENT_LINE_COLOR);

        var notes = chartRenderData.Notes;
        float maxTime = time + TOP_TIME;

        for (int i = chartRenderParams.FirstNoteIndex; i < notes.Count; i++) {
            var note = notes[i];

            if (note.StartTime > maxTime)
                break;

            if (note.EndTime < time)
                continue;

            float x = note.Column / 3f;
            int startY = TimeToY(Mathf.Clamp(note.StartTime - time, 0f, TOP_TIME));

            DrawRect((int) (x * rect.width) + 2, startY - 1, rect.width / 3 - 4, 3, NOTE_COLOR);

            if (note.EndTime <= note.StartTime)
                continue;

            int endY = TimeToY(note.EndTime - time);

            DrawRect((int) (x * rect.width + rect.width / 6f) - 1, endY, 3, startY - endY, NOTE_COLOR);
        }
    }

    private void DrawRect(int x, int y, int width, int height, Color color)
        => GUI.DrawTexture(new Rect(x + rect.x, y + rect.y, width, height), whiteTexture, ScaleMode.StretchToFill, true, 0, color, 0, 0);

    private int TimeToY(float time) => Mathf.Clamp((int) ((time - TOP_TIME) / (BOTTOM_TIME - TOP_TIME) * rect.height), 0, rect.height);
}