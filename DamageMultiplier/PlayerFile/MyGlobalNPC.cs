using System.Collections.Generic;
using MonoMod.Core.Utils;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DamageMultiplier.PlayerFile
{
    public class MyGlobalNPC : GlobalNPC
    {

        public override void OnKill(NPC npc)
        {
            if(npc.boss)
            {
                BossDefeated.bossDefeated[npc.type] = true;
                var player = Main.LocalPlayer;
                var modPlayer = Main.LocalPlayer.GetModPlayer<MyModPlayer>();
                bool isCalamityLoaded = ModLoader.TryGetMod("CalamityMod", out _);
                Dictionary<int, Item> allItems = ContentSamples.ItemsByType;
                foreach (var weaponItem in modPlayer.playerWeapons)
                {
                    foreach (var item in allItems)
                    {
                        if (DamageMultiplierScale.NormalizeName(item.Value.Name) == weaponItem)
                        {
                            Item weapon = new Item();
                            weapon.SetDefaults(item.Key);
                            int damage = MyGlobalItem.CalculateDamage(player, item.Value, isCalamityLoaded);
                            weapon.damage = damage;
                            modPlayer.ItemWithDamage[item.Key] = damage;
                        }
                    }
                }
            }
        }
    }
}