# Ky Uc Sai Gon - Project Overview

Last updated: 2026-06-04

## 1. Project Summary

`Ky Uc Sai Gon` is a Unity 6 URP third-person narrative walking simulator prototype.

The player starts from a memory bus hub, travels to symbolic Ho Chi Minh City locations, restores each memory location by solving a simple puzzle, collects 6 memory fragments, then unlocks the ending scene with Landmark 81.

This is still a playable graybox/blockout prototype. Most locations use Unity primitives and clear placeholder naming so final Tencent-generated models can replace visuals later without rewriting gameplay logic.

## 2. Current Gameplay Loop

1. Start in `Scene_00_BusHub`.
2. Interact with the route board.
3. Open the paper route map UI.
4. Select an unlocked location.
5. In the location scene:
   - Talk to NPC or collect required memory item.
   - Open puzzle.
   - Solve correct answer.
   - Location restores color/light/sound.
   - Memory fragment is collected.
   - Bus stop return trigger appears.
6. Return to BusHub.
7. After 6/6 memory fragments, open `Scene_07_Ending`.

## 3. Main Scenes

| Scene | Purpose | Memory Fragment | Puzzle Answer |
| --- | --- | --- | --- |
| `Scene_00_BusHub` | Central route hub inside vintage bus | None | Route selection |
| `Scene_01_NguyenHue_Tutorial` | Tutorial boulevard / street music memory | Nhip song tre | `1-6-8` |
| `Scene_02_BenThanh` | Market plaza memory | Doi song thuong ngay | `1-3-2` |
| `Scene_03_DinhDocLap` | Historical courtyard memory | Lich su | `1975` |
| `Scene_04_NhaThoDucBa` | Cathedral square memory | Binh yen | `La-Sol-Re-Mi-Si-Do` |
| `Scene_05_Bitexco` | Modern city transition memory | Chuyen minh | `345` |
| `Scene_06_BachDang` | Riverside flow memory | Dong chay thanh pho | `28` |
| `Scene_07_Ending` | Final future memory / Landmark 81 | Ending | Requires 6/6 fragments |

Vietnamese diacritics are used in-game through TextMeshPro. Some internal docs keep ASCII names for Git/editor safety.

## 4. Player And Camera

Current controller is third-person.

Main scripts:
- `Assets/Scripts/Player/ThirdPersonPlayerController.cs`
- `Assets/Scripts/Player/ThirdPersonCamera.cs`
- `Assets/Scripts/Player/PlayerMovementAnimator.cs`

Current player visual:
- P09 Modular Humanoid model is used as the player character visual.
- Root object should remain stable as `REPLACE_Player_Character`.
- Gameplay components/colliders should stay on stable root objects.
- Visual model can be replaced later without changing player logic.

Controls:
- `WASD`: move relative to camera direction.
- Mouse: orbit camera.
- `E`: interact.
- `Esc`: close route map / UI panels when supported.
- `Enter` or `E`: confirm route selection in paper map UI.

## 5. Progress And Unlock Logic

Main scripts:
- `Assets/Scripts/Core/GameProgressManager.cs`
- `Assets/Scripts/Core/LocationId.cs`
- `Assets/Scripts/Core/DeveloperMode.cs`
- `Assets/Scripts/Core/SceneLoader.cs`

Progress is runtime-only and persists between scenes through `DontDestroyOnLoad`.

Tracked state:
- `memoryFragmentsCollected`
- restored state for 6 locations
- `busHubUnlocked`
- `endingUnlocked`
- `developerMode`

Normal route unlock order:
1. Nguyen Hue
2. Ben Thanh
3. Dinh Doc Lap
4. Nha Tho Duc Ba
5. Bitexco
6. Bach Dang
7. Ending after all 6 restored

Developer Mode:
- Menu path: `Ky Uc Sai Gon`
- When enabled, all route nodes and ending route are unlocked for testing.
- When disabled, game follows normal progression.

## 6. Interaction System

Main scripts:
- `Assets/Scripts/Interaction/IInteractable.cs`
- `Assets/Scripts/Interaction/Interactor.cs`
- `Assets/Scripts/Interaction/NPCInteractable.cs`
- `Assets/Scripts/Interaction/ItemInteractable.cs`
- `Assets/Scripts/Interaction/PuzzleInteractable.cs`
- `Assets/Scripts/Interaction/BusStopInteractable.cs`
- `Assets/Scripts/Interaction/BusHubMapBoardInteractable.cs`
- `Assets/Scripts/Interaction/LEDHintInteractable.cs`

Interaction design:
- Player approaches/faces an interactable.
- UI prompt appears.
- Press `E` to interact.
- Most gameplay scripts stay on root objects.
- Visual meshes should stay under `Visual_REPLACE_*` children when possible.

## 7. UI Systems

