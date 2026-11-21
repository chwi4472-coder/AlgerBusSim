# AlgerBusSim — حافلة الجزائر

An Algerian bus simulator. Original engine-independent implementation, built from
scratch for Unity 6.6 LTS / Android ARM64.

> **No asset, mesh, texture, audio file or text from any third-party game is used
> in this project.** See `../PHASE1_TECHNICAL_REPORT.md` §5.

## Status

| Phase | State |
|---|---|
| 1 — XAPK inspection | ✅ complete (`../PHASE1_TECHNICAL_REPORT.md`) |
| 2 — Architecture | ✅ complete (`../PHASE2_ARCHITECTURE.md`) |
| 3 — Algerian art identity | ✅ complete (`../PHASE3_REPORT.md`) |
| 4 — Algiers map | ✅ complete (`../PHASE4_REPORT.md`) |
| 5 — Bus + gameplay | ✅ code complete (`../PHASE5_REPORT.md`) |
| 6 — Android integration | ✅ code complete (`../PHASE6_REPORT.md`) |
| 7 — Testing / APK | ⬜ planned (`../PHASE7_REPORT.md`) — gated on Unity install + device |

## First open (once Unity is installed)

1. Install **Unity 6000.6.8f1** with **Android Build Support** (+ SDK/NDK/OpenJDK)
   via Unity Hub, and sign in so the licence is active.
2. Open this folder as a Unity project.
3. Menu **AlgerBusSim → Configure Project** (sets bundle id, IL2CPP, ARM64, ASTC,
   orientation, graphics APIs).
4. Menu **AlgerBusSim → Create Bootstrap Scene** if it was not generated.
5. Press Play. Menu **AlgerBusSim → Build APK** to produce
   `Builds/AlgerBusSim.apk`.

Batch mode:

```
Unity.exe -quit -batchmode -projectPath . ^
  -executeMethod AlgerBusSim.EditorTools.BuildScript.BuildAndroid
```

## Path warning

The project currently lives under
`C:\Users\Dell\Documents\Projet par défaut\` — the non-ASCII `é` and the space can
break the Android NDK/CMake toolchain. **Before Phase 6, move the folder to an
ASCII path**, e.g. `C:\Dev\AlgerBusSim`. Nothing in the project depends on its
location.

## Cloud build (no local Unity)

A real ARM64 APK can be produced for free via GitHub Actions — no Unity install,
no 20 GB of disk, no Android SDK. See [`CLOUD_BUILD.md`](CLOUD_BUILD.md).

```
Assets/AlgerBusSim/Runtime/   game code (Core, World, Vehicles, Gameplay, Traffic, UI, Art, Audio, Data)
Assets/AlgerBusSim/Editor/    project configurator, scene installer, build script
Assets/StreamingAssets/       JSON data + PNG atlases loaded at runtime
tools/                        Python atlas generators and validators
```
