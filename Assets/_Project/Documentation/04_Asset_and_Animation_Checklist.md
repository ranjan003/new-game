# Elemental Clash — Prioritized Asset and Animation Checklist

Version 0.1 · 13 Nov 2026
Status: **No assets exist yet.** Every box below is unchecked on purpose.

Priority key:
- **P0** = needed for the Zyron-vs-Noxar prototype (M2)
- **P1** = complete combat + vertical slice (M3–M4)
- **P2** = full roster, menus, arenas (M5–M6)
- **P3** = story, polish, release (M7–M8)

## 1. Placeholder sources (free, no purchase)
Licenses must be checked by you before use; I'll keep a `Licenses.md` listing each asset's source and terms.

| Source | What | License (as published by source; verify) |
|---|---|---|
| Unity primitives / ProBuilder | Greybox arena, capsule fighters | Unity package |
| Quaternius (quaternius.com) | Stylized rigged characters, animation library | CC0 |
| Kenney (kenney.nl) | UI elements, icons, prototype textures, some SFX | CC0 |
| Mixamo (Adobe account needed) | Humanoid rigging + large mocap animation library | Royalty-free use in projects; raw files cannot be redistributed |
| Blender 4.3 (installed on your PC) | Edits, custom placeholder meshes, retargeting | GPL tool; your output is yours |
| Unity Particle System + Shader Graph | All VFX made in-project | — |
| Freesound (CC0 filter only) / self-recorded | SFX | CC0 / yours |

Placeholders are swapped for **original** final models later. Placeholder characters must not resemble any existing franchise character.

## 2. Characters

### 2.1 Per-fighter deliverables (repeat for all 11)

| Item | Priority for Zyron & Noxar | Others |
|---|---|---|
| Design brief (personality, silhouette, palette, costume, weapon) | P0 (text, done in GDD §4–5) | P2 |
| Front / side / back turnaround concept | P1 | P2 |
| Facial expression sheet (neutral, battle cry, hurt, victory, defeat) | P1 | P2 |
| Placeholder model (humanoid rig) with element-color material | **P0** | P2 |
| Final 3D model (15–20k tris, 1–2 materials, 1024² atlas) | P1–P3 (depends on art path) | P3 |
| Humanoid rig (Unity Humanoid compatible, 1 extra bone for weapon/cloak if needed) | **P0** | P2 |
| Portrait (512² card art) + icon (128²) | P1 (placeholder P0) | P2 |
| Element VFX set (see §4) | **P0 partial** | P2 |
| SFX + voice barks (5–8 lines) | P1 | P2–P3 |

### 2.2 Animation list per fighter (≈38 clips)
Shared clips can come from one library and be reused by all fighters (marked **S**). Fighter-specific clips (**F**) define each identity.

| Category | Clips | Type | Zyron/Noxar priority |
|---|---|---|---|
| Idle | Battle idle, idle variation | F (posture differs) | P0 / P1 |
| Walk | Forward, back, strafe L, strafe R (blend tree) | S | **P0** |
| Run | Run forward | S (can be F later) | **P0** |
| Jump | Jump start, air loop, land | S | P1 |
| Dodge | Dash forward, back, left, right | S | P1 (forward only P0) |
| Guard | Guard start, guard loop, guard hit, guard break | S | P1 |
| Light attack | L1, L2, L3 | F | **P0** |
| Heavy attack | Heavy, Finisher (L-L-H) | F | P1 |
| Special 1 | Wind-up, release, recovery (1–3 clips) | F | **P0** (Volt Dash, Shadow Step) |
| Special 2 | Wind-up, release, recovery | F | P1 |
| Defensive ability | Activate, hold, end | F | P1 |
| Ultimate | Attacker sequence + victim reaction (paired) | F | P1 |
| Hit reaction | Light front, light back, heavy front, launch | S | **P0** (light front + heavy front) |
| Knockdown | Fall, lie, getup, getup roll | S | **P0** (fall + getup) |
| Stun | Stun loop | S | P1 |
| Victory | Victory pose | F | **P0** |
| Defeat | Defeat / KO collapse | S/F | **P0** |
| Intro | Round intro pose | F | P2 |

