using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DamageMultiplier.PlayerFile
{   
    public class MyGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;
        private int lastSeenPrefix = -1;

        // Cached lookup map: NormalizedName -> ItemID
        public static Dictionary<string, int> ItemNameToIdMap = new Dictionary<string, int>();

        public override void Load()
        {
            ItemNameToIdMap.Clear();
            foreach (var pair in ContentSamples.ItemsByType)
            {
                if (!pair.Value.IsAir && !string.IsNullOrEmpty(pair.Value.Name))
                {
                    string norm = DamageMultiplierScale.NormalizeName(pair.Value.Name);
                    if (!ItemNameToIdMap.ContainsKey(norm))
                    {
                        ItemNameToIdMap[norm] = pair.Key;
                    }
                }
            }
        }

        public override void UpdateInventory(Item item, Player player)
        {
            TryCaptureReforgeMultiplier(item, player);
        }

        private void TryCaptureReforgeMultiplier(Item item, Player player)
        {
            if (item.IsAir) return;

            var modPlayer = player.GetModPlayer<MyModPlayer>();
            string normalizedName = DamageMultiplierScale.NormalizeName(item.Name);

            if (!modPlayer.playerWeapons.Contains(normalizedName))
                return;

            if (item.prefix == lastSeenPrefix)
                return;

            lastSeenPrefix = item.prefix;
            modPlayer.reforgeMultipliers[normalizedName] = GetPrefixDamageMultiplier(item);
        }

        private static float GetPrefixDamageMultiplier(Item item)
        {
            if (item.prefix <= 0) return 1f;

            Item baseline = new Item();
            baseline.SetDefaults(item.type);

            if (baseline.damage <= 0) return 1f;

            return item.damage / (float)baseline.damage;
        }

        private static float GetReforgeMultiplier(Player player, string normalizedWeaponName)
        {
            var modPlayer = player.GetModPlayer<MyModPlayer>();
            return modPlayer.reforgeMultipliers.TryGetValue(normalizedWeaponName, out float multiplier)
                ? multiplier
                : 1f;
        }

        public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
        {
            var modPlayer = player.GetModPlayer<MyModPlayer>();
            string normalizedName = DamageMultiplierScale.NormalizeName(item.Name);

            if (modPlayer.playerWeapons.Contains(normalizedName))
            {
                float scaledBaseDamage = GetProgressionBaseDamage(player, item);
                damage.Base += (scaledBaseDamage - item.damage);
            }
        }

        public static float GetProgressionBaseDamage(Player player, Item weapon)
        {
            int attackSpeed = weapon.useTime;
            float damage;
            var bossList = BossDefeated.OrderedBosses;
            var config = ModContent.GetInstance<DamageMultiplierConfig>();

            float highestHP = 0;
            bool allDefeated = true;

            foreach (var boss in bossList)
            {
                float bossHP = boss.MaxHP; // Instant cached lookup
                
                if (bossHP > highestHP) highestHP = bossHP;

                if (!boss.IsDowned.Invoke())
                {
                    allDefeated = false;
                    break; 
                }
            }

            if (highestHP <= 0)
            {
                int baseDamage = ContentSamples.ItemsByType.TryGetValue(weapon.type, out Item defaultWeapon) ? defaultWeapon.damage : weapon.damage;
                return baseDamage > 0 ? baseDamage : 1;
            }

            if (!allDefeated)
            {
                if (attackSpeed <= 8) damage = highestHP * config.ProgSpeed1;
                else if (attackSpeed >= 9 && attackSpeed <= 25) damage = highestHP * config.ProgSpeed2;
                else if (attackSpeed >= 26 && attackSpeed <= 35) damage = highestHP * config.ProgSpeed3;
                else if (attackSpeed >= 36) damage = highestHP * config.ProgSpeed4;
                else damage = 1;
            }
            else
            {
                if (attackSpeed <= 8) damage = highestHP * config.PostSpeed1;
                else if (attackSpeed >= 9 && attackSpeed <= 25) damage = highestHP * config.PostSpeed2;
                else if (attackSpeed >= 26 && attackSpeed <= 35) damage = highestHP * config.PostSpeed3;
                else if (attackSpeed >= 36) damage = highestHP * config.PostSpeed4;
                else damage = 1;
            }

            if (weapon.DamageType == DamageClass.Magic && weapon.mana > 28) 
            {
                damage *= config.MagicMultiplier;
            }

            float multiplier = GetReforgeMultiplier(player, DamageMultiplierScale.NormalizeName(weapon.Name));
            damage *= multiplier;

            return damage;
        }

        public static int CalculateTotalDamage(Player player, Item item)
        {
            float baseDamage = GetProgressionBaseDamage(player, item);
            StatModifier modifier = player.GetTotalDamage(item.DamageType);
            return (int)Math.Round(modifier.ApplyTo(baseDamage));
        }

        // Instant O(1) Dictionary Lookup
        public static int CalculateTotalDamageByName(Player player, string itemName)
        {
            if (!ItemNameToIdMap.TryGetValue(itemName, out int itemId))
                return 1;

            Item weapon = ContentSamples.ItemsByType[itemId];
            if (weapon == null || weapon.IsAir) 
                return 1;

            float baseDamage = GetProgressionBaseDamage(player, weapon);
            StatModifier modifier = player.GetTotalDamage(weapon.DamageType);
            
            return (int)Math.Round(modifier.ApplyTo(baseDamage));
        }
    }
}