Main scripts:
- `Assets/Scripts/UI/UIManager.cs`
- `Assets/Scripts/UI/BusHubMapUIController.cs`
- `Assets/Scripts/UI/BusHubRouteMapUI.cs`
- `Assets/Scripts/UI/BusHubRouteButtonUI.cs`
- `Assets/Scripts/UI/RouteMapNodeUI.cs`
- `Assets/Scripts/UI/WorldLabelVisibility.cs`

Current UI includes:
- Memory progress text.
- Current objective text.
- Interaction prompt.
- Dialogue/puzzle panels.
- BusHub paper route map overlay.

BusHub route selection now uses the paper map UI:
- Stand near route board.
- Press `E`.
- Select route with mouse, WASD, or arrow keys.
- Confirm with `Enter` or `E`.
- Close with `Esc`.

## 8. Restoration System

Main scripts:
- `Assets/Scripts/Restoration/MemoryZoneController.cs`
- `Assets/Scripts/Restoration/RestorableEffect.cs`
- `Assets/Scripts/Restoration/MaterialRestoreEffect.cs`
- `Assets/Scripts/Restoration/LightRestoreEffect.cs`
- `Assets/Scripts/Restoration/ParticleRestoreEffect.cs`
- `Assets/Scripts/Restoration/AudioRestoreEffect.cs`

Special small effects:
- `BoatBobEffect.cs`
- `PigeonRiseEffect.cs`
- `BitexcoTowerLightStrip.cs`

Restoration goal:
- Start gray/silent.
- After puzzle success, restore color, light, particles, audio, and progress.

## 9. BusHub Current State

`Scene_00_BusHub` is the central hub.

Current BusHub features:
- Vintage bus interior blockout.
- Primitive shell: floor, walls, ceiling, windshield, side windows.
- Seat rows, handrails, warm ceiling lights.
- Route board at the front.
- Paper route map overlay UI for destination selection.
- P09 player model inside bus.

Replacement workflow:
- Stable gameplay roots should keep scripts/colliders.
- Visual children should use `Visual_REPLACE_*` naming.
- FBX bus props are stored under:
  - `Assets/Art/Models/BusHub/`
  - `Assets/Art/Prefabs/BusHub/`
  - `Assets/Art/Materials/BusHub/`

BusHub tooling:
- `Assets/Scripts/Editor/BusHubFbxInteriorInstaller.cs`
- Menu tools can regenerate/apply BusHub FBX interior props.

## 10. Placeholder And Replacement Rules

Current project is not final art.

Keep this rule when editing scenes:
- Gameplay script and collider on stable root object.
- Replaceable mesh/model under `Visual_REPLACE_*`.
- For important objects, use names like:
  - `REPLACE_Landmark_*`
  - `REPLACE_NPC_*`
  - `REPLACE_Item_*`
  - `REPLACE_Puzzle_*`
  - `REPLACE_BusStop_*`
  - `Visual_REPLACE_*`

Do not delete root gameplay objects unless replacing the logic intentionally.

## 11. Important Editor Tools

Menu: `Ky Uc Sai Gon`

Known editor utility scripts:
- `KyUcSaiGonSceneGenerator.cs`: generates/rebuilds prototype scene blockouts.
- `KyUcSaiGonDeveloperMenu.cs`: toggles Developer Mode.
- `KyUcSaiGonPlayerModelMenu.cs`: player model/material helper tools.
- `BusHubFbxInteriorInstaller.cs`: BusHub interior FBX prop installer.

Use these carefully because generator tools can overwrite scene layout work.

## 12. Main Test Checklist

Smoke test:
1. Open `Scene_00_BusHub`.
2. Press Play.
3. Walk inside the bus.
4. Open route map with `E`.
5. Select a route.
6. Verify scene loads.
7. Interact with NPC/item/puzzle.
8. Solve puzzle.
9. Verify restore + memory progress.
10. Return to BusHub.

Full normal progression:
1. Turn Developer Mode OFF.
2. Start BusHub.
3. Complete all 6 memory locations in order.
4. Verify ending only opens after `6/6`.
5. Finish ending and return to BusHub.

Dev test:
1. Turn Developer Mode ON.
2. Open BusHub route map.
3. Verify all routes and Dev Test Ending are available.
4. Jump to specific scenes for quick QA.

## 13. Known Project Notes

- `SampleScene` still exists but is not part of the main gameplay loop.
- `Assets/P09_Modular_Humanoid` contains imported character/demo assets.
- `Assets/TextMesh Pro` contains TextMeshPro resources and examples.
- Some older or rejected BusHub objects may remain disabled in scene hierarchy for recovery/reference.
- This project currently does not use save files, networking, databases, or Asset Store gameplay systems.

## 14. Related Documentation

- `Assets/Docs/QA_Playtest_Guide.md`
- `Assets/Docs/KyUcSaiGon_Blockout_Setup.md`
- `Assets/Docs/BusHub_FBX_Interior_Specs.md`
- `Assets/Docs/BusHub/BusHub_FBX_Interior_Specs.md`
