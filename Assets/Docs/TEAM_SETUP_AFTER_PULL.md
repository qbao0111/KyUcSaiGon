# Ky Uc Sai Gon - Team Setup After Pull

Use this after cloning or pulling the project on a teammate machine.

## 1. Pull Git LFS assets

Run these commands once after clone/pull:

```powershell
git lfs install
git pull
git lfs pull
git lfs checkout
```

The project uses Git LFS for large FBX/PNG assets. If LFS assets are not hydrated, Unity may show missing references, broken models, or empty maps.

## 2. Open the correct Unity project folder

Open:

```text
D:\Game\KyUcSaiGon
```

Do not open an older copied folder such as `KyUcSaiGon_Codex...`.

## 3. Run the one-click setup tool

In Unity, use:

```text
Ky Uc Sai Gon > Team Setup > Apply After Pull
```

This tool:

- validates required large assets and Git LFS files
- verifies gameplay scenes in Build Settings
- reimports the main model/map assets only
- applies BusHub interior model references
- applies Nguyen Hue model references for facade rows, City Hall, fountain, and LED panels
- applies the AoDai player visual to all gameplay scenes

## 4. If something is still missing

Run:

```text
Ky Uc Sai Gon > Team Setup > Validate Clone Assets
```

If it reports Git LFS pointer files or missing assets, rerun:

```powershell
git lfs pull
git lfs checkout
```

## Notes

The older individual tools are still kept for debugging specific scenes. Teammates normally should not need to use them.

Avoid using:

```text
Ky Uc Sai Gon > Generate Complete Blockout Prototype
```

unless the team intentionally wants to regenerate the old blockout scenes, because it can overwrite polished scene work.
