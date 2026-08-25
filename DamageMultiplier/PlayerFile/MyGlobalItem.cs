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

            if (!modPlayer.playerWeapons.Any(w => DamageMultiplierScale.NormalizeName(w) == normalizedName))
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

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            var player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<MyModPlayer>();

            if (!item.IsAir &&
                modPlayer.playerWeapons.Any(w => DamageMultiplierScale.NormalizeName(w) ==
                                                 DamageMultiplierScale.NormalizeName(item.Name)))
            {
                TryCaptureReforgeMultiplier(item, player);

                int scaledDamage = CalculateDamage(player, item);
                item.damage = scaledDamage;

                foreach (TooltipLine line in tooltips)
                {
                    if (line.Mod == "Terraria" && line.Name == "Damage")
                    {
                        string damageType = item.DamageType.DisplayName.ToString();
                        line.Text = $"{scaledDamage} {damageType} damage";
                        break;
                    }
                }
            }
        }

        private static float GetReforgeMultiplier(Player player, string normalizedWeaponName)
        {
            var modPlayer = player.GetModPlayer<MyModPlayer>();
            return modPlayer.reforgeMultipliers.TryGetValue(normalizedWeaponName, out float multiplier)
                ? multiplier
                : 1f;
        }

        public static int CalculateDamage(Player player, Item item)
        {
            int attackSpeed = item.useTime;
            float damage;
            var bossList = BossDefeated.OrderedBosses;

            foreach (var boss in bossList)
            {
                if (!boss.IsDowned.Invoke())
                {
                    int primaryNpcId = boss.NpcIDs.First();
                    float bossHP = DamageMultiplierScale.GetBossScaleHP(primaryNpcId);
                    
                    if (attackSpeed < 10) damage = bossHP * 0.001f;
                    else if (attackSpeed >= 10 && attackSpeed < 20) damage = bossHP * 0.002f;
                    else if (attackSpeed >= 20 && attackSpeed < 30) damage = bossHP * 0.003f;
                    else if (attackSpeed >= 30) damage = bossHP * 0.004f;
                    else damage = 1;

                    StatModifier modifier = player.GetTotalDamage(item.DamageType);
                    float multiplier = GetReforgeMultiplier(player, DamageMultiplierScale.NormalizeName(item.Name));
                    if(item.DamageType == DamageClass.Magic) {
                        if(item.mana > 28) 
                            return (int)Math.Round(modifier.ApplyTo((damage*5)) * multiplier);
                        else 
                            return (int)Math.Round(modifier.ApplyTo(damage) * multiplier);
                    }
                    return (int)Math.Round(modifier.ApplyTo(damage) * multiplier);
                }
            }

            if (bossList.Count > 0)
            {
                int finalBossId = bossList.Last().NpcIDs.First();
                float finalBossHp = DamageMultiplierScale.GetBossScaleHP(finalBossId);
                
                if (attackSpeed < 10) damage = finalBossHp * 0.05f;
                else if (attackSpeed >= 10 && attackSpeed < 20) damage = finalBossHp * 0.2f;
                else if (attackSpeed >= 20 && attackSpeed < 30) damage = finalBossHp * 0.3f;
                else if (attackSpeed >= 30) damage = finalBossHp * 0.5f;
                else damage = 1;

                StatModifier endModifier = player.GetTotalDamage(item.DamageType);
                float endMultiplier = GetReforgeMultiplier(player, DamageMultiplierScale.NormalizeName(item.Name));
                if(item.DamageType == DamageClass.Magic) {
                    if(item.mana > 28) 
                        return (int)Math.Round(endModifier.ApplyTo((damage*5)) * endMultiplier);
                    else 
                        return (int)Math.Round(endModifier.ApplyTo(damage) * endMultiplier);
                }
                return (int)Math.Round(endModifier.ApplyTo(damage) * endMultiplier);
            }

            int baseDamage = ContentSamples.ItemsByType.TryGetValue(item.type, out Item defaultItem) ? defaultItem.damage : 1;
            return baseDamage > 0 ? baseDamage : 1;
        }

        public static int CalculateDamageByName(Player player, string item)
        {
            Item weapon = new Item();
            var modPlayer = player.GetModPlayer<MyModPlayer>(); 
            Dictionary<string, int> weaponName = modPlayer.weaponName;

            if (weaponName.TryGetValue(item, out int id))
            {
                weapon.SetDefaults(id);
            }

            int attackSpeed = weapon.useTime;
            float damage;
            var bossList = BossDefeated.OrderedBosses;

            foreach (var boss in bossList)
            {
                if (!boss.IsDowned.Invoke())
                {
                    int primaryNpcId = boss.NpcIDs.First();
                    float bossHP = DamageMultiplierScale.GetBossScaleHP(primaryNpcId);
                    
                    if (attackSpeed < 10) damage = bossHP * 0.001f;
                    else if (attackSpeed >= 10 && attackSpeed < 20) damage = bossHP * 0.002f;
                    else if (attackSpeed >= 20 && attackSpeed < 30) damage = bossHP * 0.003f;
                    else if (attackSpeed >= 30) damage = bossHP * 0.004f;
                    else damage = 1;

                    StatModifier modifier = player.GetTotalDamage(weapon.DamageType);
                    float multiplier = GetReforgeMultiplier(player, item);
                    if(weapon.DamageType == DamageClass.Magic) {
                        if(weapon.mana > 28) 
                            return (int)Math.Round(modifier.ApplyTo((damage*5)) * multiplier);
                        else 
                            return (int)Math.Round(modifier.ApplyTo(damage) * multiplier);
                    }
                    return (int)Math.Round(modifier.ApplyTo(damage) * multiplier);
                }
            }

            if (bossList.Count > 0)
            {
                int finalBossId = bossList.Last().NpcIDs.First();
                float finalBossHp = DamageMultiplierScale.GetBossScaleHP(finalBossId);
                
                if (attackSpeed < 10) damage = finalBossHp * 0.05f;
                else if (attackSpeed >= 10 && attackSpeed < 20) damage = finalBossHp * 0.2f;
                else if (attackSpeed >= 20 && attackSpeed < 30) damage = finalBossHp * 0.3f;
                else if (attackSpeed >= 30) damage = finalBossHp * 0.5f;
                else damage = 1;

                StatModifier endModifier = player.GetTotalDamage(weapon.DamageType);
                float endMultiplier = GetReforgeMultiplier(player, item);
                if(weapon.DamageType == DamageClass.Magic) {
                    if(weapon.mana > 28) 
                        return (int)Math.Round(endModifier.ApplyTo((damage*5)) * endMultiplier);
                    else 
                        return (int)Math.Round(endModifier.ApplyTo(damage) * endMultiplier);
                }
                return (int)Math.Round(endModifier.ApplyTo(damage) * endMultiplier);
            }
            
            int baseDamage = ContentSamples.ItemsByType.TryGetValue(weapon.type, out Item defaultWeapon) ? defaultWeapon.damage : 1;
            return baseDamage > 0 ? baseDamage : 1;
        }
    }
}