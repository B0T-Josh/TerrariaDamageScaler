using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DamageMultiplier.PlayerFile
{
    public class MyModPlayer : ModPlayer
    {
        public Dictionary<int, int> ItemWithDamage = new Dictionary<int, int>();
        public List<string> playerWeapons = new List<string>();
        public Dictionary<int, Item> allItems;
        public Dictionary<string, int> weaponName = new Dictionary<string, int>();
        public Dictionary<string, float> reforgeMultipliers = new Dictionary<string, float>();
        
        public override void SaveData(Terraria.ModLoader.IO.TagCompound tag)
        {
            tag["weapons"] = playerWeapons;
        }

        public override void LoadData(Terraria.ModLoader.IO.TagCompound tag)
        {
            playerWeapons = tag.GetList<string>("weapons")?.ToList() ?? new List<string>();
        }

        public override void OnEnterWorld()
        {
            // FIX: Don't execute local UI / text routines if running on a dedicated server
            if (Main.netMode == NetmodeID.Server)
                return;

            var player = Main.LocalPlayer;
            allItems = ContentSamples.ItemsByType; 
            
            if (playerWeapons.Count > 0)
            {
                foreach (var weapons in playerWeapons)
                {
                    foreach (var items in allItems)
                    {
                        if (DamageMultiplierScale.NormalizeName(items.Value.Name) == weapons)
                        {
                            int damage = MyGlobalItem.CalculateDamage(player, items.Value);
                            ItemWithDamage[items.Key] = damage;
                            weaponName[DamageMultiplierScale.NormalizeName(items.Value.Name)] = items.Key;
                        }
                    }
                }
            }
            else
            {
                Main.NewText("No weapon was stored");
            }
        }
    }
}