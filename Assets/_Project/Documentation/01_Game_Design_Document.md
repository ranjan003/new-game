# Elemental Clash: Arena Legends — Game Design Document

Version 0.1 (draft for approval) · 13 Nov 2026
Status: **Design only. No Unity project, scripts, or assets exist yet.**

---

## 1. Overview

| Item | Decision |
|---|---|
| Title | Elemental Clash: Arena Legends |
| Genre | 3D 1v1 elemental arena brawler (third-person, free movement, not a 2D-plane fighter) |
| Engine | Unity 6 LTS (6000.0.x) with URP, C#, Input System |
| Platform | Android, landscape only. Windows editor builds for testing. |
| Players | 1 player vs AI (offline). Online is out of scope for v1. |
| Session length | 2 to 6 minutes per match |
| Art style | Stylized, cel-shaded, bright readable elemental color coding, original designs |
| Monetization | None planned for v1 (decision pending, see doc 06) |

### 1.1 Elevator pitch
Pick one of eleven elemental fighters, each wielding exactly one power, and duel an AI opponent in a compact floating arena. Fast light/heavy combos, readable specials, a dodge with a real window, and a cinematic ultimate that you earn by fighting well.

### 1.2 Design pillars
1. **Responsive first.** Input to visible reaction in under 100 ms. Every button press does something or clearly tells you why not (cooldown, no energy).
2. **Readable combat.** Every attack telegraphs. Element colors are consistent: you always know whose effect is on screen and when you can dodge.
3. **One power, one identity.** Each fighter has exactly one element and a moveset that only makes sense for that element.
4. **Fair, not flat.** Elemental matchups nudge, never decide. Any fighter can beat any other.
5. **Mobile-honest.** Designed for thumbs and mid-range phones, not ported from a gamepad.

### 1.3 Originality rules (apply to every asset and line of text)
- No names, costumes, logos, catchphrases, transformations, or signature attacks from existing cartoons or fighting games.
- Silhouettes are defined by each fighter's own element shape language (see §5), not by reference to existing heroes.
- No device-based transformations (watches, rings, amulets) as the source of power. Powers are innate and tied to the world's elemental Cores.
- All music, SFX, models, and animations are original, CC0, or licensed for commercial use (see doc 04).

---

## 2. Core loop

```
Main Menu → Free Combat → Pick Fighter → Pick Opponent → Arena → Rules (rounds, timer, AI difficulty)
   → Round intro → FIGHT → KO or time-out → Round result → (next round) → Match result
   → Rematch | Character Select | Main Menu
```

Moment-to-moment loop: **approach → probe (light) → commit (heavy/special) → defend (guard/dodge) → build meter → cash out (ultimate)**.

---

## 3. Combat system

### 3.1 Resources

| Resource | Range | Gain | Spend |
|---|---|---|---|
| Health (HP) | 100 base, per-fighter 85–120 | — | Taken by hits; chip damage on guard |
| Elemental Energy (EP) | 0–100 | Regenerates 6 EP/s after 1.0 s without spending; +3 EP per landed light hit | Specials (20–35 EP), defensive ability (15–25 EP), dodge (0 EP, cooldown-based) |
| Ultimate meter | 0–100% | +1% per 1 damage dealt, +0.6% per 1 damage taken, +5% per perfect dodge | Ultimate uses 100% |
| Guard stamina | 0–100 | Regenerates 20/s when not guarding for 0.8 s | Guarding a hit costs stamina proportional to damage; empty = **Guard Break** (1.0 s stun) |

### 3.2 Actions (every fighter)

| Action | Button | Notes |
|---|---|---|
| Move | Left joystick | Camera-relative, 8-way analog |
| Light attack | L | 3-hit chain (L-L-L). Fast startup, low damage, builds EP |
| Heavy attack | H | Slower, causes knockback, cancels from 2nd light hit |
| Combo finisher | L-L-H | Launcher or wall-splat depending on fighter |
| Special 1 / Special 2 | S1 / S2 | Cost EP + cooldown. Cancelable from light hits |
| Defensive ability | Hold Guard + Dodge, or dedicated button in Advanced layout | Element-specific; costs EP |
| Guard | G (hold) | Blocks front 150°. Chip damage 15%. Drains guard stamina |
| Dodge | D | Directional dash, 0.20 s invulnerable window, 0.8 s cooldown. Perfect dodge (within 0.12 s before a hit) slows time 30% for 0.4 s |
| Ultimate | U | Needs 100% meter. Short cinematic if it connects; whiffs are punishable |
| Lock-on | Auto, tap camera area to switch target (future-proof for 2v2) | Camera keeps both fighters framed |

