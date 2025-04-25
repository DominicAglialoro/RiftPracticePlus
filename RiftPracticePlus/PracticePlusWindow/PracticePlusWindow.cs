using System;
using UnityEngine;

namespace RiftPracticePlus;

public class PracticePlusWindow {
    private const int SIDE_PADDING = 8;
    private const int TOP_PADDING = 20;
    private static readonly string[] STARTING_VIBE_OPTIONS = { "0", "1", "2" };

    public int StartingVibe { get; private set; }

    public  Action OpenVisualizerClicked { get; set; }

    private Rect windowRect;

    private readonly RectInt drawSpaceRect;
    private readonly ChartRenderer chartRenderer;

    public PracticePlusWindow(int x, int y, int windowWidth, int windowHeight) {
        windowRect = new Rect(x, y, windowWidth, windowHeight);
        drawSpaceRect = new RectInt(SIDE_PADDING, TOP_PADDING, windowWidth - 2 * SIDE_PADDING, windowHeight - TOP_PADDING - SIDE_PADDING);
        chartRenderer = new ChartRenderer(new RectInt(drawSpaceRect.x, drawSpaceRect.y + 56, drawSpaceRect.width, drawSpaceRect.height - 28));
    }

    public void Render(ChartRenderParams chartRenderParams) {
        windowRect = GUI.Window(0, windowRect, _ => DrawWindow(chartRenderParams), "Practice Plus");

        var position = Vector2Int.FloorToInt(windowRect.position);

        if (position.x != Plugin.WindowPositionX.Value)
            Plugin.WindowPositionX.Value = position.x;

        if (position.y != Plugin.WindowPositionY.Value)
            Plugin.WindowPositionY.Value = position.y;
    }

    private void DrawWindow(ChartRenderParams chartRenderParams) {
        GUI.Label(new Rect(drawSpaceRect.x, drawSpaceRect.y, 100f, 20f), "Starting Vibe:");
        StartingVibe = GUI.Toolbar(new Rect(drawSpaceRect.xMax - 100f, drawSpaceRect.y, 100f, 20f), StartingVibe, STARTING_VIBE_OPTIONS);

        if (GUI.Button(new Rect(drawSpaceRect.x, drawSpaceRect.y + 28f, drawSpaceRect.width, 20f), "Open Visualizer"))
            OpenVisualizerClicked?.Invoke();

        chartRenderer.Render(chartRenderParams);
        GUI.DragWindow();
    }
}