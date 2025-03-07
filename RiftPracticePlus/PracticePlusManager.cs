using Shared;
using Shared.RhythmEngine;
using UnityEngine;

namespace RiftPracticePlus;

public class PracticePlusManager : MonoBehaviour {
    private const float HIDE_CURSOR_AFTER_TIME = 2f;

    private BeatmapPlayer beatmapPlayer;
    private ChartRenderData chartRenderData;
    private PracticePlusWindow practicePlusWindow;
    private float mouseMovedAt;
    private int firstBeatIndex = 1;
    private int firstNoteIndex;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.P)) {
            Plugin.ShowPracticePlusWindow.Value = !Plugin.ShowPracticePlusWindow.Value;
            Cursor.visible = Plugin.ShowPracticePlusWindow.Value;
            mouseMovedAt = Time.time;
        }

        if (!Plugin.ShowPracticePlusWindow.Value || !GameWindow.Instance._isFocused)
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
        var beatData = chartRenderData.BeatData;
        var notes = chartRenderData.Notes;

        while (beatData.GetTimeFromBeat(firstBeatIndex) < time - 0.1f)
            firstBeatIndex++;

        while (firstNoteIndex < notes.Count && notes[firstNoteIndex].EndTime < time)
            firstNoteIndex++;

        if (Plugin.ShowPracticePlusWindow.Value)
            practicePlusWindow.Render(new ChartRenderParams(time, firstBeatIndex, firstNoteIndex, chartRenderData));
    }

    public void Init(BeatmapPlayer beatmapPlayer, ChartRenderData chartRenderData, PracticePlusWindow practicePlusWindow) {
        this.beatmapPlayer = beatmapPlayer;
        this.chartRenderData = chartRenderData;
        this.practicePlusWindow = practicePlusWindow;
        firstBeatIndex = 1;
        firstNoteIndex = 0;
        enabled = true;
    }
}