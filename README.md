# E-Kiraro Simulation

**Project demonstration:** [E-Kiraro Simulation](https://youtu.be/PYlYuRPDQng)

E-Kiraro is a first-person Unity simulation about navigating a travel-service process. The player follows instructions, speaks with an NPC, uses a TV remote, completes a laptop form, and reaches the ending area.

## Mission Statement

Our mission is to facilitate the lives of the citizens of Burundi through software solutions. E-Kiraro is the first of many steps. 

## Problem Context

People can find multi-step service processes difficult to navigate when instructions, required documents, or next steps are unclear. This simulation presents that challenge as a travel and passport-service journey, where the player encounters a queue, receives guidance, and completes an application.

The issue matters because confusing processes can make access to essential services frustrating. The project connects to its GCGO by illustrating how clear guidance and straightforward digital interactions can improve the experience.

## Simulation Overview

E-Kiraro is a first-person simulation for players exploring the challenges of a multi-step service journey. The player progresses through the scenario by following on-screen instructions and interacting with scene objects.

### Key interactions

- Press **Start** to begin the simulation.
- Use **WASD** to move and the **mouse** to look around.
- Press **Escape** to unlock or relock the cursor.
- Click the travel-agency NPC to continue its dialogue.
- Walk onto objective planes to trigger the next parts of the scenario.
- Click the remote to turn on the TV.
- Click the laptop, complete its UI, and press **Submit**.
- Reach the final objective area to display the ending UI.

## Unity Mechanics Implemented

### UI

Unity UI is used for the start screen, timed dialogue, laptop application, instructions, and ending panel. The dialogue manager queues messages and displays each for four seconds. The laptop submission advances the sequence and enables the final objective.

### Scripting

C# scripts implement first-person movement, input actions, click interactions, dialogue sequencing, objective progression, and audio cues. Audio sources are assigned through the Inspector; the talking sound plays briefly with each dialogue message, and the victory sound plays after a valid laptop submission.

### Collision

The player uses a `CharacterController` for movement, gravity, and collision with the environment. Trigger colliders on objective planes detect when the player reaches progression areas.

### Raycasting

On a mouse click, a ray is cast from the camera through the cursor position, or through the screen centre while the cursor is locked. The ray checks nearby colliders and identifies interactable objects such as the NPC, remote, and laptop.


## Additional Features

- Cursor locking during gameplay, with Escape to toggle it.
- Dialogue and audio cues coordinated with the objective sequence.
- The NPC queue disappears after laptop submission.

## Build Information

- **Unity Editor:** 6000.6.0f1
- **Input System package:** 1.20.0
- **WebGL deployment:** 
- **Android APK:** 

### Run the project in the Unity Editor

1. Install Unity Editor **6000.6.0f1** using Unity Hub.
2. In Unity Hub, choose **Add** and select this repository's root folder.
3. Open the project and wait for Unity to finish importing assets and resolving packages.
4. Open `Assets/Scenes/SampleScene.unity`.
5. Press **Play** and click **Start** in the game view.

To create deployable versions, use Unity's **Build Profiles** to build for WebGL or Android. A WebGL build must be hosted to create a public deployment link. An Android build must be built as an APK and shared from an accessible download location. Add those links to this README when they are available.
