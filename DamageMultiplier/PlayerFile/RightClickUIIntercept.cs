using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DamageMultiplier.PlayerFile
{
    public class RightClickUIIntercept : ModSystem
    {
        public override void PostSetupContent()
        {
            MyGlobalItem.ItemNameToIdMap.Clear();
            MyGlobalItem.ProjectileToItemTypeMap.Clear();
            MyGlobalItem.BuffToItemTypeMap.Clear(); // NEW: Tracks Buffs to Weapons

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

                    // Cache which weapon gives which buff (Crucial for Calamity minions)
                    if (item.buffType > 0 && !MyGlobalItem.BuffToItemTypeMap.ContainsKey(item.buffType))
                        MyGlobalItem.BuffToItemTypeMap[item.buffType] = pair.Key;
                }
            }
        }

        public override void PostUpdateInput()
        {
            if (Main.netMode == Terraria.ID.NetmodeID.Server) return;

            if (Main.mouseRight && Main.mouseRightRelease && !Main.HoverItem.IsAir && Main.HoverItem.damage > 0)
            {
                string normName = DamageMultiplierScale.NormalizeName(ContentSamples.ItemsByType[Main.HoverItem.type].Name);
                IndividualWeaponUISystem.OpenUIForWeapon(normName);
                Main.mouseRightRelease = false; 
            }
        }
    }
}