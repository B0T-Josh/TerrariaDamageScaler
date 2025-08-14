using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DamageMultiplier.PlayerFile
{
    public class MyGlobalItemInInventory : GlobalItem
    {
        private static Mod Calamity = ModLoader.GetMod("CalamityMod");

        // Apply exact calculated damage per hit
        public override void UpdateInventory(Item item, Player player)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<MyModPlayer>();
            bool isCalamityLoaded = ModLoader.HasMod("CalamityMod") && Calamity != null;
            if (modPlayer.playerWeapons.Contains(DamageMultiplierScale.NormalizeName(player.HeldItem.Name)))
            {
                Item heldItem = player.HeldItem;
                int damage = CalculateDamage(heldItem, isCalamityLoaded);
                modPlayer.itemDamage = damage;
                heldItem.damage = damage;
            }
        }
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            var player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<MyModPlayer>();

            if (!item.IsAir &&
                modPlayer.playerWeapons.Any(w => DamageMultiplierScale.NormalizeName(w) ==
                                                 DamageMultiplierScale.NormalizeName(item.Name)))
            {
                bool isCalamityLoaded = ModLoader.HasMod("CalamityMod") && Calamity != null;
                int scaledDamage = CalculateDamage(item, isCalamityLoaded);

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
        private int CalculateDamage(Item item, bool isCalamityLoaded)
        {
            int attackSpeed = item.useTime;
            float damage = 1;

            var bossList = isCalamityLoaded ? BossDefeated.CalamityBosses : BossDefeated.vanillaBosses;
            Dictionary<int, bool> bossDefeatedList = BossDefeated.bossDefeated;

            foreach (var boss in bossDefeatedList)
            {
                if (!boss.Value)
                {
                    float bossHP = DamageMultiplierScale.GetBossScaleHP(boss.Key);
                    if (attackSpeed < 20)
                        damage = bossHP * 0.002f * 0.5f;
                    else if (attackSpeed > 19 && attackSpeed < 25)
                        damage = bossHP * 0.002f;
                    else
                        damage = bossHP * 0.004f;
                    break;
                }
            }
            return (int)damage;
        }
    }
}
