using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;

namespace DamageMultiplier.PlayerFile
{
    public class RightClickUIIntercept : ModSystem
    {
        // NEW: This cache holds onto the item to bridge the gap between Draw and Update phases
        private static Item lastHoveredItem = new Item();

        public override void PostSetupContent()
        {
            MyGlobalItem.ItemNameToIdMap.Clear();
            MyGlobalItem.ProjectileToItemTypeMap.Clear();
            MyGlobalItem.BuffToItemTypeMap.Clear(); 

            foreach (var pair in ContentSamples.ItemsByType)
            {
                Item item = pair.Value;
                if (!item.IsAir)
                {
                    string norm = DamageMultiplierScale.NormalizeName(item.Name);
                    
                    if (!MyGlobalItem.ItemNameToIdMap.ContainsKey(norm))
                        MyGlobalItem.ItemNameToIdMap[norm] = pair.Key;
                    
                    if (item.shoot > 0 && !MyGlobalItem.ProjectileToItemTypeMap.ContainsKey(item.shoot))
                        MyGlobalItem.ProjectileToItemTypeMap[item.shoot] = pair.Key;

                    if (item.buffType > 0 && !MyGlobalItem.BuffToItemTypeMap.ContainsKey(item.buffType))
                        MyGlobalItem.BuffToItemTypeMap[item.buffType] = pair.Key;
                }
            }
        }

        // STEP 1: Cache the item during the Draw phase when the inventory populates it
        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            if (Main.HoverItem != null && !Main.HoverItem.IsAir)
            {
                // Clone it so the game can't wipe our reference
                lastHoveredItem = Main.HoverItem.Clone();
            }
            else
            {
                lastHoveredItem = new Item();
            }
        }

        // STEP 2: Read the click during the Input phase using our cached item
        public override void PostUpdateInput()
        {
            if (Main.netMode == Terraria.ID.NetmodeID.Server) return;

            if (Main.mouseRight && Main.mouseRightRelease && lastHoveredItem != null && !lastHoveredItem.IsAir && lastHoveredItem.damage > 0)
            {
                string normName = DamageMultiplierScale.NormalizeName(ContentSamples.ItemsByType[lastHoveredItem.type].Name);
                IndividualWeaponUISystem.OpenUIForWeapon(normName);
                
                // Consume the click so it doesn't leak into the game world or inventory
                Main.mouseRightRelease = false; 
            }
        }
    }
}