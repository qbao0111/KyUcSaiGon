# Nguyen Hue Promenade Floor Report

Scene: `Assets/Scenes/Scene_01_NguyenHue_Tutorial.unity`

## Gameplay Collider

- Object: `NguyenHue_GroundCollider`
- Parent: `NguyenHue_EnvironmentRoot/Ground_TileModules`
- Local position: `0, 0, -4`
- BoxCollider size: `42, 0.4, 92`
- Trigger: disabled
- Layer: Default
- Static: enabled

The collider covers the player spawn at `0, 1, -44` and the full main Nguyen Hue playable boulevard.

## Linear Boulevard Layout

The playable axis now follows the Z direction:

`PlayerSpawn -> Central Promenade -> Fountain Plaza -> CityHall Backdrop`

Key placement:

- `REPLACE_Player_Character`: `0, 1, -44`
- `PlayerSpawn`: `0, 1, -44`
- `REPLACE_Landmark_NguyenHue_Fountain`: `0, 0, 12`
- `REPLACE_Puzzle_SpeakerMixer`: `-8.5, 1, 5.5`
- `REPLACE_NPC_StreetMusician`: `-12.5, 1, 5.8`
- `REPLACE_LEDHint_Bass_Red_1`: `-10.5, 0, 8`
- `REPLACE_LEDHint_Mid_Green_6`: `10.5, 0, 13`
- `REPLACE_LEDHint_Treble_Gold_8`: `0, 0, 23`
- `Visual_REPLACE_CityHallBackdrop`: `0, 0, 39`

The fountain sits on the central axis, LED panels sit around the plaza without blocking the main path, and the CityHall backdrop anchors the far end of the boulevard.

## Visual Tile Coverage

Created/updated under `Ground_TileModules`:

- `MainPromenadeFloor`
- `CentralPathTiles`
- `SidePlazaTiles`
- `AccentGuideTiles`
- `FountainPlazaTiles`
- `DecorativeTileInlays`

The main tile grid covers roughly `X -20..20` and `Z -48..40`.

## Collision Setup

- Main visual tiles do not have colliders.
- The stable walkable collision comes from `NguyenHue_GroundCollider`.
- Decorative accents are visual only, except large stone edge blocks may keep simple colliders as boundary-like geometry.

## Old Floor Visuals

The old rough floor renderers are hidden:

- `REPLACE_Environment_NguyenHue_Boulevard`
- `REPLACE_Prop_NguyenHue_MainWalkingPath`
- `REPLACE_Prop_NguyenHue_FountainPlaza`

They are not deleted, so they can still be inspected or restored if needed.

## Materials

Created under `Assets/Art/Materials/NguyenHue/`:

- `M_NguyenHue_Pavement_LightGray`
- `M_NguyenHue_Pavement_DarkGray`
- `M_NguyenHue_Pavement_Seam`
- `M_NguyenHue_GoldAccent`
- `M_NguyenHue_StoneEdge`
- `M_NguyenHue_CityHall_WarmWhite`
- `M_NguyenHue_SideBuilding_Dark`

## CityHall Backdrop

Created under `NguyenHue_EnvironmentRoot/Landmark_Backdrop/Visual_REPLACE_CityHallBackdrop`:

- `Visual_REPLACE_CityHall_MainBlock`
- `Visual_REPLACE_CityHall_LeftWing`
- `Visual_REPLACE_CityHall_RightWing`
- `Visual_REPLACE_CityHall_Tower`
- `Visual_REPLACE_CityHall_RoofLine`
- `Visual_REPLACE_CityHall_Clock`
- `Visual_REPLACE_Backdrop_LeftMass`
- `Visual_REPLACE_Backdrop_RightMass`

These are primitive placeholder visuals only. Future FBX/GLB civic backdrop models should replace children under `Visual_REPLACE_CityHallBackdrop`, while gameplay objects stay untouched.

## Builder

The floor can be rebuilt from Unity menu:

`Ky Uc Sai Gon > Build Nguyen Hue Promenade Floor`
