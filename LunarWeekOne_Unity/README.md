# Lunar Week One Prototype

This folder is the Unity project root for the MoonStation prototype.
Open `LunarWeekOne_Unity` directly in Unity Hub / Unity 2022 LTS.

This folder is a Unity prototype skeleton for the MoonStation concept.
It is no longer treated as a complete, ready-made production project.
Instead, it now provides:

- a cleaned-up runtime flow for the 7-day experience loop
- a shared data model and default config factory
- save/load, ritual, resource, audio, and feedback systems
- a runtime bootstrap that can scaffold a minimal playable scene
- editor tools that can generate `StartupScene` and `LunarBase`

## What You Can Do Right Now

If you only want to verify the scripts quickly:

1. Open any empty scene in Unity 2022 LTS.
2. Enter Play Mode.
3. `LunarPrototypeBootstrap` will create the minimum camera, systems, prototype nodes, ritual valve, floor, debug HUD, and feedback panel at runtime.
4. If no clips exist under `Assets/Resources/Audio`, the prototype will generate fallback ambient, breath, and cue audio automatically.

If you want an actual startup flow:

1. Open Unity.
2. Run `Lunar Week One/Create Prototype Scenes` from the menu.
3. The tool will create:
   - `Assets/Scenes/StartupScene.unity`
   - `Assets/Scenes/LunarBase.unity`
4. The tool also adds those scenes to Build Settings.
5. Open `StartupScene` and enter Play Mode.

## Generated Scene Flow

`StartupScene`

- shows Start New / Continue / Quit
- disables Continue when no saved session exists
- fades into the main experience scene
- keeps startup UI references wired automatically

`LunarBase`

- is the main experience scene target
- stays intentionally light as an editable shell
- gets its minimum prototype content from `LunarPrototypeBootstrap` during Play Mode

## Important Runtime Behavior

- `LunarPrototypeBootstrap` now detects startup context and does not auto-spawn prototype gameplay objects in `StartupScene`.
- `LunarExperienceController` no longer auto-starts the experience when the startup scene is active.
- Returning to `StartupScene` now suspends the active experience shell instead of letting timers and decay continue in the background.
- `UserSessionManager` keeps in-memory session intent, so `Start New` will not be overridden by an old save during scene transition.

## Main Scripts

- `Assets/Scripts/Core/LunarExperienceController.cs`
- `Assets/Scripts/Core/LunarDayStateMachine.cs`
- `Assets/Scripts/Core/LunarPrototypeBootstrap.cs`
- `Assets/Scripts/Core/LunarStartupScene.cs`
- `Assets/Scripts/Systems/ResourceManager.cs`
- `Assets/Scripts/Systems/RitualEngine.cs`
- `Assets/Scripts/Systems/UserSessionManager.cs`
- `Assets/Scripts/Systems/ExperienceFeedbackCollector.cs`
- `Assets/Scripts/Editor/LunarConfigEditor.cs`
- `Assets/Scripts/Editor/LunarSceneBuilder.cs`

## Story Reference

- Detailed narrative bible: `../22步故事法_剧情设计圣经.md`
- Scene-by-scene execution sheet: `../7天场景卡_叙事执行表.md`
- Voice, log, and communication text library: `../旁白_日志_通信文本库.md`
- Timeline and audio naming sheet: `../Timeline触发表_音频资产命名表.md`
- Log and prop placement sheet: `../日志与物件投放表.md`
- Default story data is encoded in `Assets/Scripts/Data/LunarDefaultConfigFactory.cs`

## Current Limits

- No real production scenes, models, or audio assets are included yet.
- The generated scenes are starter shells, not final content.
- This repo has been statically refactored, but it has not been Play Mode tested in this environment because Unity is not available here.

## Recommended Next Step

After generating the scenes in Unity, decide which direction to take:

- keep the runtime bootstrap as a fast prototype path
- or replace the generated placeholder content inside `LunarBase` with authored scene objects and assets
