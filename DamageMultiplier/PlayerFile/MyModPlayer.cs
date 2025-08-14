using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace DamageMultiplier.PlayerFile
{
    public class MyModPlayer : ModPlayer
    {
        public int itemDamage = 1;
        public void setItemDamage(int damage)
        {
            itemDamage = damage;
        }

        public Item heldItem = null;
        public void setHeldItem(Item item)
        {
            heldItem = item;
        }
        public List<string> playerWeapons = new List<string>();

        public override void SaveData(Terraria.ModLoader.IO.TagCompound tag)
        {
            tag["weapons"] = playerWeapons;
        }

        public override void LoadData(Terraria.ModLoader.IO.TagCompound tag)
        {
            playerWeapons = tag.GetList<string>("weapons")?.ToList() ?? new List<string>();
        }
    }
}