### 3.3 Move data model (every attack is data, not code)
Each move is a `MoveDefinition` asset with:
- **Startup / Active / Recovery** (in seconds, authored at 60 Hz frames for precision)
- Damage, chip damage, guard stamina damage
- Hitstun, blockstun, knockback vector and force, launch (yes/no)
- Hitbox shape, offset, and size per active window
- EP cost, cooldown, ultimate gain multiplier
- Cancel windows (which moves can interrupt this one, and from which frame)
- Armor (none / 1 hit / full) and whether it can be dodged or only guarded
- VFX, SFX, camera shake, hit-stop duration

### 3.4 Hit rules
- **Hit-stop:** 50–120 ms freeze on both fighters on impact (heavier = longer). Main source of "feel".
- **Counter hit:** Hitting during the opponent's startup = +25% damage and +50% hitstun. Shown with a distinct flash.
- **Punish:** Hitting during recovery = +15% damage.
- **Combo scaling:** Each hit after the 3rd in a combo deals 10% less (min 40%), prevents touch-of-death combos.
- **Knockdown:** Heavy finishers and ultimates cause knockdown. Getup has 0.4 s invulnerability and optional roll direction.
- **Stun:** Guard Break, and some specials (e.g. Freeze Trap) stun up to 1.2 s. Stun cannot be re-applied for 3 s (anti-lock).
- **Interruptions:** Any hit interrupts an attack in startup or recovery unless it has armor.

### 3.5 Telegraphs and readability
- Every special and ultimate has a 0.15 s+ wind-up with a colored glow on the attacking limb.
- Ground-targeted attacks show a decal marker before they hit.
- Unblockable moves (only ultimates and two boss moves) flash red-white.
- Cooldown is shown as a radial wipe on each button, EP cost as a small number, unavailable buttons desaturate.

### 3.6 Elemental affinity (soft matchups)
A single ring of all 11 powers. Each power is **favored** against the next one in the ring.

```
Fire → Nature → Earth → Lightning → Crystal → Light → Shadow → Gravity → Wind → Magma → Ice → (Fire)
```

| Attacker | Favored vs | Flavor |
|---|---|---|
| Fire | Nature | Burns growth |
| Nature | Earth | Roots split stone |
| Earth | Lightning | Grounds the current |
| Lightning | Crystal | Overloads the lattice |
| Crystal | Light | Refracts and scatters beams |
| Light | Shadow | Dispels darkness |
| Shadow | Gravity | Slips free of the pull |
| Gravity | Wind | Pins the air in place |
| Wind | Magma | Cools the crust |
| Magma | Ice | Melts through |
| Ice | Fire | Smothers the flame |

Effect: favored attacker deals **+8% damage**. The disfavored defender gains **+15% ultimate meter** from hits taken (comeback lever). Toggle "Elemental Affinity" in match settings (default ON; always OFF in Training unless enabled).

### 3.7 Match rules
- Formats: Single round, Best of 3, Best of 5.
- Round timer: 60 / 99 / infinite (default 99 s). Time-out = higher HP% wins; tie = both lose the round (sudden death in the final round).
- HP and EP reset each round. Ultimate meter carries over at 50%.
- Pause (menu button top-center): Resume, Move List, Settings, Restart, Quit to Menu.
- Post-match: Rematch, Character Select, Main Menu.

---

## 4. Roster

### 4.1 Stat sheet (initial balance values, tuned in testing)

Stats on a 1–5 scale; numbers in brackets are starting data values.

