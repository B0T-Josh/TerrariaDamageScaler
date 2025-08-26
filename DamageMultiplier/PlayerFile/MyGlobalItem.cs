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

        // Apply exact calculated damage per hit
        // public override void UpdateInventory(Item item, Player player)
        // {
        //     var modPlayer = Main.LocalPlayer.GetModPlayer<MyModPlayer>();
        //     bool isCalamityLoaded = ModLoader.HasMod("CalamityMod") && Calamity != null;
        //     if (modPlayer.playerWeapons.Contains(DamageMultiplierScale.NormalizeName(player.HeldItem.Name)))
        //     {
        //         Item heldItem = player.HeldItem;
        //         int damage = CalculateDamage(heldItem, isCalamityLoaded);
        //         modPlayer.itemDamage = damage;
        //         heldItem.damage = damage;
        //     }
        // }
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

            // Default scaling (before modifiers)
            if (attackSpeed < 20)
                damage = DamageMultiplierScale.GetBossScaleHP(NPCID.KingSlime) * 0.001f * 0.5f;
            else if (attackSpeed > 19 && attackSpeed < 25)
                damage = DamageMultiplierScale.GetBossScaleHP(NPCID.KingSlime) * 0.001f;
            else
                damage = DamageMultiplierScale.GetBossScaleHP(NPCID.KingSlime) * 0.002f;

            var bossList = isCalamityLoaded ? BossDefeated.CalamityBosses : BossDefeated.vanillaBosses;
            Dictionary<int, bool> bossDefeatedList = BossDefeated.bossDefeated;

            foreach (var boss in bossDefeatedList)
            {
                if (!boss.Value)
                {
                    float bossHP = DamageMultiplierScale.GetBossScaleHP(boss.Key);
                    if (attackSpeed < 20)
                        damage = bossHP * 0.001f * 0.5f;
                    else if (attackSpeed > 19 && attackSpeed < 25)
                        damage = bossHP * 0.001f;
                    else
                        damage = bossHP * 0.002f;

                    // Apply player StatModifier here
                    StatModifier modifier = player.GetTotalDamage(item.DamageType);
                    return (int)Math.Round(modifier.ApplyTo(damage));
                }
            }

            // If all bosses are defeated, scale off the last boss
            var size = bossList.Count - 1;
            float finalBossHp = DamageMultiplierScale.GetBossScaleHP(bossList[size]);
            if (attackSpeed < 20)
                damage = finalBossHp * 0.01f * 0.5f;
            else if (attackSpeed > 19 && attackSpeed < 25)
                damage = finalBossHp * 0.01f;
            else
                damage = finalBossHp * 0.02f;

            // Apply player StatModifier here
            StatModifier endModifier = player.GetTotalDamage(item.DamageType);
            return (int)Math.Round(endModifier.ApplyTo(damage));
        }


        public static int CalculateDamageByName(Player player, string item, bool isCalamityLoaded)
        {
            Item weapon = new Item();
            Dictionary<int, Item> allItems = ContentSamples.ItemsByType;
            var modPayer = Main.LocalPlayer.GetModPlayer<MyModPlayer>();
            foreach (var items in allItems)
            {
                if (DamageMultiplierScale.NormalizeName(items.Value.Name) == item)
                {
                    weapon.SetDefaults(items.Key);
                }
            }

            int attackSpeed = weapon.useTime;
            float damage;

            if (attackSpeed < 20)
                damage = DamageMultiplierScale.GetBossScaleHP(NPCID.KingSlime) * 0.001f * 0.5f;
            else if (attackSpeed > 19 && attackSpeed < 25)
                damage = DamageMultiplierScale.GetBossScaleHP(NPCID.KingSlime) * 0.001f;
            else
                damage = DamageMultiplierScale.GetBossScaleHP(NPCID.KingSlime) * 0.002f;

            var bossList = isCalamityLoaded ? BossDefeated.CalamityBosses : BossDefeated.vanillaBosses;
            Dictionary<int, bool> bossDefeatedList = BossDefeated.bossDefeated;

            foreach (var boss in bossDefeatedList)
            {
                if (!boss.Value)
                {
                    float bossHP = DamageMultiplierScale.GetBossScaleHP(boss.Key);
                    if (attackSpeed < 20)
                        damage = bossHP * 0.001f * 0.5f;
                    else if (attackSpeed > 19 && attackSpeed < 25)
                        damage = bossHP * 0.001f;
                    else
                        damage = bossHP * 0.002f;

                    // Apply player StatModifier here
                    StatModifier modifier = player.GetTotalDamage(weapon.DamageType);
                    return (int)Math.Round(modifier.ApplyTo(damage));
                }
            }

            // If all bosses are defeated, scale off the last boss
            var size = bossList.Count - 1;
            float finalBossHp = DamageMultiplierScale.GetBossScaleHP(bossList[size]);
            if (attackSpeed < 20)
                damage = finalBossHp * 0.01f * 0.5f;
            else if (attackSpeed > 19 && attackSpeed < 25)
                damage = finalBossHp * 0.01f;
            else
                damage = finalBossHp * 0.02f;

            // Apply player StatModifier here
            StatModifier endModifier = player.GetTotalDamage(weapon.DamageType);
            return (int)Math.Round(endModifier.ApplyTo(damage));
        }
    }
}
