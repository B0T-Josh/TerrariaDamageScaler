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

            float highestHP = 0;
            bool allDefeated = true;

            foreach (var boss in bossList)
            {
                int primaryNpcId = boss.NpcIDs.First();
                float bossHP = DamageMultiplierScale.GetBossScaleHP(primaryNpcId);
                
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
                if (attackSpeed < 10) damage = highestHP * 0.001f;
                else if (attackSpeed >= 10 && attackSpeed < 20) damage = highestHP * 0.002f;
                else if (attackSpeed >= 20 && attackSpeed < 30) damage = highestHP * 0.003f;
                else if (attackSpeed >= 30) damage = highestHP * 0.004f;
                else damage = 1;
            }
            else
            {
                if (attackSpeed < 10) damage = highestHP * 0.05f;
                else if (attackSpeed >= 10 && attackSpeed < 20) damage = highestHP * 0.2f;
                else if (attackSpeed >= 20 && attackSpeed < 30) damage = highestHP * 0.3f;
                else if (attackSpeed >= 30) damage = highestHP * 0.5f;
                else damage = 1;
            }

            if (weapon.DamageType == DamageClass.Magic && weapon.mana > 28) 
            {
                damage *= 3f;
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

        // FIX: Directly searches ContentSamples.ItemsByType instead of relying on fragile weaponName dictionary
        public static int CalculateTotalDamageByName(Player player, string itemName)
        {
            Item weapon = null;
            
            foreach (var pair in ContentSamples.ItemsByType)
            {
                if (DamageMultiplierScale.NormalizeName(pair.Value.Name) == itemName)
                {
                    weapon = pair.Value;
                    break;
                }
            }

            if (weapon == null || weapon.IsAir) 
                return 1;

            float baseDamage = GetProgressionBaseDamage(player, weapon);
            StatModifier modifier = player.GetTotalDamage(weapon.DamageType);
            
            return (int)Math.Round(modifier.ApplyTo(baseDamage));
        }
    }
}