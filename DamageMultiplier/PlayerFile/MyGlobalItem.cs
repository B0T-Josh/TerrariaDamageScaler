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

        // This applies actual damage for gameplay
        public override void UpdateInventory(Item item, Player player)
        {
            var modPlayer = player.GetModPlayer<MyModPlayer>();
            List<string> input = modPlayer.playerWeapons;

            if (!item.IsAir && input.Any(w => DamageMultiplierScale.NormalizeName(w) == DamageMultiplierScale.NormalizeName(item.Name)))
            {
                bool isCalamityLoaded = ModLoader.HasMod("CalamityMod") && Calamity != null;
                float summonDamage = 0f;
                int damage = CalculateDamage(item, isCalamityLoaded);

                summonDamage = damage * 0.01f;

                if (item.DamageType != DamageClass.Summon)
                    item.damage = damage;

                if (summonDamage > 0)
                    player.GetDamage(DamageClass.Summon) *= 1f + summonDamage;

                modPlayer.setHeldItem(item);
                modPlayer.setItemDamage(damage);
            }
        }

        // This changes what shows in the tooltip
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            var player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<MyModPlayer>();

            if (!item.IsAir && modPlayer.playerWeapons.Any(w => DamageMultiplierScale.NormalizeName(w) == DamageMultiplierScale.NormalizeName(item.Name)))
            {
                bool isCalamityLoaded = ModLoader.HasMod("CalamityMod") && Calamity != null;
                int damage = CalculateDamage(item, isCalamityLoaded);

                foreach (TooltipLine line in tooltips)
                {
                    if (line.Mod == "Terraria" && line.Name == "Damage")
                    {
                        // Preserve the original damage type wording
                        string damageType = item.DamageType.DisplayName.ToString();
                        line.Text = $"{damage} {damageType} damage";
                        break;
                    }
                }
            }
        }

        private int CalculateDamage(Item item, bool isCalamityLoaded)
        {
            int damage = 1;
            int attack_speed = item.useTime;

            if (attack_speed < 20)
                damage = (int)((DamageMultiplierScale.GetBossScaleHP(NPCID.KingSlime) * 0.002f) * 0.5f);
            else if(attack_speed > 19 && attack_speed < 25)
                damage = (int)(DamageMultiplierScale.GetBossScaleHP(NPCID.KingSlime) * 0.002f);
            else
                damage = (int)(DamageMultiplierScale.GetBossScaleHP(NPCID.KingSlime) * 0.004f);

            var bossList = isCalamityLoaded ? BossDefeated.CalamityBosses : BossDefeated.vanillaBosses;

            foreach (var boss in bossList)
            {
                if (BossDefeated.bossDefeated.TryGetValue(boss, out bool defeated) && defeated)
                {
                    if (attack_speed > 19 && attack_speed < 25)
                        damage = (int)(DamageMultiplierScale.GetBossScaleHP(boss) * 0.002);
                    else if (attack_speed < 20)
                        damage = (int)((DamageMultiplierScale.GetBossScaleHP(boss) * 0.002) * 0.5f);
                    else
                        damage = (int)(DamageMultiplierScale.GetBossScaleHP(boss) * 0.004f);
                    break;
                }
            }
            return damage;
        }
    }
}
