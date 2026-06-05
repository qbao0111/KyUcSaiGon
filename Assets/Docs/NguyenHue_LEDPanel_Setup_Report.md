# Nguyen Hue LED Panel Setup Report

Scene: `Assets/Scenes/Scene_01_NguyenHue_Tutorial.unity`

## Summary

The Nguyen Hue LED hint panels now use one reusable prefab:

`Assets/Art/Prefabs/NguyenHue/PF_NguyenHue_LEDHintPanel.prefab`

The source model is very small, so scale compensation is handled inside the prefab visual child instead of leaving scene instances at large scales.

## Final Prefab Setup

- Prefab root: `PF_NguyenHue_LEDHintPanel`
- Root position: `0, 0, 0`
- Root rotation: `0, 0, 0`
- Root scale: `1, 1, 1`
- Visual child: `Visual_REPLACE_LEDHintPanel_Model`
- Visual child scale: `300, 300, 300`
- Screen color patch: `ScreenColorPatch`, disabled by default

Configured target bounds in scene:

- Width: about `1.5 - 2.2` Unity units
- Height: about `2.5 - 3.2` Unity units
- Depth: about `0.15 - 0.45` Unity units

Note: the model source scale is tiny. The prefab compensates for this once internally so scene instances can stay at normal scale.

## Materials

Material folder:

`Assets/Art/Materials/NguyenHue/`

Materials:

- `M_LEDPanel_Frame_Dark.mat`
- `M_LEDPanel_Screen_Green.mat`
- `M_LEDPanel_Screen_Yellow.mat`
- `M_LEDPanel_Screen_Red.mat`

For now `ScreenColorPatch` is disabled to avoid the large colored-plane issue. The color distinction is preserved through TMP labels and material variants kept for future screen-mesh assignment.

## Scene Instances

There are three main LED hint objects under `LED_HintPanels`:

- `REPLACE_LEDHint_Bass_Red_1`
  - Position: `-18, 0, -18`
  - Scene visual scale: `1, 1, 1`
  - Label: `BASS`
  - Number: `1`
  - Dialogue: `LED đỏ nhấp nháy số 1. Đây là Bass.`

- `REPLACE_LEDHint_Mid_Green_6`
  - Position: `18, 0, -10`
  - Scene visual scale: `1, 1, 1`
  - Label: `MID`
  - Number: `6`

- `REPLACE_LEDHint_Treble_Gold_8`
  - Position: `10, 0, 12`
  - Scene visual scale: `1, 1, 1`
  - Label: `TREBLE`
  - Number: `8`

The current Nguyen Hue puzzle answer is `1-6-8`, matching the current scene labels.

## Text Placement

Each LED display is managed by `LEDHintInteractable`:

- Parent: `ScreenRoot`, local to the LED panel root
- Screen patch: `ScreenQuad`, local position `0, 2.023, -0.18`, local scale `1.18, 1.48, 0.025`, no collider
- Label: `TMP_LED_Label`, local position `0, 1.98, -0.215`, font size `6.5`
- Number: `TMP_LED_Number`, local position `0, 1.27, -0.225`, font size `22`, local scale `0.34`
- Number pulse: driven at runtime by `blinkSpeed`

This keeps the hint text attached to the kiosk instead of floating above the boulevard.

Inspector editing:

- Keep `Auto Layout` enabled to edit layout from the `LEDHintInteractable` fields.
- Disable `Auto Layout` if you want to move `ScreenRoot`, `ScreenQuad`, `TMP_LED_Label`, or `TMP_LED_Number` directly in the hierarchy without the script resetting them on Play.

## Interaction Colliders

Each LED hint root keeps its gameplay interaction component and collider.

Collider settings:

- Trigger: enabled
- Size: `2.2, 3, 1.4`
- Center: `0, 1.35, 0`

This keeps interaction available without blocking the player path.

## Gameplay Safety

Preserved:

- `LEDHintInteractable`
- TMP labels
- existing LED hint dialogue flow
- Nguyen Hue puzzle object and answer
- scene restoration flow

Changed only:

- LED panel visual prefab scale
- scene LED instance scale/placement
- interaction collider size
- broken color patch behavior

## Remaining Visual Notes

If the imported model has a separate screen mesh later, assign the matching screen material directly to that mesh and keep `ScreenColorPatch` disabled or deleted.

Do not scale scene instances to `100`. Keep scene instance scale near `1, 1, 1`.
