using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace DamageMultiplier.PlayerFile
{
    public class DamageMultiplierUISystem : ModSystem
    {
        private UserInterface scalerUIInterface;
        public static DamageMultiplierUI ScalerUI;
        internal static bool Visible;

        public override void Load()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            ScalerUI = new DamageMultiplierUI();
            ScalerUI.Activate();
            scalerUIInterface = new UserInterface();
            scalerUIInterface.SetState(ScalerUI);
            Visible = false;
        }

        public static void ToggleUI()
        {
            Visible = !Visible;
            if (Visible && ScalerUI != null)
            {
                ScalerUI.RefreshWeaponList();
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (Visible && scalerUIInterface != null)
            {
                scalerUIInterface.Update(gameTime);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "DamageMultiplier: Scaler UI",
                    delegate
                    {
                        if (Visible && scalerUIInterface != null)
                        {
                            scalerUIInterface.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }
    }

    public class ScalerCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "scaler";
        public override string Description => "Open the Damage Scaler UI management panel.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            DamageMultiplierUISystem.ToggleUI();
            caller.Reply("Toggled Damage Scaler Interface.", Color.Green);
        }
    }
}