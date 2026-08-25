using System.Collections.Generic;
using Terraria;
using Terraria.ID; // <--- This was missing!
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

                // Loop through all active connected players instead of Main.LocalPlayer
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
                                    int damage = MyGlobalItem.CalculateDamage(player, item.Value);
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