| # | Fighter | Power | Class | HP | Speed (m/s) | Power | Defense | EP regen | Unlock |
|---|---|---|---|---|---|---|---|---|---|
| H1 | Zyron | Lightning | Speed melee | 2 [90] | 5 [7.0] | 3 | 2 | 4 | Start |
| H2 | Pyra | Fire | Aggressor | 3 [100] | 3 [5.8] | 5 | 2 | 2 | Start |
| H3 | Terron | Earth | Heavy defender | 5 [120] | 1 [4.6] | 4 | 5 | 3 | Start |
| H4 | Aeris | Wind | Aerial skirmisher | 1 [85] | 5 [6.8] | 2 | 2 | 4 | Start |
| H5 | Kryon | Ice | Ranged control | 3 [100] | 3 [5.6] | 3 | 3 | 3 | Start |
| H6 | Vyra | Nature | Trapper | 3 [100] | 3 [5.8] | 2 | 3 | 4 | Start |
| H7 | Solen | Light | Precision ranged | 2 [92] | 3 [5.8] | 4 | 2 | 3 | Start |
| V1 | Noxar | Shadow | All-rounder / main antagonist | 3 [105] | 4 [6.4] | 4 | 3 | 3 | Story Ch.5 or 15 Free Combat wins |
| V2 | Magrox | Magma | Super-heavy | 5 [125] | 1 [4.4] | 5 | 4 | 2 | Story Ch.1 |
| V3 | Crystara | Crystal | Counter / zoner | 3 [98] | 3 [5.8] | 3 | 4 | 3 | Story Ch.3 |
| V4 | Gravox | Gravity | Space control | 4 [110] | 2 [5.0] | 4 | 3 | 3 | Story Ch.4 |

Villains are unlocked through Story **or** Free Combat milestones, so players who skip Story can still get them. For the prototype, Noxar is playable from the start.

### 4.2 Movesets
Light combo, heavy, and finisher are listed with the element-specific twist. EP costs and cooldowns are starting values.

#### Zyron — Lightning (Hero)
*Brave, impulsive. Wins by never letting go.*
- **Light chain:** jab-jab-spinning kick with crackling trails. Fastest startup in the game (5f).
- **Heavy:** Charged straight punch, short knockback, can be held 0.5 s to add a mini-stun.
- **Special 1 — Volt Dash** (20 EP, 4 s): Lightning-fast forward rush that passes through the opponent, hitting once. Leaves a short spark trail. Punishable if guarded.
- **Special 2 — Arc Barrage** (30 EP, 7 s): Five rapid electric strikes in a forward cone, last hit launches.
- **Defense — Static Guard** (20 EP, 8 s): 0.6 s charged parry. If struck, retaliates with a shock that stuns 0.5 s.
- **Ultimate — Volt Tempest:** Blinks around the opponent striking from 6 directions, then a downward thunder strike.
- Strength: speed, combo pressure. Weakness: lowest guard stamina (70).

#### Pyra — Fire (Hero)
*Fearless, competitive. Hits hardest, burns out fastest.*
- **Light:** Flame-gauntlet hooks. **Heavy:** Overhead smash with fire burst.
- **S1 — Fireball Burst** (25 EP, 3 s): Mid-speed projectile, explodes on contact (small AoE).
- **S2 — Flame Rush** (30 EP, 6 s): Advancing three-punch flurry wrapped in flame, armored on first hit.
- **Defense — Ember Shield** (20 EP, 9 s): 1 s ring of embers that blocks projectiles and burns attackers for chip damage.
- **Ultimate — Inferno Breaker:** Grabs, launches, and drives the opponent down with a flaming double-fist slam.
- Strength: highest damage. Weakness: specials cost about 20% more EP; slow EP regen.

#### Terron — Earth (Hero)
*Calm, protective. A wall that punches back.*
- **Light:** Stone-fist jabs. **Heavy:** Ground pound with short-range shockwave.
- **S1 — Boulder Throw** (25 EP, 5 s): Rips up and throws a slow, large boulder that destroys smaller projectiles.
- **S2 — Ground Spike** (30 EP, 6 s): A line of stone spikes erupting toward the opponent, decal telegraph, launches.
- **Defense — Stone Wall** (25 EP, 10 s): Raises a temporary wall (3 s, 60 HP) that blocks movement and projectiles.
- **Ultimate — Titan Rupture:** Splits the arena floor in a radius, launching the opponent into a falling-rock combo.
- Strength: heaviest guard, light attacks have 1-hit armor. Weakness: slowest movement and dodge.

