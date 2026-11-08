using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace DamageMultiplier.PlayerFile
{
    public class IndividualWeaponUISystem : ModSystem
    {
        private UserInterface weaponUIInterface;
        internal static IndividualWeaponUIState WeaponUI;
        internal static bool Visible;

        public override void Load()
        {
            if (Main.netMode == Terraria.ID.NetmodeID.Server)
                return;

            WeaponUI = new IndividualWeaponUIState();
            WeaponUI.Activate();
            weaponUIInterface = new UserInterface();
            weaponUIInterface.SetState(WeaponUI);
            Visible = false;
        }

        public static void OpenUIForWeapon(string weaponName)
        {
            Visible = true;
            WeaponUI.SetWeapon(weaponName);
        }

        public static void CloseUI()
        {
            Visible = false;
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (Visible && weaponUIInterface != null)
            {
                weaponUIInterface.Update(gameTime);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "DamageMultiplier: Individual Weapon Scaler UI",
                    delegate
                    {
                        // FIX: Check for the right-click during the Draw phase.
                        // This guarantees Main.HoverItem is populated regardless of PC hardware speed.
                        if (Main.mouseRight && Main.mouseRightRelease && !Main.HoverItem.IsAir && Main.HoverItem.damage > 0)
                        {
                            string normName = DamageMultiplierScale.NormalizeName(ContentSamples.ItemsByType[Main.HoverItem.type].Name);
                            OpenUIForWeapon(normName);
                            Main.mouseRightRelease = false; 
                        }

                        if (Visible && weaponUIInterface != null)
                        {
                            weaponUIInterface.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }
    }
}