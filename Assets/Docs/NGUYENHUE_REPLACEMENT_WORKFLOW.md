# Scene_01_NguyenHue_Tutorial - Replacement Workflow

Last updated: 2026-06-04

## Goal

`Scene_01_NguyenHue_Tutorial` is organized so future Tencent-generated FBX/GLB models can replace placeholder visuals without breaking gameplay.

Keep gameplay scripts, colliders, triggers, and interactable components on stable root objects. Replace only visual children named `Visual_REPLACE_*`.

## Clean Scene Parents

Current hierarchy target:

```text
NguyenHue_EnvironmentRoot
├── Ground_TileModules
├── Fountain_Area
├── LED_HintPanels
├── StreetFurniture
├── Landmark_Backdrop
├── NPC_Area
├── Puzzle_Area
└── Interaction_Items
```

## Where To Drop Future Models

### Ground / Boulevard

Parent:
- `NguyenHue_EnvironmentRoot/Ground_TileModules`

Current placeholders:
- `REPLACE_Environment_NguyenHue_Boulevard`
- `REPLACE_Prop_NguyenHue_MainWalkingPath`
- `REPLACE_Prop_NguyenHue_FountainPlaza`

Future replacement:
- Replace visual mesh children only, or add final ground modules under this parent.
- Keep player spawn and gameplay triggers unchanged.

### Fountain

Parent:
- `NguyenHue_EnvironmentRoot/Fountain_Area`

Stable root:
- `REPLACE_Landmark_NguyenHue_Fountain`

Replaceable visual marker:
- `Visual_REPLACE_NguyenHue_Fountain`

Current visual children:
- `Visual_REPLACE_NguyenHue_Fountain_Base`
- `Visual_REPLACE_NguyenHue_Fountain_Water`

Future replacement:
- Drop the Tencent fountain FBX/GLB under `Visual_REPLACE_NguyenHue_Fountain`.
- Delete or disable only the old visual children.
- Do not delete `REPLACE_Landmark_NguyenHue_Fountain`.

### LED Hint Panels

Parent:
- `NguyenHue_EnvironmentRoot/LED_HintPanels`

Stable gameplay roots:
- `REPLACE_LEDHint_Bass_Red_1`
- `REPLACE_LEDHint_Mid_Green_6`
- `REPLACE_LEDHint_Treble_Gold_8`

Replaceable visual markers:
- `Visual_REPLACE_LED_HintPanel_01`
- `Visual_REPLACE_LED_HintPanel_02`
- `Visual_REPLACE_LED_HintPanel_03`

Future replacement:
- Replace only `Visual_REPLACE_LED_HintPanel_*` visuals.
- Keep the `REPLACE_LEDHint_*` roots because they hold LED hint interaction logic.
- Keep LED hint text readable unless the final model includes equivalent readable signage.

### Street Furniture

Parent:
- `NguyenHue_EnvironmentRoot/StreetFurniture`

Current placeholders include:
- `REPLACE_Prop_NguyenHue_LeftBench_*`
- `REPLACE_Prop_NguyenHue_RightBench_*`
- `REPLACE_Prop_NguyenHue_LeftPlanter_*`
- `REPLACE_Prop_NguyenHue_RightPlanter_*`
- `REPLACE_Prop_NguyenHue_LeftStreetLight_*`
- `REPLACE_Prop_NguyenHue_RightStreetLight_*`

Replacement marker:
- `Visual_REPLACE_PlanterBench`

Future replacement:
- Drop modular benches, planters, and lamps here.
- Keep colliders sensible so the player does not get stuck.

### Landmark / Backdrop

Parent:
- `NguyenHue_EnvironmentRoot/Landmark_Backdrop`

Current placeholders:
- `REPLACE_Landmark_NguyenHue_LeftBuilding_*`
- `REPLACE_Landmark_NguyenHue_RightBuilding_*`

Replacement marker:
- `Visual_REPLACE_CityHallBackdrop`

Future replacement:
- Drop city hall / building backdrop models under this parent.
- These should stay non-gameplay visuals.

### NPC Area

Parent:
- `NguyenHue_EnvironmentRoot/NPC_Area`

Stable container:
- `NPCRoot`

Important gameplay object:
- `REPLACE_NPC_StreetMusician`

Future replacement:
- Replace only the NPC visual child, for example `Visual_REPLACE_NPC_StreetMusician`.
- Do not delete `REPLACE_NPC_StreetMusician` because it holds NPC interaction data.

### Puzzle Area

Parent:
- `NguyenHue_EnvironmentRoot/Puzzle_Area`

Stable container:
- `PuzzleRoot`

Important gameplay object:
- `REPLACE_Puzzle_SpeakerMixer`

Current visual child:
- `Visual_REPLACE_Puzzle_SpeakerMixer`

Future replacement:
- Replace only `Visual_REPLACE_Puzzle_SpeakerMixer`.
- Do not remove `PuzzleInteractable` from `REPLACE_Puzzle_SpeakerMixer`.

### Interaction Items / Return

Parent:
- `NguyenHue_EnvironmentRoot/Interaction_Items`

Stable container:
- `BusStopRoot`

Important gameplay object:
- `REPLACE_BusStop_ReturnHub`

Current visual child:
- `Visual_REPLACE_BusStop_ReturnHub`

Future replacement:
- Replace only the bus stop visual child.
- Keep the `BusStopInteractable` root object and trigger/collider intact.

## Gameplay Objects To Preserve

Do not delete:
- `PlayerSpawn`
- `REPLACE_Player_Character`
- `REPLACE_NPC_StreetMusician`
- `REPLACE_Puzzle_SpeakerMixer`
- `REPLACE_LEDHint_Bass_Red_1`
- `REPLACE_LEDHint_Mid_Green_6`
- `REPLACE_LEDHint_Treble_Gold_8`
- `REPLACE_BusStop_ReturnHub`
- `MemoryZone_NguyenHue`
- `UI_Canvas`
- `EventSystem`
- `GameProgressManager`

## Quick Test After Replacement

1. Open `Scene_01_NguyenHue_Tutorial`.
2. Press Play.
3. Walk to each LED and press `E`.
4. Talk to `REPLACE_NPC_StreetMusician`.
5. Open speaker puzzle.
6. Solve `1-6-8`.
7. Confirm restore effect plays.
8. Confirm bus stop return works.
9. Confirm memory progress updates correctly.
