using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
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