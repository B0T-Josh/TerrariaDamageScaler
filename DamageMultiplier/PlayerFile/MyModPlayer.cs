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
            Mod Calamity = ModLoader.GetMod("CalamityMod");
            bool isCalamityLoaded = ModLoader.HasMod("CalamityMod") && Calamity != null;
            Dictionary<int, Item> allItems = ContentSamples.ItemsByType;
            if (playerWeapons.Count > 0)
            {
                foreach (var weapons in playerWeapons)
                {
                    foreach (var items in allItems)
                    {
                        if (DamageMultiplierScale.NormalizeName(items.Value.Name) == weapons)
                        {
                            int damage = MyGlobalItem.CalculateDamage(items.Value, isCalamityLoaded);
                            ItemWithDamage.Add(items.Key, damage);
                        }
                    }
                }
                Item weapon = new Item();
                foreach (var itemId in ItemWithDamage)
                {
                    weapon.SetDefaults(itemId.Key);
                    weapon.damage = itemId.Value;
                }
            }
            else
            {
                Main.NewText("No weapon was stored");
            }
        }
    }
}