#### Aeris — Wind (Hero)
*Clever, adventurous. Never where you expect.*
- **Light:** Twin curved wind-blade slashes. **Heavy:** Spinning rising slash (launcher).
- **S1 — Air Cutter** (20 EP, 3 s): Fast crescent projectile, low damage, can be fired in the air.
- **S2 — Cyclone Lift** (30 EP, 7 s): Small tornado that lifts the opponent, enabling an air combo.
- **Defense — Wind Step** (15 EP, 6 s): Instant short teleport-dash with longer invulnerability than a normal dodge.
- **Ultimate — Skybreaker Cyclone:** Pulls the opponent into a huge cyclone, finishes with a diving blade strike.
- Strength: double jump, air dash, air specials. Weakness: lowest HP.

#### Kryon — Ice (Hero)
*Disciplined, analytical. Controls space, then strikes.*
- **Light:** Frost-sword slashes. **Heavy:** Wide horizontal sweep that briefly slows (−20% speed, 1.5 s).
- **S1 — Frost Spear** (25 EP, 4 s): Fast straight projectile, slows on hit.
- **S2 — Freeze Trap** (30 EP, 9 s): Places a glyph (lasts 6 s). Opponent stepping in is frozen 1.0 s.
- **Defense — Crystal Barrier** (20 EP, 8 s): Frontal ice wall that reflects the next projectile.
- **Ultimate — Absolute Frost:** Freezes the arena floor, encases the opponent, and shatters the ice with a final slash.
- Strength: slows and zone control. Weakness: longest recovery on heavies.

#### Vyra — Nature (Hero)
*Patient, intelligent. Turns the arena into a garden of traps.*
- **Light:** Leaf-blade whips (longest light range). **Heavy:** Vine lash that pulls the opponent closer.
- **S1 — Vine Snare** (25 EP, 6 s): Vine travels along the ground, roots the opponent 0.8 s.
- **S2 — Thorn Burst** (30 EP, 7 s): Plants a seed that bursts into thorns after 0.6 s (AoE). Up to 2 seeds.
- **Defense — Root Barrier** (20 EP, 8 s): Roots herself in place, heavy armor for 1 s, regenerates 5 HP.
- **Ultimate — Wildroot Dominion:** Arena-wide roots bind the opponent while thorns strike repeatedly.
- Strength: area control. Weakness: low burst damage.

#### Solen — Light (Hero)
*Confident, thoughtful. Picks you apart from range.*
- **Light:** Energy-spear thrusts with short light bolts. **Heavy:** Charged spear throw (returns).
- **S1 — Radiant Beam** (30 EP, 6 s): Short-charge long beam, highest special range, narrow.
- **S2 — Flash Strike** (25 EP, 5 s): Blinding flash teleport behind the opponent followed by a strike.
- **Defense — Prism Shield** (20 EP, 8 s): Floating panels absorb two projectiles and return them as bolts.
- **Ultimate — Solar Judgment:** Rises, charges, and fires a column of light from above with a targeting circle.
- Strength: range. Weakness: lowest close-range guard and slower light startup.

#### Noxar — Shadow (Villain, main antagonist)
*Cold, ambitious. Wants to awaken the Shattered Core.*
- **Light — Shadow Slash chain:** Clawed gauntlet slashes trailing violet smoke.
- **Heavy:** Cloak sweep that pushes the opponent away.
- **S1 — Shadow Step** (20 EP, 4 s): Sinks into shadow and reappears beside the opponent (left/right by stick), with a slash.
- **S2 — Dark Clone** (35 EP, 10 s): Summons a clone that mirrors his next attack from another angle, then fades (2 s).
- **Defense — Void Veil** (20 EP, 8 s): Becomes intangible for 0.5 s; attacks pass through. Followed by a short vulnerable fade-in.
- **Ultimate — Eclipse Dominion:** Arena dims to near-black; repeated slashes from the darkness, then a crushing void burst.
- Boss version (Story): 2 phases. Phase 2 at 50% HP: clone duration doubles and Shadow Step chains twice.

#### Magrox — Magma (Villain, heavy boss)
- **Light — Molten Punch chain.** **Heavy:** Overhead double fist, cracks the floor.
- **S1 — Lava Burst** (25 EP, 5 s): Lobs three magma globs that leave burning pools for 3 s.
- **S2 — Magma Eruption** (35 EP, 9 s): Three telegraphed eruptions around him, launches.
- **Defense — Molten Armor** (25 EP, 12 s): 3 s super armor, takes 30% less damage.
- **Ultimate — Volcanic Cataclysm:** Leaps and slams, triggering an arena-wide eruption.
- Boss: Phase 2 armor is permanent but he moves slower; punish windows grow.

