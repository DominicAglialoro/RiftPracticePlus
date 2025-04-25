# Rift Practice Plus

This is a mod that extends the features of Rift of the Necrodancer's Practice Mode

## To Use

1. Download the lastest version of BepInEx at <https://github.com/BepInEx/BepInEx/releases/latest>
2. Extract the contents of the BepInEx zip archive to your Rift of the Necrodancer game directory (C:\Program Files (x86)\Steam\steamapps\common\RiftOfTheNecroDancerOSTVolume1)
3. Open Rift of the Necrodancer to generate BepInEx config files
4. Download the latest version of RiftPracticePlus at <https://github.com/DominicAglialoro/RiftPracticePlus/releases/latest>
5. Extract the contents of the RiftPracticePlus zip archive to your BepInEx plugins folder (C:\Program Files (x86)\Steam\steamapps\common\RiftOfTheNecroDancerOSTVolume1\BepInEx\plugins)
6. For custom charts, play through a chart in its entirety with the Golden Lute modifier enabled to generate a binary file with the data to be displayed in practice mode. For all base game Impossible charts, this data is included with each release of the mod
7. Open a chart in practice mode. Practice Plus should open the associated binary file automatically and display a window indicating the timings of every note in the chart
8. Press P to show or hide the window. You can use your mouse to drag the window to another part of the screen

## Binary File Format

The generated binary files consist of the following values, with the specified types:

* The string "RIFT_CHART_DATA" (string)
* The version number of the file format (int)
* The name of the chart (string)
* The level ID of the chart (string)
* The difficulty level of the chart (int), with:\
1 = Easy, \
2 = Medium, \
3 = Hard, \
4 = Impossible
* Whether the chart is a custom chart (bool)
* The BPM of the chart (float)
* The number of divisions per beat (int)
* The number of beat timings stored in the chart (int)
* For each beat timing, the value of that timing (double)
* The number of captured hits (int)
* For each captured hit:
  * The time of the hit (double)
  * The beat of the hit (double)
  * The end time of the hit, for wyrms (double)
  * The end beat of the hit (double)
  * The type of the enemy hit (int), with: \
    0 = None, \
    1 = Green Slime, \
    2 = Blue Slime, \
    3 = Yellow Slime, \
    4 = Blue Bat, \
    5 = Yellow Bat, \
    6 = Red Bat, \
    7 = Green Zombie, \
    8 = Blue Zombie, \
    9 = Red Zombie, \
    10 = White Skeleton, \
    11 = White Shield Skeleton, \
    12 = White Double Shield Skeleton, \
    13 = Yellow Skeleton, \
    14 = Yellow Shield Skeleton, \
    15 = Black Skeleton, \
    16 = Black Shield Skeleton, \
    17 = Blue Armadillo, \
    18 = Red Armadillo, \
    19 = Yellow Armadillo, \
    20 = Wyrm, \
    21 = Green Harpy, \
    22 = Blue Harpy, \
    23 = Red Harpy, \
    24 = Blademaster, \
    25 = Blue Blademaster, \
    26 = Yellow Blademaster, \
    27 = White Skull, \
    28 = Blue Skull, \
    29 = Red Skull, \
    30 = Apple, \
    31 = Cheese, \
    32 = Drumstick, \
    33 = Ham
  * The column the enemy is in (int)
  * The score value of the hit (base score * multiplier at that point, no true perfect or vibe bonus) (int)
  * Whether this hit is an instance of gaining Vibe (bool). Note that Vibe gains are captured as separate hits
* The maximum point bonus to be gained via Vibe (int)
* The number of possible single-Vibe activations found (int)
* For each single-Vibe activation:
  * The earliest possible start time for the activation (double)
  * The latest possible start time for the activation (double)
  * The time of the last hit during the activation (double)
  * The total score bonus gained from the activation (int)
  * Whether the activation is part of the optimal Vibe path (bool)
* The number of possible double-Vibe activations found (int)
* For each double-Vibe activation, the same data listed above
