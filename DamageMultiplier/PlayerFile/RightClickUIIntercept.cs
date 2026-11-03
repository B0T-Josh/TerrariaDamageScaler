using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DamageMultiplier.PlayerFile
{
    public class RightClickUIIntercept : ModSystem
    {
        // NEW: This guarantees dictionaries are built AFTER Calamity items are loaded
        public override void PostSetupContent()
        {
            MyGlobalItem.ItemNameToIdMap.Clear();
            MyGlobalItem.ProjectileToItemMap.Clear();

            foreach (var pair in ContentSamples.ItemsByType)
            {
                if (!pair.Value.IsAir && !string.IsNullOrEmpty(pair.Value.Name))
                {
                    string norm = DamageMultiplierScale.NormalizeName(pair.Value.Name);
                    
                    if (!MyGlobalItem.ItemNameToIdMap.ContainsKey(norm))
                    {
                        MyGlobalItem.ItemNameToIdMap[norm] = pair.Key;
                    }
                    
                    if (pair.Value.shoot > 0 && !MyGlobalItem.ProjectileToItemMap.ContainsKey(pair.Value.shoot))
                    {
                        MyGlobalItem.ProjectileToItemMap[pair.Value.shoot] = norm;
                    }
                }
            }
        }

        public override void PostUpdateInput()
        {
            if (Main.netMode == Terraria.ID.NetmodeID.Server) return;

            if (Main.mouseRight && Main.mouseRightRelease && !Main.HoverItem.IsAir && Main.HoverItem.damage > 0)
            {
                string normName = DamageMultiplierScale.NormalizeName(Main.HoverItem.Name);
                
                IndividualWeaponUISystem.OpenUIForWeapon(normName);
                
                Main.mouseRightRelease = false; 
            }
        }
    }
}