#### Crystara — Crystal (Villain, tactical boss)
- **Light — Crystal Slash chain.** **Heavy:** Faceted blade lunge.
- **S1 — Crystal Spike** (25 EP, 4 s): Spikes erupt beneath the opponent's last position.
- **S2 — Shard Storm** (30 EP, 7 s): Orbiting shards fire outward in a spiral.
- **Defense — Reflective Barrier** (20 EP, 8 s): Full-body barrier that reflects projectiles and counters melee.
- **Ultimate — Crystal Cataclysm:** Encases the arena in a crystal dome; shards rain inward.
- Boss: Places crystal pillars that she can teleport to (Phase 2).

#### Gravox — Gravity (Villain, advanced boss)
- **Light — Gravity Punch chain.** **Heavy:** Downward-gravity slam that pins (0.3 s).
- **S1 — Gravity Pull** (20 EP, 5 s): Pulls the opponent toward him (counter: dodge out).
- **S2 — Gravity Well** (35 EP, 9 s): Zone that slows opponents by 40% for 4 s.
- **Defense — Repulsion Field** (20 EP, 8 s): Pushes the opponent and projectiles away.
- **Ultimate — Singularity Collapse:** Creates a singularity, drags the opponent in, releases a collapsing burst.
- Boss: 3 phases; Phase 3 inverts gravity briefly (fighters float, air moves only) — optional hazard.

---

## 5. Character visual direction

Every fighter shares a "Convergence armor" baseline (stylized, chunky, 6.5-heads tall, readable at phone size) and departs from it with **element shape language**:

| Power | Shape language | Primary / Secondary / Glow |
|---|---|---|
| Lightning (Zyron) | Asymmetric zig-zag plates, one longer shoulder fin | Electric blue #2F7BFF / charcoal #2B2E35 / cyan-white #BFF4FF |
| Fire (Pyra) | Rounded flared gauntlets, ember seams | Copper-red #B4472A / soot #3A2A25 / amber #FFB347 |
| Earth (Terron) | Blocky geometric slabs, wide stance | Bronze #9C7A3C / moss #5E7A3A / warm gold #E6C36A |
| Wind (Aeris) | Curves, fabric fins, crescent blades | Teal #22A6A0 / ivory #F2EEDF / pale mint #C8FFF0 |
| Ice (Kryon) | Faceted translucent shoulders, long straight lines | White #F4F8FF / cobalt #2346A8 / ice blue #9FE3FF |
| Nature (Vyra) | Organic spirals, vine bands | Emerald #2E9E5B / bark #4A3424 / lime #B8FF5C |
| Light (Solen) | Floating geometric panels, halo-free rings | White #FFFFFF / gold #D9A532 / soft yellow #FFF2A8 |
| Shadow (Noxar) | Sharp asymmetric cloak segments, angular mask | Black #121018 / violet #5B2A86 / magenta-violet #C06BFF |
| Magma (Magrox) | Massive cracked slabs, oversized fists | Obsidian #1B1A1F / basalt #3B3533 / lava orange #FF6A1A |
| Crystal (Crystara) | Faceted blades, diamond motifs | Violet #7C4DCC / lilac #C9B5FF / prism pink #FF9BE8 |
| Gravity (Gravox) | Mechanical plates, floating orbital rings | Indigo #2C2A6B / gunmetal #4A4F5C / deep blue-white #9AA8FF |

Full per-fighter briefs (front/side/back views, faces, weapons, rig and VFX lists) are tracked in doc 04 and will be produced per milestone.

---

## 6. Game modes

### 6.1 Free Combat (priority 1)
Player picks fighter, opponent (or Random), arena (or Random), format, timer, AI difficulty, affinity ON/OFF. Starts immediately. Tracks win count for unlocks.

### 6.2 Training (priority 2)
Dummy behaviors: Stand, Guard all, Guard random, Dodge random, Jump, CPU (Easy/Normal/Hard). Toggles: infinite HP, infinite EP, full ultimate, show hitboxes, frame data overlay, input display, combo counter, damage numbers, reset position. Move list overlay.

