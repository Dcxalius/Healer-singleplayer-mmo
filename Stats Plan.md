# Stats System Plan
> Scope: finish stat system cleanup after moving rate values to a consistent `0..1` model.

## 1. Goals (definition of done)
1. All chance/rate doubles/floats use `0..1` (`0%..100%`) everywhere.
2. Attack-driven abilities never scale from spell-only stats.
3. Spell-driven abilities always use spell stats with `base + school` behavior.
4. Item suffix names always match the stats they grant.
5. Legacy/deprecated stat paths are removed or clearly marked and isolated.

## 2. Rules and conventions
1. Any field/property ending in `Chance`, `Percent`, `Vampirism`, `Haste`, `Reduction`, or `Scaler` is treated as `0..1`.
2. Clamp at write/aggregation points (`Math.Clamp(x, 0, 1)`), not only at final consumers.
3. Crit damage is a multiplier (`1.5`, `2.0`, etc.), not a percent.
4. UI/report formatting can convert to `%` for display, but gameplay math stays normalized.

## 3. Status snapshot
1. Completed: core rate normalization pass.
   1. Attack chance/hit/penetration/vampirism clamped to `0..1`.
   2. Spell chance/hit/penetration/haste/vampirism clamped to `0..1`.
   3. School-aggregated spell hit/crit/penetration values clamped to `0..1`.
   4. Hit table entries clamped to `0..1`.
2. In progress: broader system cleanup and validation.
3. Completed: Phase A class data normalization.
   1. All current `.class` files now explicitly define `DodgeScaling`, `BaseDodge`, `MeleeCritScaling`, `SpellCritScaling`, `CanDualWield`, `IsCaster`, `CanParry`.
   2. Class constructors now require explicit `spellCritScaling`.
   3. Mob class constructor now accepts explicit `baseDodge`.
4. In progress: Phase B ability stat-source audit.
   1. Spell effect trigger signature/order has been normalized to `(caster, target)` so instant and projectile casts follow the same path.
   2. `SpellEffect` now exposes explicit stat source (`Spell` vs `Attack`).
   3. `Entity.RecieveSpellAttack` now selects crit/hit channel based on the effect stat source.
5. Completed: spell scalar system baseline.
   1. Direct spell effects now derive scalar from unmodified cast time: instant = `0.4`, `3.5s+` = `1.0`, linear in-between.
   2. DoT/HoT effects now distribute scalar by full duration tick count (`1 / ticks` per tick).

## 4. Remaining phases
### Phase A: Class data normalization
1. Make class files explicit for: `IsCaster`, `MeleeCritScaling`, `SpellCritScaling`, `DodgeScaling`, `BaseDodge`.
2. Remove reliance on constructor defaults for class behavior.
3. Acceptance:
   1. Every `.class` file deserializes with explicit scaling inputs.
   2. No class behavior changes when constructor default values are altered.

### Phase B: Ability stat-source audit
1. Audit all damage/heal/effect application paths and tag each as:
   1. Attack-based
   2. Spell-based
   3. Hybrid (explicitly defined conversion rules)
2. Ensure attack abilities use only `SecondaryStats.Attack` inputs.
3. Ensure spells use `SecondaryStats.Spell` school-aware accessors (`...ForSchool(s)` / `...ForSchools(...)`).
4. Acceptance:
   1. No rogue-style physical ability gains benefit from spell crit/hit.
   2. Caster spells correctly apply `base + school` bonuses.

#### Phase B audit findings (current)
1. Attack-based:
   1. `Entity.AttackTarget/HitTarget/RecieveAttack` (weapon auto attacks and melee hit table).
   2. `Instant` effects with physical damage (`DamageType.Physical`) now route crit/hit through attack secondary stats.
2. Spell-based:
   1. `Instant` effects with non-physical damage.
   2. `OverTime` and `Periodic` effects (through underlying `Instant` effects).
   3. Binary spell resist paths (`SpellResitance`) for spell-sourced effects.
3. Hybrid/bridge behavior:
   1. Physical spell effects enter through `RecieveSpellAttack` but are immediately routed into melee `RecieveAttack` hit outcomes.
   2. Attack-sourced spell effects use melee hittables with custom rules: no glancing, no dual-wield miss penalty.
4. Remaining Phase B gaps:
   1. None currently identified.

### Phase C: Suffix/stat binding integrity
1. Validate all suffix templates map only to matching stat families.
2. Enforce non-zero floor rule for allowed single-stat templates (current +1 rule).
3. Add guardrails so suffix name and rolled stat payload cannot diverge.
4. Acceptance:
   1. No mismatches like `"of Agility"` granting spell-only stats.
   2. No zero-value roll for allowed floor stats.

### Phase D: Remove deprecated stat paths
1. Find old code paths that still read/write obsolete stat fields or pre-green-system assumptions.
2. Replace with one authoritative flow: item data -> equipment aggregation -> secondary stats refresh.
3. Leave explicit TODO markers only where rare/epic systems are intentionally deferred.
4. Acceptance:
   1. No duplicate or competing stat calculation paths remain active.

### Phase E: Validation harness
1. Add targeted tests or deterministic validation routines for:
   1. Equip/unequip does not reroll item stats.
   2. Suffix name always matches rolled stats.
   3. `base + school` spell aggregation.
   4. Normalized chance bounds (`0..1`) under extreme gear/class inputs.
2. Acceptance:
   1. Repeatable checks pass before merge.

## 5. Execution order
1. Phase A
2. Phase B
3. Phase C
4. Phase D
5. Phase E

## 6. Open decisions to resolve during implementation
1. Whether crit damage reduction can ever exceed base crit multiplier (currently clamped by downstream `Math.Max`).
2. How hybrid abilities declare their stat source (data-driven flag vs hardcoded behavior).
3. Whether to display normalized values as raw fractions or percentages in debug/UI reports.
