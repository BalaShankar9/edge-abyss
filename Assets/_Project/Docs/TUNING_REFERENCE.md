# EdgeAbyss Tuning Reference

## PHASE 6: Feel/Tuning Lock

This document records the locked tuning values for the MVP build.

---

## Bike Tuning (Fast, Twitchy, Risky)

| Parameter | Value | Notes |
|-----------|-------|-------|
| Max Speed | 35 m/s | ~126 km/h - fast enough to feel dangerous |
| Acceleration | 18 | Quick to reach speed |
| Brake Deceleration | 30 | Strong brakes for emergencies |
| Drag | 2.5 | Moderate resistance |
| Max Turn Rate | 100°/s | Very responsive steering |
| Steer Response | 10 | Instant response to input |
| High Speed Steer Factor | 0.4 | Reduced steering at speed (safety) |
| Stability Recovery | 0.6 | Medium recovery rate |
| Fall Threshold | 0.1 | Falls at 10% stability |
| Steer Stability Cost | 0.25 | Turning costs stability |
| Focus Stability Bonus | 0.15 | Holding focus helps |
| Max Lean Angle | 30° | Aggressive lean |
| Lean Speed | 8 | Fast lean transitions |
| Gravity Multiplier | 1.2 | Slightly heavier feel |
| Auto Correction | 0 | No auto-stabilization (skill based) |
| Momentum Inertia | 0 | No momentum carry |
| Lean Turn Influence | 0.6 | Lean affects turn strongly |

**Design Intent**: The bike is for skilled players who want speed and risk. Twitchy controls reward precision but punish mistakes.

---

## Horse Tuning (Stable, Momentum-Based, Forgiving)

| Parameter | Value | Notes |
|-----------|-------|-------|
| Max Speed | 28 m/s | ~100 km/h - still fast but controllable |
| Acceleration | 12 | Slower to build speed |
| Brake Deceleration | 20 | Softer brakes |
| Drag | 1.5 | Less resistance |
| Max Turn Rate | 70°/s | Wide turning arc |
| Steer Response | 5 | Delayed response (momentum) |
| High Speed Steer Factor | 0.6 | Better steering at speed |
| Stability Recovery | 0.8 | Fast recovery |
| Fall Threshold | 0.08 | More forgiving (8% threshold) |
| Steer Stability Cost | 0.15 | Cheaper to steer |
| Focus Stability Bonus | 0.25 | Focus helps more |
| Max Lean Angle | 18° | Subtle lean |
| Lean Speed | 4 | Slow, deliberate lean |
| Gravity Multiplier | 1 | Normal weight |
| Auto Correction | 0.4 | Some auto-stabilization |
| Momentum Inertia | 0.6 | Carries momentum |
| Lean Turn Influence | 0.2 | Lean has minor effect |

**Design Intent**: The horse is for beginners or players wanting a more relaxed experience. Momentum-based controls feel heavy but stable.

---

## Score Tuning

| Parameter | Value | Notes |
|-----------|-------|-------|
| Points Per Unit | 1 | 1 point per meter |
| Min Scoring Speed | 2 m/s | Must be moving to score |
| Reference Speed | 30 m/s | Speed multiplier baseline |
| Max Speed Multiplier | 3x | Up to 3x score at max speed |
| Streak Build Time | 3s | Clean riding builds streak |
| Max Streak Level | 10 | x10 streak maximum |
| Streak Bonus Per Level | 0.1 | 10% bonus per streak level |
| Edge Bonus Points/Sec | 20 | Bonus for riding near edge |
| Fall Penalty | 100 | Points lost on fall |

---

## Camera Tuning

| Parameter | Value | Notes |
|-----------|-------|-------|
| Base FOV | 75° | Standard field of view |
| Max Speed FOV Boost | 12° | FOV increases to 87° at speed |
| FOV Lerp Speed | 4 | Smooth FOV transitions |
| Max Roll Angle | 6° | Subtle camera roll on turns |
| Roll Lerp Speed | 5 | Smooth roll transitions |
| Speed Shake Intensity | 0.015 | Light shake at speed |
| Position Follow Speed | 20 | Responsive camera following |
| Rotation Follow Speed | 15 | Smooth rotation following |

---

## Wind Tuning

| Parameter | Value | Notes |
|-----------|-------|-------|
| Base Wind Intensity | 1.5 | Constant ambient wind |
| Direction Variance | 15° | Wind shifts direction slightly |
| Variance Speed | 0.3 | Slow direction changes |
| Gust Interval | 8s | Time between gusts |
| Gust Duration | 1.5s | Length of each gust |
| Gust Intensity Multiplier | 2x | Gusts are 2x base wind |
| Stability Impact | 0.015 | Wind affects stability mildly |
| Strong Wind Threshold | 5 | Wind zones above this are dangerous |

---

## Audio Tuning

| Parameter | Value | Notes |
|-----------|-------|-------|
| Master Volume | 1.0 | Full volume default |
| Music Volume | 0.7 | Music slightly quieter |
| SFX Volume | 1.0 | Full SFX |
| UI Volume | 0.8 | UI sounds slightly quieter |
| Wind Sound Start Speed | 10 m/s | Wind audio kicks in |
| Wind Sound Max Speed | 30 m/s | Wind audio maxes out |
| Ambient Volume | 0.3 | Subtle ambient sounds |
| Music Crossfade | 2s | Smooth music transitions |

---

## Design Philosophy

### "Scary but Fair"
- High speed feels dangerous but controllable
- Crashes feel earned, never random
- Edge riding rewards risk with higher scores
- Controls are smooth with appropriate damping

### Bike vs Horse Trade-off
| Aspect | Bike | Horse |
|--------|------|-------|
| Speed | Higher | Lower |
| Stability | Lower | Higher |
| Controls | Twitchy | Heavy |
| Skill Ceiling | High | Medium |
| Forgiveness | Low | High |

---

*Tuning locked for MVP build - January 2026*