### 6.3 Story Combat — "The Shattered Core" (priority 3)

**Setting:** Aetheris, a world of floating realms held together by eleven elemental Cores. A century ago the Prime Core shattered; its fragments now power the realms. The **Convergence Trials** are an ancient tournament where elemental fighters compete to become Core Wardens.

**Premise:** Zyron enters the Trials looking to prove himself. Noxar, a former Warden candidate who was rejected, is secretly using the Trials to gather the champions' Core energy and re-forge the Prime Core under his own control.

Each chapter: 3–4 fights, short dialogue scenes (portrait + text, skippable), one boss. Player character is chosen from the hero roster per fight, with suggested heroes for story beats.

| Ch. | Title | Location | Beats | Boss / Unlock |
|---|---|---|---|---|
| 1 | The Convergence Begins | Molten Crater | Heroes meet as rivals in the opening rounds (Zyron vs Pyra, Terron vs Aeris). Magrox crashes the arena claiming the Trials are rigged. | Magrox → unlock Magrox |
| 2 | Trial of the Elements | Frost Citadel | Elemental trials; Kryon and Vyra suspect someone is siphoning energy from defeated fighters. Rival rematches. | Kryon (rival test, not villain) → unlock Training extras |
| 3 | The Crystal Conspiracy | Frost Citadel → Sky Sanctuary | Crystara is found harvesting fragments; she reveals she is working for "the one who sees beneath the Trials". | Crystara → unlock Crystara |
| 4 | The Gravity Fortress | Sky Sanctuary → Void Nexus | Heroes storm Gravox's orbiting fortress that holds collected Core energy. Solen joins. | Gravox (3 phases) → unlock Gravox |
| 5 | Eclipse of the Core | Void Nexus | Noxar reveals himself, begins the eclipse. Heroes face his clones, then Noxar himself. Ending: the heroes choose to keep the Core shattered and shared. | Noxar (2 phases) → unlock Noxar + credits |

Sample dialogue tone (original):
> **Zyron:** "Fast is fine. Faster is better. Let's go!"
> **Noxar:** "You fight for the Trials. I fight for what comes after them."
> **Terron:** "Stand behind me. Then stand beside me."

Full script is a Phase 6 deliverable.

---

## 7. Arenas

All arenas: circular or octagonal fighting floor 22–26 m across, invisible boundary wall with visible edge VFX, no ring-outs in v1, baked lighting + 1 realtime directional light, hazards OFF by default.

| Arena | Look | Readability rule | Optional hazard |
|---|---|---|---|
| Molten Crater | Basalt platform over a lava lake, orange rim light, heat-haze particles at edges only | Floor is dark grey so orange/fire VFX still pop | Lava vents erupt on a timer at marked spots |
| Frost Citadel | Icy courtyard of crystalline towers, light snowfall, glossy floor (fake reflections via baked probe) | Floor desaturated blue-grey, no full mirror reflections | Ice patches reduce friction |
| Sky Sanctuary | Floating stone ring above clouds, broken arches, drifting islands in background | Warm midday light, high contrast against sky | Gusts push fighters slightly |
| Void Nexus | Dark cosmic platform, floating dark crystals, slow nebula skybox | Bright rim light on fighters so they read against dark background | Gravity pulses pull toward center |

The prototype uses one arena (Sky Sanctuary greybox, the most neutral backdrop for both blue and violet effects).

---

## 8. Enemy AI

### 8.1 Structure
- Utility-scored decision layer on top of a state machine (Approach, Pressure, Defend, Retreat/Zone, Punish, Recover).
- The AI uses the **same command interface** as the player (it "presses buttons"), so it obeys the same cooldowns, EP, and frame data.
- **Perception delay:** The AI only sees the world through a buffered snapshot that is `reactionTime` old. It never reads player input directly.

### 8.2 Difficulty profiles (`AIProfile` assets)

