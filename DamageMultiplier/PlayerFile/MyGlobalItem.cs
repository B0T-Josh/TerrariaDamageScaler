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
        public static Dictionary<int, int> ProjectileToItemTypeMap = new Dictionary<int, int>();
        public static Dictionary<int, int> BuffToItemTypeMap = new Dictionary<int, int>();

        public override void UpdateInventory(Item item, Player player)
        {
            TryCaptureReforgeMultiplier(item, player);
        }

        private void TryCaptureReforgeMultiplier(Item item, Player player)
        {
            if (item.IsAir) return;

            var modPlayer = player.GetModPlayer<MyModPlayer>();
            string normalizedName = DamageMultiplierScale.NormalizeName(ContentSamples.ItemsByType[item.type].Name);

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
            return modPlayer.reforgeMultipliers.TryGetValue(normalizedWeaponName, out float multiplier) ? multiplier : 1f;
        }

        public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
        {
            var modPlayer = player.GetModPlayer<MyModPlayer>();
            string normalizedName = DamageMultiplierScale.NormalizeName(ContentSamples.ItemsByType[item.type].Name);

            if (modPlayer.WeaponConfigs.TryGetValue(normalizedName, out WeaponScaleData config) && config.IsEnabled)
            {
                float scaledBaseDamage = GetProgressionBaseDamage(player, item);
                damage.Base += (scaledBaseDamage - item.damage);
            }
        }

        public static float GetProgressionBaseDamage(Player player, Item weapon)
        {
            var modPlayer = player.GetModPlayer<MyModPlayer>();
            string normalizedName = DamageMultiplierScale.NormalizeName(ContentSamples.ItemsByType[weapon.type].Name);

            if (modPlayer.WeaponConfigs.TryGetValue(normalizedName, out WeaponScaleData config))
            {
                if (!config.IsEnabled) return weapon.damage; 

                // FIX: Grab the dynamically scaled HP straight from the tracker
                float targetHP = BossDefeated.CurrentTargetHP;
                
                float damage = (targetHP * 0.0001f) * config.DamageScale;

                if (weapon.DamageType == DamageClass.Magic && weapon.mana > 28)
                {
                    damage *= 2f;
                }
                
                damage *= GetReforgeMultiplier(player, normalizedName);
                return damage;
            }

            return weapon.damage > 0 ? weapon.damage : 1;
        }

        public static int CalculateTotalDamage(Player player, Item item)
        {
            float baseDamage = GetProgressionBaseDamage(player, item);
            StatModifier modifier = player.GetTotalDamage(item.DamageType);
            return (int)Math.Round(modifier.ApplyTo(baseDamage));
        }

        public static int CalculateTotalDamageByType(Player player, int itemType)
        {
            if (itemType <= 0) return 1;
            Item weapon = ContentSamples.ItemsByType[itemType];
            if (weapon == null || weapon.IsAir) return 1;

            float baseDamage = GetProgressionBaseDamage(player, weapon);
            StatModifier modifier = player.GetTotalDamage(weapon.DamageType);
            
            return (int)Math.Round(modifier.ApplyTo(baseDamage));
        }

        public static int CalculateMinionBaseDamageByType(Player player, int itemType)
        {
            if (itemType <= 0) return 1;
            Item weapon = ContentSamples.ItemsByType[itemType];
            if (weapon == null || weapon.IsAir) return 1;

            float baseDamage = GetProgressionBaseDamage(player, weapon);
            return (int)Math.Round(baseDamage);
        }
        
        public static string GetCurrentHPReferenceBoss()
        {
            // FIX: Grab the string straight from the tracker
            return BossDefeated.CurrentTargetName;
        }
    }
}