using Project_1.GameObjects;
using Project_1.Items;
using Project_1.Items.SubTypes;
using Project_1.Messaging.Events;
using Project_1.World.GameObjects.Unit.Stats.Secondary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Project_1.Managers
{
    internal static class StatValidation
    {
        static readonly int[] hashSeeds = { 0, 1, -1, 123456789, -987654321 };
        static bool hasRun;
        static string lastResultMessage = string.Empty;

        public static bool TryRun(out string aMessage)
        {
            if (hasRun)
            {
                aMessage = lastResultMessage;
                return true;
            }

            ItemData[] allItemData = ItemFactory.GetAllItemDataSnapshot();
            if (allItemData.Length == 0)
            {
                aMessage = string.Empty;
                return false;
            }

            try
            {
                ValidateEquipUnequipRoundTrip(allItemData);
                ValidateSuffixStatBinding(allItemData);
                ValidateSpellBasePlusSchoolAggregation();
                ValidateChanceBounds();
                lastResultMessage = "StatValidation: PASS (round-trip, suffix binding, spell base+school, normalized chance bounds).";
            }
            catch (Exception ex)
            {
                lastResultMessage = $"StatValidation: FAIL - {ex.GetType().Name}: {ex.Message}";
            }

            hasRun = true;
            aMessage = lastResultMessage;
            return true;
        }

        static void ValidateEquipUnequipRoundTrip(ItemData[] aAllItemData)
        {
            for (int i = 0; i < aAllItemData.Length; i++)
            {
                if (aAllItemData[i] is not EquipmentData equipmentData)
                {
                    continue;
                }

                for (int seedIndex = 0; seedIndex < hashSeeds.Length; seedIndex++)
                {
                    int hash = unchecked(hashSeeds[seedIndex] + equipmentData.ID * 73856093);
                    Items.SubTypes.Equipment original = CreateEquipmentWithHash(equipmentData, hash);
                    Items.SubTypes.Equipment firstRoundTrip = RoundTripViaSnapshot(original);
                    Items.SubTypes.Equipment secondRoundTrip = RoundTripViaSnapshot(firstRoundTrip);

                    AssertSameEquipmentPayload(original, firstRoundTrip, equipmentData.Name, hash, "first");
                    AssertSameEquipmentPayload(original, secondRoundTrip, equipmentData.Name, hash, "second");
                }
            }
        }

        static Items.SubTypes.Equipment RoundTripViaSnapshot(Items.SubTypes.Equipment aEquipment)
        {
            ItemUiSnapshot snapshot = ItemUiSnapshot.FromItem(aEquipment);
            Item rebuilt = snapshot.ToItem();
            if (rebuilt is not Items.SubTypes.Equipment rebuiltEquipment)
            {
                throw new InvalidOperationException($"Snapshot round-trip did not recreate equipment for item ID {aEquipment.ID}.");
            }

            return rebuiltEquipment;
        }

        static Items.SubTypes.Equipment CreateEquipmentWithHash(EquipmentData aData, int aHash)
        {
            if (aData is WeaponData weaponData)
            {
                return new Weapon(weaponData, aHash);
            }

            return new Items.SubTypes.Equipment(aData, aHash);
        }

        static void AssertSameEquipmentPayload(
            Items.SubTypes.Equipment aExpected,
            Items.SubTypes.Equipment aActual,
            string aItemName,
            int aHash,
            string aRoundTripLabel)
        {
            if (aExpected.Hash != aActual.Hash)
            {
                throw new InvalidOperationException($"Hash changed on {aRoundTripLabel} round-trip for '{aItemName}' (seed {aHash}).");
            }

            if (!string.Equals(aExpected.Name, aActual.Name, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Name changed on {aRoundTripLabel} round-trip for '{aItemName}' (seed {aHash}).");
            }

            if (aExpected.Stats.Armor != aActual.Stats.Armor)
            {
                throw new InvalidOperationException($"Armor changed on {aRoundTripLabel} round-trip for '{aItemName}' (seed {aHash}).");
            }

            int[] expectedPrimary = aExpected.Stats.Stats;
            int[] actualPrimary = aActual.Stats.Stats;
            if (expectedPrimary.Length != actualPrimary.Length)
            {
                throw new InvalidOperationException($"Primary stat length changed on {aRoundTripLabel} round-trip for '{aItemName}' (seed {aHash}).");
            }

            for (int i = 0; i < expectedPrimary.Length; i++)
            {
                if (expectedPrimary[i] != actualPrimary[i])
                {
                    throw new InvalidOperationException($"Primary stat index {i} changed on {aRoundTripLabel} round-trip for '{aItemName}' (seed {aHash}).");
                }
            }

            AssertSuffixEqual(aExpected.Suffix, aActual.Suffix, aItemName, aHash, aRoundTripLabel);

            string expectedSecondaryInt = CanonicalSecondaryInt(aExpected.SecondaryStatsInt);
            string actualSecondaryInt = CanonicalSecondaryInt(aActual.SecondaryStatsInt);
            if (!string.Equals(expectedSecondaryInt, actualSecondaryInt, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Secondary int stats changed on {aRoundTripLabel} round-trip for '{aItemName}' (seed {aHash}).");
            }

            string expectedSecondaryFloat = CanonicalSecondaryFloat(aExpected.SecondaryStatsFloat);
            string actualSecondaryFloat = CanonicalSecondaryFloat(aActual.SecondaryStatsFloat);
            if (!string.Equals(expectedSecondaryFloat, actualSecondaryFloat, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Secondary float stats changed on {aRoundTripLabel} round-trip for '{aItemName}' (seed {aHash}).");
            }
        }

        static void AssertSuffixEqual(
            (string name, (EquipmentData.StatBonuses stat, int value)[] stats) aExpected,
            (string name, (EquipmentData.StatBonuses stat, int value)[] stats) aActual,
            string aItemName,
            int aHash,
            string aRoundTripLabel)
        {
            if (!string.Equals(aExpected.name, aActual.name, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Suffix name changed on {aRoundTripLabel} round-trip for '{aItemName}' (seed {aHash}).");
            }

            int expectedLength = aExpected.stats?.Length ?? 0;
            int actualLength = aActual.stats?.Length ?? 0;
            if (expectedLength != actualLength)
            {
                throw new InvalidOperationException($"Suffix stat count changed on {aRoundTripLabel} round-trip for '{aItemName}' (seed {aHash}).");
            }

            for (int i = 0; i < expectedLength; i++)
            {
                if (aExpected.stats[i].stat != aActual.stats[i].stat || aExpected.stats[i].value != aActual.stats[i].value)
                {
                    throw new InvalidOperationException($"Suffix stat index {i} changed on {aRoundTripLabel} round-trip for '{aItemName}' (seed {aHash}).");
                }
            }
        }

        static string CanonicalSecondaryInt(SecondayStatBonus<int>[] aStats)
        {
            if (aStats == null || aStats.Length == 0)
            {
                return string.Empty;
            }

            return string.Join("|", aStats
                .OrderBy(x => x.SecondaryStat, StringComparer.Ordinal)
                .ThenBy(x => x.Value)
                .Select(x => $"{x.SecondaryStat}:{x.Value.ToString(CultureInfo.InvariantCulture)}"));
        }

        static string CanonicalSecondaryFloat(SecondayStatBonus<float>[] aStats)
        {
            if (aStats == null || aStats.Length == 0)
            {
                return string.Empty;
            }

            return string.Join("|", aStats
                .OrderBy(x => x.SecondaryStat, StringComparer.Ordinal)
                .ThenBy(x => x.Value)
                .Select(x => $"{x.SecondaryStat}:{x.Value.ToString("R", CultureInfo.InvariantCulture)}"));
        }

        static void ValidateSuffixStatBinding(ItemData[] aAllItemData)
        {
            for (int i = 0; i < aAllItemData.Length; i++)
            {
                if (aAllItemData[i] is not EquipmentData equipmentData || equipmentData.Quality != Item.Quality.Uncommon)
                {
                    continue;
                }

                for (int seedIndex = 0; seedIndex < hashSeeds.Length; seedIndex++)
                {
                    int hash = unchecked(hashSeeds[seedIndex] + equipmentData.ID * 19349663);
                    var suffix = equipmentData.Suffix(hash);
                    if (string.IsNullOrWhiteSpace(suffix.name) || suffix.stats == null || suffix.stats.Length == 0)
                    {
                        throw new InvalidOperationException($"Uncommon item '{equipmentData.Name}' generated empty suffix for hash {hash}.");
                    }

                    if (!EquipmentData.IsValidSuffixDefinition(suffix, out string error))
                    {
                        throw new InvalidOperationException($"Invalid suffix definition for '{equipmentData.Name}' hash {hash}: {error}");
                    }

                    int suffixId = equipmentData.SuffixID(hash);
                    if (suffixId < 0)
                    {
                        throw new InvalidOperationException($"Uncommon item '{equipmentData.Name}' produced invalid suffix ID for hash {hash}.");
                    }

                    for (int statIndex = 0; statIndex < suffix.stats.Length; statIndex++)
                    {
                        int valueFromAccessor = equipmentData.SuffixStatValue(statIndex, hash);
                        if (suffix.stats[statIndex].value != valueFromAccessor)
                        {
                            throw new InvalidOperationException($"Suffix stat mismatch for '{equipmentData.Name}' hash {hash}, stat index {statIndex}.");
                        }

                        if (valueFromAccessor <= 0)
                        {
                            throw new InvalidOperationException($"Suffix stat rolled non-positive value for '{equipmentData.Name}' hash {hash}, stat index {statIndex}.");
                        }
                    }

                    equipmentData.AssertSuffixDeterminism(hash);
                }
            }
        }

        static void ValidateSpellBasePlusSchoolAggregation()
        {
            Spell spell = new Spell(new[]
            {
                SpellStats.CreateForValidation(SpellSchool.Base, aSpellDamage: 10, aCritChance: 0.6, aFlatPenetration: 3, aPercentPenetration: 0.2, aBonusHitChance: 0.1),
                SpellStats.CreateForValidation(SpellSchool.Fire, aSpellDamage: 7, aCritChance: 0.15, aFlatPenetration: 2, aPercentPenetration: 0.05, aBonusHitChance: 0.07)
            });

            AssertEqual(17, spell.SpellDamageForSchool(SpellSchool.Fire), "SpellDamageForSchool should include base + school.");
            AssertEqual(5, spell.FlatPenetrationForSchool(SpellSchool.Fire), "FlatPenetrationForSchool should include base + school.");
            AssertNear(0.25, spell.PercentPenetrationForSchool(SpellSchool.Fire), 0.0001, "PercentPenetrationForSchool should include base + school.");
            AssertNear(0.17, spell.BonusHitChanceForSchool(SpellSchool.Fire), 0.0001, "BonusHitChanceForSchool should include base + school.");

            HashSet<SpellSchool> twoSchools = new HashSet<SpellSchool> { SpellSchool.Fire, SpellSchool.Frost };
            AssertEqual(14, spell.SpellDamageForSchools(twoSchools), "SpellDamageForSchools should average base+school values.");
            AssertEqual(4, spell.FlatPenetrationForSchools(twoSchools), "FlatPenetrationForSchools should average base+school values.");
            AssertNear(0.225, spell.PercentPenetrationForSchools(twoSchools), 0.0001, "PercentPenetrationForSchools should average base+school values.");
            AssertNear(0.135, spell.BonusHitChanceForSchools(twoSchools), 0.0001, "BonusHitChanceForSchools should average base+school values.");
        }

        static void ValidateChanceBounds()
        {
            Spell clampedHigh = new Spell(new[]
            {
                SpellStats.CreateForValidation(SpellSchool.Base, aCritChance: 2.5, aPercentPenetration: 3.0, aBonusHitChance: 2.0),
                SpellStats.CreateForValidation(SpellSchool.Shadow, aCritChance: 1.5, aPercentPenetration: 4.0, aBonusHitChance: 3.0)
            });

            Spell clampedLow = new Spell(new[]
            {
                SpellStats.CreateForValidation(SpellSchool.Base, aCritChance: -2.5, aPercentPenetration: -3.0, aBonusHitChance: -2.0),
                SpellStats.CreateForValidation(SpellSchool.Arcane, aCritChance: -1.5, aPercentPenetration: -4.0, aBonusHitChance: -3.0)
            });

            AssertNear(1, clampedHigh.CriticalChanceForSchool(SpellSchool.Shadow), 0.0001, "Spell crit chance should clamp to 1.");
            AssertNear(1, clampedHigh.PercentPenetrationForSchool(SpellSchool.Shadow), 0.0001, "Spell penetration chance should clamp to 1.");
            AssertNear(1, clampedHigh.BonusHitChanceForSchool(SpellSchool.Shadow), 0.0001, "Spell hit chance should clamp to 1.");

            AssertNear(0, clampedLow.CriticalChanceForSchool(SpellSchool.Arcane), 0.0001, "Spell crit chance should clamp to 0.");
            AssertNear(0, clampedLow.PercentPenetrationForSchool(SpellSchool.Arcane), 0.0001, "Spell penetration chance should clamp to 0.");
            AssertNear(0, clampedLow.BonusHitChanceForSchool(SpellSchool.Arcane), 0.0001, "Spell hit chance should clamp to 0.");

            if (ObjectManager.Player == null)
            {
                return;
            }

            var attackStats = ObjectManager.Player.SecondaryStats.Attack;
            EnsureZeroToOne(attackStats.CriticalChance, "Player attack crit chance");
            EnsureZeroToOne(attackStats.BonusHitChance, "Player attack bonus hit chance");
            EnsureZeroToOne(attackStats.PercentPenetration, "Player attack percent penetration");
            EnsureZeroToOne(attackStats.Vampirism, "Player attack vampirism");

            var spellStats = ObjectManager.Player.SecondaryStats.Spell;
            foreach (SpellSchool school in Enum.GetValues(typeof(SpellSchool)))
            {
                EnsureZeroToOne(spellStats.CriticalChanceForSchool(school), $"Player spell crit chance ({school})");
                EnsureZeroToOne(spellStats.PercentPenetrationForSchool(school), $"Player spell percent penetration ({school})");
                EnsureZeroToOne(spellStats.BonusHitChanceForSchool(school), $"Player spell bonus hit chance ({school})");
            }
        }

        static void EnsureZeroToOne(double aValue, string aContext)
        {
            if (double.IsNaN(aValue) || aValue < 0 || aValue > 1)
            {
                throw new InvalidOperationException($"{aContext} is outside [0,1]: {aValue.ToString(CultureInfo.InvariantCulture)}");
            }
        }

        static void AssertEqual(int aExpected, int aActual, string aMessage)
        {
            if (aExpected != aActual)
            {
                throw new InvalidOperationException($"{aMessage} Expected {aExpected}, got {aActual}.");
            }
        }

        static void AssertNear(double aExpected, double aActual, double aTolerance, string aMessage)
        {
            if (Math.Abs(aExpected - aActual) > aTolerance)
            {
                throw new InvalidOperationException($"{aMessage} Expected {aExpected.ToString(CultureInfo.InvariantCulture)}, got {aActual.ToString(CultureInfo.InvariantCulture)}.");
            }
        }
    }
}