**P0 total for the prototype:** about 14 clips per fighter, about 10 of which can be shared.

## 3. Arenas

| Item | Priority |
|---|---|
| Greybox arena (ProBuilder ring, 24 m, boundary collider, spawn points) | **P0** |
| Sky Sanctuary art pass 1 (modular stone ring, arches, cloud skybox) | P1 |
| Molten Crater, Frost Citadel, Void Nexus art pass 1 | P2 |
| Baked lighting + light probes per arena | P1 (first arena), P2 (others) |
| Arena thumbnails (16:9) | P2 |
| Optional hazards (4) | P3 |
| Ambient loops per arena | P2 |

## 4. VFX (built in Unity Particle System + Shader Graph)

| Effect | Priority |
|---|---|
| Generic hit spark (light / heavy) tinted by element | **P0** |
| Guard spark | P1 |
| Dodge trail / afterimage | P1 |
| Zyron: fist crackle trail, Volt Dash streak + impact | **P0** |
| Noxar: claw smoke trail, Shadow Step sink/emerge | **P0** |
| Zyron: Arc Barrage, Static Guard, Volt Tempest | P1 |
| Noxar: Dark Clone, Void Veil, Eclipse Dominion | P1 |
| Telegraph ground decal (generic) | P1 |
| Ultimate-ready aura on fighter | P1 |
| KO slow-mo flash | **P0** (simple) |
| Other 9 fighters' VFX sets (≈8 effects each) | P2 |

## 5. UI

| Item | Priority |
|---|---|
| On-screen joystick + 7 buttons (placeholder sprites) | **P0** (L, H, S1, D, G at minimum; S2/ULT P1) |
| HP bars (with delayed damage trail), round text, KO banner | **P0** |
| EP bar, ultimate meter, cooldown wipes | P0 (EP, cooldown) / P1 (ult) |
| Timer, round wins | P1 |
| Pause menu | P1 |
| Result screen (Rematch / Menu) | **P0** (simple) |
| Element icons (11) | P1 |
| Main menu, character select (11 cards), arena select, match settings | P2 |
| Settings + button layout editor | P2 |
| Font (open license, e.g. SIL OFL) | P1 |
| Story dialogue box + portraits | P3 |

## 6. Audio

| Item | Priority |
|---|---|
| Hit SFX light / heavy (generic) | **P0** |
| Whoosh, dodge, guard | P1 |
| Element SFX families (11) | P1 (Lightning, Shadow), P2 (others) |
| Announcer: "Round 1/2/3", "Fight", "K.O.", "Time", "You Win", "You Lose" | P1 |
| Menu music, 1 battle theme | P1 |
| 3 more arena themes + boss theme | P2–P3 |
| Character voice barks | P2 |
| UI clicks | P1 |

## 7. Data assets (ScriptableObjects)

| Asset | Priority |
|---|---|
| FD_Zyron, FD_Noxar | **P0** |
| Moves: Zyron L1–L3, Volt Dash; Noxar L1–L3, Shadow Step | **P0** |
| AI_Normal profile | **P0** |
| MatchRules_Single | **P0** |
| Remaining moves for Zyron/Noxar, AI_Easy/Hard, Bo3/Bo5 | P1 |
| AffinityTable + 11 ElementDefinitions | P1 |
| 9 other fighters + moves | P2 |
| Arena definitions (4) | P2 |
| Story chapter + dialogue assets | P3 |

## 8. Documentation
| Item | Priority |
|---|---|
| These design docs | Done (draft) |
| Unity setup guide (install, open, configure Android) | **P0** (with M1) |
| Inspector assignment guide per prefab | **P0** (with M2) |
| Licenses.md | **P0** (first placeholder import) |
| Device test sheet | **P0** (end of M2) |
| Balancing sheet (move data export) | P1 |
| Build and signing guide | P3 |
