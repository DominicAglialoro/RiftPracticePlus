using Shared;
using Shared.RhythmEngine;
using UnityEngine;

namespace RiftPracticePlus;

public class PracticePlusWindow : MonoBehaviour {
    private const float HIDE_CURSOR_AFTER_TIME = 2f;

    private BeatmapPlayer beatmapPlayer;
    private RenderData renderData;
    private ChartRenderer chartRenderer;
    private bool visible = true;
    private float mouseMovedAt;
    private int firstBeatIndex = 1;
    private int firstNoteIndex;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.P)) {
            visible = !visible;
            Cursor.visible = visible;
            mouseMovedAt = Time.time;
        }

        if (!visible || !GameWindow.Instance._isFocused)
            return;

        if (Input.GetAxis("Mouse X") != 0f || Input.GetAxis("Mouse Y") != 0f) {
            Cursor.visible = true;
            mouseMovedAt = Time.time;
        }
        else if (Cursor.visible && Time.time > mouseMovedAt + HIDE_CURSOR_AFTER_TIME)
            Cursor.visible = false;
    }

    private void OnGUI() {
        float time = (float) (beatmapPlayer._activeSpeedAdjustment * beatmapPlayer.FmodTimeCapsule.Time + beatmapPlayer._musicInitialSkippedTimeInSeconds);
        var beatData = renderData.BeatData;
        var notes = renderData.Notes;

        while (beatData.GetTimeFromBeat(firstBeatIndex) < time - 0.1f)
            firstBeatIndex++;

        while (firstNoteIndex < notes.Count && notes[firstNoteIndex].EndTime < time)
            firstNoteIndex++;

        if (visible)
            chartRenderer.Render(renderData, new RenderParams(time, firstBeatIndex, firstNoteIndex));
    }

    public void Init(BeatmapPlayer beatmapPlayer, RenderData renderData, ChartRenderer chartRenderer) {
        this.beatmapPlayer = beatmapPlayer;
        this.renderData = renderData;
        this.chartRenderer = chartRenderer;
        firstBeatIndex = 1;
        firstNoteIndex = 0;
        enabled = true;
    }
}