| Setting | Easy | Normal | Hard |
|---|---|---|---|
| Reaction time | 450–600 ms | 250–350 ms | 170–230 ms (never below human-plausible) |
| Max combo length | 2 hits | 3 hits + special | Full chains + cancels |
| Guard chance vs telegraphed attack | 15% | 40% | 65% |
| Dodge chance | 5% | 20% | 40% (prefers perfect timing 30% of the time) |
| Uses specials | Rarely, random | When in range | When it can confirm or punish |
| Uses defensive ability | Never | Sometimes | Reads patterns (e.g. after 2 repeated attacks) |
| Punishes whiffs | No | 30% | 75% |
| Idle pauses | Frequent (0.6–1.2 s) | Occasional | Rare |
| Spacing | Walks straight in | Keeps preferred range | Holds optimal range per fighter, baits |

### 8.3 Bosses
Boss behavior = fighter + `BossPhase` list (HP threshold, new AIProfile, unlocked moves, arena event). Every new phase starts with a 1.5 s roar/pose that is invulnerable but clearly telegraphed.

---

## 9. UI and controls

### 9.1 HUD (landscape, 16:9 reference 1920×1080, safe area aware)

```
┌──────────────────────────────────────────────────────────────────────────────┐
│ [P1 portrait] ███████████ HP           ⏸   99   ●○        HP ███████████ [P2] │
│               ▓▓▓▓▓▓ EP  ◔ ULT 64%                    ULT 30% ◔  EP ▓▓▓▓▓▓    │
│                                                                              │
│                                                                              │
│      (camera swipe area: right 55% of screen above buttons)                  │
│                                                                              │
│                                                           [S2]  [ULT]        │
│    ◯                                                  [S1]   [H]             │
│   (◉)  joystick (floating, left 40%)              [G]    [L]                 │
│    ◯                                                 [D]                     │
└──────────────────────────────────────────────────────────────────────────────┘
●○ = round wins   ◔ = radial meter   [D] Dodge [G] Guard [L] Light [H] Heavy
```
- Light is the largest button under the right thumb's resting position. Heavy, S1, S2 arc around it. Dodge and Guard sit to the left of the arc. Ultimate top-right, pulses at 100%.
- Defensive ability: Guard + Dodge together, or a separate button in "Advanced layout".
- Combo counter and damage numbers appear near the attacker's side.

### 9.2 Menus (wireframe list)
1. **Title / Main menu:** Free Combat · Story · Training · Settings · Credits.
2. **Character Select:** 11-card grid (4-4-3) on the left; right panel shows 3D rotating model, name, element icon, class, stat bars, S1/S2/Defense/Ultimate names, lock state and unlock condition. Confirm → Opponent Select (same screen, P2 side).
3. **Arena Select:** 4 large cards with thumbnail and optional hazard toggle.
4. **Match Settings:** Format, timer, AI difficulty, affinity. "FIGHT" button.
5. **Result:** Winner pose, stats (damage dealt, max combo, perfect dodges), Rematch / Character Select / Main Menu.
6. **Settings:** Controls (button size 80–140%, opacity, layout editor drag-and-drop, joystick fixed/floating, camera sensitivity X/Y, invert), Graphics (Low/Medium/High, 30/60 FPS, render scale, shadows, post-processing), Audio (master, music, SFX, voice), Gameplay (vibration, damage numbers, show input hints).

### 9.3 Camera
Third-person, behind and above the player, auto-framing both fighters (target group). Swipe orbits horizontally ±60° around the default; auto-recenters after 1.5 s idle. Pulls back as fighters separate (FOV 50–62). Ultimate cinematics use a short Cinemachine sequence, skippable with a setting.

---

## 10. Progression and save
- Local JSON save in `Application.persistentDataPath`: unlocks, Free Combat win counts, Story progress, settings, custom button layout.
- All 7 heroes unlocked from the start. Villains unlock via Story **or** Free Combat milestones (5 / 10 / 15 / 20 wins).
- No accounts, no network, no ads, no IAP in v1.

---

## 11. Audio direction
- Music: one theme per arena + menu theme + boss variant. Energetic electronic/orchestral hybrid. Source TBD (doc 06).
- SFX: per-element hit families (light, heavy, special, ultimate), guard, dodge whoosh, perfect-dodge chime, UI clicks, announcer lines ("Round 1", "Fight", "K.O.", "Time"). Original recordings or CC0.

---

## 12. Out of scope for v1
Online multiplayer, 2v2, cosmetics/skins, in-app purchases, ads, cloud saves, ring-outs, controller remapping beyond touch layout, localization beyond English (structure ready, text not translated).
