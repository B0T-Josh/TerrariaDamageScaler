using Terraria;
using Terraria.ModLoader;

namespace DamageMultiplier.PlayerFile
{
    public class RightClickUIIntercept : ModSystem
    {
        public override void PostUpdateInput()
        {
            if (Main.netMode == Terraria.ID.NetmodeID.Server) return;

            // Added Main.HoverItem.damage > 0 to ensure it is a weapon
            if (Main.mouseRight && Main.mouseRightRelease && !Main.HoverItem.IsAir && Main.HoverItem.damage > 0)
            {
                string normName = DamageMultiplierScale.NormalizeName(Main.HoverItem.Name);
                
                IndividualWeaponUISystem.OpenUIForWeapon(normName);
                
                Main.mouseRightRelease = false; 
            }
        }
    }
}