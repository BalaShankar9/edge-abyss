# EdgeAbyss

**A tense, smooth, and addictive edge-riding game where you balance on the precipice of disaster.**

## Overview

EdgeAbyss is a precision-control riding game where players navigate narrow ridge tracks at high speeds. The core gameplay loop revolves around the tension between speed (which increases score but decreases stability) and safety (which is slower but more controlled).

## Features

- **Multiple Riders**: Switch between Bike (fast, twitchy) and Horse (stable, momentum-based)
- **Dynamic Wind System**: Environmental wind pushes riders over time, creating fair but challenging conditions
- **Scoring System**: Distance-based scoring with streak bonuses and edge proximity rewards
- **Stability Mechanics**: Balance management that rewards skill without punishing unfairly

## Controls

### Keyboard
| Key | Action |
|-----|--------|
| W | Accelerate |
| S | Brake |
| A/D | Steer Left/Right |
| Left Shift | Focus (stability boost) |
| R | Reset/Respawn |
| 1 | Switch to Bike |
| 2 | Switch to Horse |
| Escape | Pause Menu |
| F3 | Debug Overlay (toggle) |

### Gamepad
| Button | Action |
|--------|--------|
| Right Trigger | Accelerate |
| Left Trigger | Brake |
| Left Stick | Steer |
| A/Cross | Focus |
| Start | Pause |

## How to Run

### Quick Start (From Scratch)
1. Open the project in Unity 6000.3.4f1 (Unity 6 LTS)
2. Wait for packages to import
3. Go to menu: **EdgeAbyss > Build Complete Game**
4. Click "Build" in the confirmation dialog
5. Open `Assets/_Project/Scenes/TestTrack.unity`
6. Press Play

### If Game Is Already Built
1. Open `Assets/_Project/Scenes/Boot.unity`
2. Press Play
3. The game will load the Main Menu automatically
4. Click "Play" to start on TestTrack

### Quick Play (Skip Menu)
1. Open `Assets/_Project/Scenes/TestTrack.unity` directly
2. Press Play

## Editor Tools

| Menu Item | Description |
|-----------|-------------|
| EdgeAbyss > Build Complete Game | Creates all assets, prefabs, and scenes |
| EdgeAbyss > Validate Project | Checks project setup and reports issues |
| EdgeAbyss > Quick Open > Boot Scene | Opens Boot.unity |
| EdgeAbyss > Quick Open > MainMenu Scene | Opens MainMenu.unity |
| EdgeAbyss > Quick Open > TestTrack Scene | Opens TestTrack.unity |

## Project Structure

```
Assets/_Project/
├── Scenes/          # Game scenes (Boot, MainMenu, TestTrack, etc.)
├── Prefabs/         # Game prefabs
│   ├── Riders/      # Bike, Horse rider prefabs
│   └── UI/          # HUD, menus, etc.
├── Tuning/          # ScriptableObject tuning assets
├── Scripts/         # All game code
│   ├── Core/        # Bootstrapping, scene loading
│   ├── Gameplay/    # Riders, scoring, environment
│   ├── UI/          # HUD, menus
│   └── Editor/      # Editor tools
├── Materials/       # Visual materials
├── Audio/           # Sound effects and music
└── Art/             # Visual assets
```

## Tuning

All gameplay values are exposed through ScriptableObjects in `Assets/_Project/Tuning/`:

- **RiderTuning_Bike.asset**: Bike speed, handling, stability
- **RiderTuning_Horse.asset**: Horse speed, handling, stability
- **ScoreTuning.asset**: Scoring multipliers, streaks
- **CameraTuning.asset**: FOV, shake, follow settings
- **WindTuning.asset**: Wind intensity, gust behavior

## Design Philosophy

**"Scary but Fair"**
- High speed should feel dangerous but controllable
- Crashes should feel earned, never random
- Edge riding rewards risk with higher scores
- Controls are smooth with appropriate damping

## Requirements

- Unity 6000.3.4f1 (Unity 6 LTS)
- Universal Render Pipeline (URP)
- Input System Package
- TextMeshPro
- Cinemachine

## License

© 2026 EdgeAbyss Project
