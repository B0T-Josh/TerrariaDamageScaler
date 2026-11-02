using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DamageMultiplier.PlayerFile
{   
    public class MyGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;
        private int lastSeenPrefix = -1;

        public static Dictionary<string, int> ItemNameToIdMap = new Dictionary<string, int>();
        // Reverse lookup to track buff-spawned minions back to their original weapon
        public static Dictionary<int, string> ProjectileToItemMap = new Dictionary<int, string>();

        public override void Load()
        {
            ItemNameToIdMap.Clear();
            ProjectileToItemMap.Clear();
            foreach (var pair in ContentSamples.ItemsByType)
            {
                if (!pair.Value.IsAir && !string.IsNullOrEmpty(pair.Value.Name))
                {
                    string norm = DamageMultiplierScale.NormalizeName(pair.Value.Name);
                    if (!ItemNameToIdMap.ContainsKey(norm))
                    {
                        ItemNameToIdMap[norm] = pair.Key;
                    }
                    
                    // Maps the projectile ID back to the weapon name
                    if (pair.Value.shoot > 0 && !ProjectileToItemMap.ContainsKey(pair.Value.shoot))
                    {
                        ProjectileToItemMap[pair.Value.shoot] = norm;
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

            if (!modPlayer.WeaponConfigs.TryGetValue(normalizedName, out WeaponScaleData config) || !config.IsEnabled)
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

            if (modPlayer.WeaponConfigs.TryGetValue(normalizedName, out WeaponScaleData config) && config.IsEnabled)
            {
                float scaledBaseDamage = GetProgressionBaseDamage(player, item);
                damage.Base += (scaledBaseDamage - item.damage);
            }
        }

        public static float GetProgressionBaseDamage(Player player, Item weapon)
        {
            var modPlayer = player.GetModPlayer<MyModPlayer>();
            string normalizedName = DamageMultiplierScale.NormalizeName(weapon.Name);

            if (modPlayer.WeaponConfigs.TryGetValue(normalizedName, out WeaponScaleData config))
            {
                if (!config.IsEnabled) 
                {
                    return weapon.damage; 
                }

                float highestHP = 0;
                foreach (var boss in BossDefeated.OrderedBosses)
                {
                    if (boss.IsDowned.Invoke() && boss.MaxHP > highestHP)
                    {
                        highestHP = boss.MaxHP;
                    }
                }

                // FIX: If no bosses are defeated, use the Max HP of the first boss in the progression list.
                if (highestHP == 0 && BossDefeated.OrderedBosses.Count > 0)
                {
                    highestHP = BossDefeated.OrderedBosses[0].MaxHP;
                }
                
                // Calculate scaled damage using the boss HP
                float damage = (highestHP * 0.001f) * config.DamageScale;

                // Apply magic & reforge multipliers (config removed, using hardcoded 2x)
                if (weapon.DamageType == DamageClass.Magic && weapon.mana > 28)
                {
                    damage *= 2f;
                }
                
                damage *= GetReforgeMultiplier(player, normalizedName);
                return damage;
            }

            // Default return if weapon is not in the scaling system at all
            return weapon.damage > 0 ? weapon.damage : 1;
        }

        public static int CalculateTotalDamage(Player player, Item item)
        {
            float baseDamage = GetProgressionBaseDamage(player, item);
            StatModifier modifier = player.GetTotalDamage(item.DamageType);
            return (int)Math.Round(modifier.ApplyTo(baseDamage));
        }

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

        // Fetches purely the raw scaled damage so minions don't double-dip player stats
        public static int CalculateMinionBaseDamageByName(Player player, string itemName)
        {
            if (!ItemNameToIdMap.TryGetValue(itemName, out int itemId))
                return 1;

            Item weapon = ContentSamples.ItemsByType[itemId];
            if (weapon == null || weapon.IsAir) 
                return 1;

            float baseDamage = GetProgressionBaseDamage(player, weapon);
            return (int)Math.Round(baseDamage);
        }
    }
}