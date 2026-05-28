# Ky Uc Sai Gon Blockout Prototype Setup

## Generate the scenes

1. Open the project in Unity 2022+ or Unity 6.
2. Wait for scripts to compile.
3. Use menu: `Ky Uc Sai Gon > Generate Complete Blockout Prototype`.
4. The generator creates these scenes in `Assets/Scenes` and adds them to Build Settings:
   - `Scene_00_BusHub`
   - `Scene_01_NguyenHue_Tutorial`
   - `Scene_02_BenThanh`
   - `Scene_03_DinhDocLap`
   - `Scene_04_NhaThoDucBa`
   - `Scene_05_Bitexco`
   - `Scene_06_BachDang`
   - `Scene_07_Ending`

## Test the third-person prototype

1. Open `Scene_01_NguyenHue_Tutorial`.
2. Press Play.
3. Move with WASD.
4. Rotate/orbit the camera with the mouse.
5. Face or look toward an NPC, item, puzzle console, route button, or bus stop.
6. Press `E` to interact.
7. Press `Enter` to submit puzzle answers.
8. Press `Esc` to close dialogue or puzzle UI.

## Current blockout scale

- Nguyen Hue: about `100 x 60`.
- Ben Thanh: about `80 x 80`.
- Dinh Doc Lap: about `90 x 80`.
- Nha Tho Duc Ba: about `80 x 80`.
- Bitexco: about `80 x 80`.
- Bach Dang: about `100 x 70`, including river area.

Each scene includes red transparent boundary cubes around the playable area.

## Placeholder colors

- Gray: inactive environment and landmarks.
- Blue: interactable memory items and readable props.
- Yellow: puzzle objects and route buttons.
- Green: restored state color.
- Purple: NPCs.
- Orange: bus stops.
- Red transparent: scene boundaries.

## Third-person player

- Player object: `REPLACE_Player_Character`.
- Camera target child: `Player_CameraTarget`.
- Movement script: `ThirdPersonPlayerController`.
- Camera script: `ThirdPersonCamera`.
- Interaction script: `Interactor`.

The camera uses a simple custom orbit follow with SphereCast collision avoidance, so Cinemachine is not required.

## Puzzle answers

- Nguyen Hue speaker mixer: `1-6-8`
- Ben Thanh fruit basket: `1-3-2`
- Dinh Doc Lap history code: `1975`
- Nha Tho Duc Ba bell sequence: `La-Sol-Re-Mi-Si-Do`
- Bitexco security code: `345`
- Bach Dang river crossing: `28`

## Full gameplay path

1. Restore Nguyen Hue.
2. Use the bus stop to return to `Scene_00_BusHub`.
3. Select each unlocked route on the route board.
4. Restore all 5 remaining locations.
5. Return to the Bus Hub.
6. Select `Ending Route` after progress reaches `6/6`.

## Placeholder replacement workflow

Each generated scene has these parent roots:

- `LandmarkRoot`
- `NPCRoot`
- `ItemRoot`
- `PuzzleRoot`
- `PropRoot`
- `EffectsRoot`
- `SpawnPoints`

Replace objects named with the `REPLACE_` prefix, for example:

- `REPLACE_Landmark_NguyenHue_Fountain`
- `REPLACE_NPC_StreetMusician`
- `REPLACE_Puzzle_SpeakerMixer`
- `REPLACE_Prop_LED_Red_Number_1`
- `REPLACE_Landmark_BenThanh_Gate`
- `REPLACE_Item_OldConicalHat`
- `REPLACE_Puzzle_FruitBasket`
- `REPLACE_BusStop_ReturnHub`
- `REPLACE_BusHub_MapBoard`

Keep the interactable script on the replacement object or move the script to the new model root.
Keep colliders on interactable objects so the player's raycast can detect them.

## Important scripts

- `GameProgressManager`: persistent progress with `DontDestroyOnLoad`.
- `ThirdPersonPlayerController`: WASD movement relative to camera direction.
- `ThirdPersonCamera`: mouse orbit camera with collision avoidance.
- `Interactor`: camera-center and player-forward interaction with `E`.
- `PuzzleInteractable`: shared puzzle logic.
- `MemoryZoneController`: marks a location restored and adds the memory fragment.
- `MaterialRestoreEffect`, `LightRestoreEffect`, `ParticleRestoreEffect`, `AudioRestoreEffect`: simple restore feedback.
- `MapSelectionInteractable`: Bus Hub route buttons.
- `BusStopInteractable`: return route after restoration.

## Notes

- The generator uses Unity primitives only.
- Runtime UI uses built-in `uGUI` for maximum compatibility.
- World labels try to use TextMeshPro through reflection if TMP exists in the project. If TMP is not available, labels fall back to Unity `TextMesh`.
- Audio sources are placeholders without clips. Add music or ambience clips later to each `AudioRestoreEffect_AmbiencePlaceholder`.
