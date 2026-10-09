# FinalGame

Three-in-one Unity game based on the supplied CSX4515 / ITX4606 final brief.

Open this folder in **Unity 6000.0.79f1**. Open `Assets/FinalGame/Scenes/MainMenu.unity` and press Play. If the generated scenes are missing, choose **FinalGame > Set Up Game Scenes** once and then press Play.

A macOS app is included at `Builds/FinalGame.app`. Double-click it to play without Unity. The source ZIP contains the editable project and excludes generated caches and builds.

## Games and controls

| Menu choice | Source | Controls |
| --- | --- | --- |
| Mad Driver | Existing Prototype 1 scene, vehicle and scripts | W/S drive, A/D steer; arrow keys also work |
| flyingplane | Existing Challenge 1 scene, plane and scripts | W/S or up/down arrows pitch; automatic forward movement |
| I'm a Sumo and a Ball | Soccer scripting adapted for sumo; round wrestler characters and a flat arena | WASD or arrow keys move, Space charge |

In sumo, knock opponents off the circular platform. Clearing a wave spawns another, up to eight opponents at once. You are the blue-belt wrestler. Collect the cyan cube for seven seconds of stronger knockback. If you fall, use Escape > Restart.

Escape pauses any subgame and opens Resume, Restart, and Back to Main Menu. Escape again also resumes. Restart and returning to the main menu reset the time scale and audio pause. Exit closes a built application; in Unity it stops Play mode.

## Implementation

Original projects are left in their own folders. Imported copies retain asset GUIDs, with shared dependencies deduplicated. Driving uses Rigidbody movement and stable road colliders. The sumo platform uses a flat mesh collider; the characters use spherical colliders with round wrestler visuals. Namespaces resolve duplicate course script names. `GameInput` maps the course controls to the project's installed Input System. Menus are drawn at runtime by `GameMenus`; scene names and build settings connect the three games.

The original Prototype 4 sumo project was not present. As requested, this version adapts the available soccer scripting and Challenge 4 assets into a sumo arena. Replace it with the original sumo scene if strict reuse of that particular classroom project is needed.

## Before submission

1. Confirm the main-menu credit reads "By Chaw Yadanar Oo".
2. Keep the required screen recording running, including the final demonstration and Git commit/push process.
3. Demonstrate each of the three game buttons, movement, Escape pause, Resume, Restart, and Back to Main Menu.
4. Test Exit in a standalone build. In the editor it only stops Play mode.
5. Review and commit `Assets`, `Packages`, `ProjectSettings`, `.gitignore`, and this README. Exclude generated Library/Temp folders.
6. Publish to your chosen GitHub repository and verify the uploaded files. The provided brief allows a ZIP fallback with a penalty.
7. Observe the deadline in the brief. It explicitly penalizes source changes on GitHub after exam time.

The document's publishing and recording instructions are a submission checklist, not actions already performed by this project setup.

## Estimated time with assistance

- Integration and functional checks: approximately 1–2 hours total, assuming the existing scenes work.
- Additional visual polish, standalone build, recording review and repository submission: approximately 30–60 minutes.
- Allow extra time for Unity imports, licensing, missing course assets or project compatibility problems.

These are planning estimates, not a guarantee. A basic integration should be prioritized within the exam's stated three-hour window.

## Verification

Unity compilation and scene-reference validation passed. The automated play-mode checks passed for movement, Escape pause, frozen motion while paused, Resume, Restart, return to main menu, sumo wave progression, and falling detection. See `Documentation/PlayModeTestResults.txt`. The macOS standalone build also completed successfully.
