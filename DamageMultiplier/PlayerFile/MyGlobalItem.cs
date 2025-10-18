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
        private static Mod Calamity = ModLoader.GetMod("CalamityMod");
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            var player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<MyModPlayer>();

            if (!item.IsAir &&
                modPlayer.playerWeapons.Any(w => DamageMultiplierScale.NormalizeName(w) ==
                                                 DamageMultiplierScale.NormalizeName(item.Name)))
            {
                bool isCalamityLoaded = ModLoader.HasMod("CalamityMod") && Calamity != null;
                int scaledDamage = CalculateDamage(player, item, isCalamityLoaded);

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

        // Calculate scaled damage based on boss HP and attack speed
        public static int CalculateDamage(Player player, Item item, bool isCalamityLoaded)
        {
            int attackSpeed = item.useTime;
            float damage;

            var bossList = BossDefeated.bossDefeated.Keys.ToList();
            Dictionary<int, bool> bossDefeatedList = BossDefeated.bossDefeated;

            foreach (var boss in bossDefeatedList)
            {
                if (!boss.Value)
                {
                    float bossHP = DamageMultiplierScale.GetBossScaleHP(boss.Key);
                    if (attackSpeed <= 15)
                    {
                        damage = bossHP * 0.001f * 0.5f;
                    }
                    else if (attackSpeed > 15 && attackSpeed < 20)
                    {
                        damage = bossHP * 0.001f;
                    }
                    else if (attackSpeed >= 20 && attackSpeed < 25)
                    {
                        damage = bossHP * 0.002f;
                    }
                    else if (attackSpeed >= 25)
                    {
                        damage = bossHP * 0.01f;
                    }
                    else
                    {
                        damage = 1;
                    }

                    // Apply player StatModifier here
                    StatModifier modifier = player.GetTotalDamage(item.DamageType);
                    return (int)Math.Round(modifier.ApplyTo(damage));
                }
            }

            // If all bosses are defeated, scale off the last boss
            var size = bossList.Count - 1;
            float finalBossHp = DamageMultiplierScale.GetBossScaleHP(bossDefeatedList.Last().Key);
            if (attackSpeed <= 15)
            {
                damage = finalBossHp * 0.1f * 0.5f;
            }
            else if (attackSpeed > 15 && attackSpeed < 20)
            {
                damage = finalBossHp * 0.1f;
            }
            else if (attackSpeed >= 20 && attackSpeed < 25)
            {
                damage = finalBossHp * 0.2f;
            }
            else if (attackSpeed >= 25)
            {
                damage = finalBossHp * 0.5f;
            }
            else
            {
                damage = 1;
            }

            // Apply player StatModifier here
            StatModifier endModifier = player.GetTotalDamage(item.DamageType);
            return (int)Math.Round(endModifier.ApplyTo(damage));
        }


        public static int CalculateDamageByName(Player player, string item, bool isCalamityLoaded)
        {
            Item weapon = new Item();
            var modPlayer = Main.LocalPlayer.GetModPlayer<MyModPlayer>();
            Dictionary<string, int> weaponName = modPlayer.weaponName;

            if (weaponName.TryGetValue(item, out int id))
            {
                weapon.SetDefaults(id);
            }

            int attackSpeed = weapon.useTime;
            float damage;

            var bossList = BossDefeated.bossDefeated.Keys.ToList();
            Dictionary<int, bool> bossDefeatedList = BossDefeated.bossDefeated;

            foreach (var boss in bossDefeatedList)
            {
                if (!boss.Value)
                {
                    float bossHP = DamageMultiplierScale.GetBossScaleHP(boss.Key);
                    if (attackSpeed <= 15)
                    {
                        damage = bossHP * 0.001f * 0.5f;
                    }
                    else if (attackSpeed > 15 && attackSpeed < 20)
                    {
                        damage = bossHP * 0.001f;
                    }
                    else if (attackSpeed >= 20 && attackSpeed < 25)
                    {
                        damage = bossHP * 0.002f;
                    }
                    else if (attackSpeed >= 25)
                    {
                        damage = bossHP * 0.01f;
                    }
                    else
                    {
                        damage = 1;
                    }

                    // Apply player StatModifier here
                    StatModifier modifier = player.GetTotalDamage(weapon.DamageType);
                    return (int)Math.Round(modifier.ApplyTo(damage));
                }
            }

            // If all bosses are defeated, scale off the last boss
            var size = bossList.Count - 1;
            float finalBossHp = DamageMultiplierScale.GetBossScaleHP(bossDefeatedList.Last().Key);
            if (attackSpeed <= 15)
            {
                damage = finalBossHp * 0.1f * 0.5f;
            }
            else if (attackSpeed > 15 && attackSpeed < 20)
            {
                damage = finalBossHp * 0.1f;
            }
            else if (attackSpeed >= 20 && attackSpeed < 25)
            {
                damage = finalBossHp * 0.2f;
            }
            else if (attackSpeed >= 25)
            {
                damage = finalBossHp * 0.5f;
            }
            else
            {
                damage = 1;
            }

            // Apply player StatModifier here
            StatModifier endModifier = player.GetTotalDamage(weapon.DamageType);
            return (int)Math.Round(endModifier.ApplyTo(damage));
        }
    }
}
