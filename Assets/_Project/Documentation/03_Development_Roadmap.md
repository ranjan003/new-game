# Elemental Clash — Development Roadmap

Version 0.1 (draft for approval) · 13 Nov 2026

## Assumptions behind the estimates
- One developer (you) working with AI assistance, about **15–20 hours per week**.
- Placeholder art (free/CC0 models and animations) until final art is sourced. Final original character models and animations are the single biggest unknown and are **not** included in these durations (see doc 06).
- I write scripts and instructions; you run them in Unity on your PC and report back errors and feel. Each milestone ends with a test on a real Android phone.
- Estimates are honest ranges, not promises. They will be revised after M1.

## Milestones

| # | Milestone | Maps to brief phase | Est. duration | Exit criteria (all must be true) |
|---|---|---|---|---|
| **M0** | Design approved | Phase 1 | Now → your approval | You approve docs 01–06 and answer the decisions in doc 06 |
| **M1** | Project skeleton | Phase 2 start | 1 week | Unity 6 URP project opens with no errors; folder structure, asmdefs, Input Actions, URP mobile asset, Android build settings done; empty APK installs and runs on your phone |
| **M2** | **First playable: Zyron vs Noxar** | Phase 2 | 3–4 weeks | Doc 05 acceptance criteria met: move, light combo, one special each, HP/damage, basic AI, KO, victory/defeat, rematch, on-device at ≥30 FPS |
| **M3** | Complete combat system (2 fighters) | Phase 3 | 4–5 weeks | Heavy, finisher, guard + stamina, dodge + perfect dodge, both specials, defensive ability, ultimate (with simple cinematic), EP, rounds (1/Bo3/Bo5), timer, pause, 3 AI difficulties, EditMode tests green |
| **M4** | Vertical slice polish | Phase 3/7 | 2 weeks | Hit-stop, VFX, SFX, camera shake tuned; 5 external playtesters say combat "feels good"; first performance profile on device recorded |
| **M5a** | Roster wave 1 (+4) | Phase 4 | 4–6 weeks | Pyra, Terron, Kryon, Solen fully playable with unique moves and AI tuning |
| **M5b** | Roster wave 2 (+5) | Phase 4 | 5–7 weeks | Aeris (aerial systems), Vyra, Magrox, Crystara, Gravox playable; all 11 in Free Combat |
| **M6** | Menus, arenas, Training | Phase 5 | 4–5 weeks | Main menu, character/opponent select with 3D preview, arena select, match settings, 4 arenas (art pass 1), Training mode, Settings (incl. button layout editor), save/unlocks |
| **M7** | Story Combat | Phase 6 | 4–6 weeks | 5 chapters, dialogue system, boss phases, unlock progression |
| **M8** | Android polish and release candidate | Phase 7 | 3–4 weeks | Device test matrix passed, balancing pass, bug bash, signed AAB built, store-ready (publishing only with your approval) |

**Rough total:** about 9–12 months part-time with placeholder art. Final custom art runs in parallel and may extend this.

## Approval gates
You approve before each of these scope expansions:
1. Start of M2 (prototype scope locked).
2. After M2: "Is the core loop fun?" If not, we iterate on M2 before building more.
3. Start of M5 (roster expansion).
4. Start of M7 (story).
5. Any external publishing, asset purchase, or account change.

## Risk register

| Risk | Impact | Mitigation |
|---|---|---|
| No source for 11 original rigged characters and ~35 animations each | High | Placeholders first; decide art path early (doc 06 Q3) |
| Combat feels floaty on touch | High | M2 focuses on feel: input buffer, hit-stop, camera; tune before roster |
| Performance on mid-range phones | Medium | Budgets in doc 02 §8.1, profile at end of M2 and M4 |
| Scope creep (story, 11 fighters) | High | Gates above; Free Combat first |
| Unity 6 Android module install issue | Low | Verify in Unity Hub before M1 (doc 06 Q1) |
| Lost work | Medium | Git + LFS remote from M1 |

## Progress checklist (living)
- [x] Inspect available tools
- [x] Draft GDD, architecture, roadmap, asset list, prototype plan, decisions
- [ ] Design approved (M0)
- [ ] M1 Project skeleton
- [ ] M2 Zyron vs Noxar playable
- [ ] M3 Full combat system
- [ ] M4 Vertical slice polish
- [ ] M5a / M5b Full roster
- [ ] M6 Menus, arenas, Training
- [ ] M7 Story
- [ ] M8 Release candidate
