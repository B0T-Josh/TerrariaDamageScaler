using System.Collections.Generic;
using Terraria;
using Terraria.ID; 
using Terraria.ModLoader;

namespace DamageMultiplier.PlayerFile
{
    public class MyGlobalNPC : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            if (npc.boss)
            {
                Dictionary<int, Item> allItems = ContentSamples.ItemsByType;

                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (player != null && player.active)
                    {
                        var modPlayer = player.GetModPlayer<MyModPlayer>();

                        foreach (var weaponItem in modPlayer.playerWeapons)
                        {
                            foreach (var item in allItems)
                            {
                                if (DamageMultiplierScale.NormalizeName(item.Value.Name) == weaponItem)
                                {
                                    // NEW NAMING CONVENTION HERE
                                    int damage = MyGlobalItem.CalculateTotalDamage(player, item.Value);
                                    modPlayer.ItemWithDamage[item.Key] = damage;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}