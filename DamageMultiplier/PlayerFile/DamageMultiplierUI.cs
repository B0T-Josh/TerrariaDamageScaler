using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace DamageMultiplier.PlayerFile
{
    public class DamageMultiplierUI : UIState
    {
        private UIPanel panel;
        private UIList weaponList;
        private UIScrollbar scrollbar;

        public override void OnInitialize()
        {
            panel = new UIPanel();
            panel.Top.Set(100, 0f);
            panel.Left.Set(-200, 0.5f);
            panel.Width.Set(400, 0f);
            panel.Height.Set(450, 0f);
            panel.BackgroundColor = new Color(33, 43, 79) * 0.85f;

            UIText titleText = new UIText("Damage Scaler Weapons", 1.1f, true);
            titleText.HAlign = 0.5f;
            titleText.Top.Set(10, 0f);
            panel.Append(titleText);

            UIText hintText = new UIText("Manage your linked damage scaler weapons below:", 0.8f);
            hintText.HAlign = 0.5f;
            hintText.Top.Set(45, 0f);
            panel.Append(hintText);

            weaponList = new UIList();
            weaponList.Width.Set(330, 0f);
            weaponList.Height.Set(280, 0f);
            weaponList.Top.Set(80, 0f);
            weaponList.Left.Set(10, 0f);
            panel.Append(weaponList);

            scrollbar = new UIScrollbar();
            scrollbar.SetView(100f, 1000f);
            scrollbar.Height.Set(280, 0f);
            scrollbar.Top.Set(80, 0f);
            scrollbar.Left.Set(350, 0f);
            panel.Append(scrollbar);
            weaponList.SetScrollbar(scrollbar);

            UITextPanel<string> closeButton = new UITextPanel<string>("Close", 0.9f, true);
            closeButton.Width.Set(100, 0f);
            closeButton.Height.Set(35, 0f);
            closeButton.Left.Set(-50, 0.5f);
            closeButton.Top.Set(380, 0f);
            closeButton.OnLeftClick += (evt, element) => DamageMultiplierUISystem.ToggleUI();
            panel.Append(closeButton);

            Append(panel);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (panel.ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;
            }
        }

        public void RefreshWeaponList()
        {
            weaponList.Clear();
            var modPlayer = Main.LocalPlayer.GetModPlayer<MyModPlayer>();

            foreach (var weaponName in modPlayer.playerWeapons)
            {
                UIPanel itemPanel = new UIPanel();
                itemPanel.Width.Set(310, 0f);
                itemPanel.Height.Set(45, 0f);
                itemPanel.BackgroundColor = new Color(50, 60, 90);

                UIText nameText = new UIText(weaponName, 0.85f);
                nameText.VAlign = 0.5f;
                nameText.Left.Set(10, 0f);
                itemPanel.Append(nameText);

                UITextPanel<string> removeButton = new UITextPanel<string>("Remove", 0.7f);
                removeButton.Width.Set(70, 0f);
                removeButton.Height.Set(30, 0f);
                removeButton.VAlign = 0.5f;
                removeButton.Left.Set(220, 0f);

                string currentWeaponName = weaponName;
                removeButton.OnLeftClick += (evt, element) =>
                {
                    modPlayer.playerWeapons.Remove(currentWeaponName);
                    Dictionary<int, Item> allItems = ContentSamples.ItemsByType;
                    foreach (var items in allItems)
                    {
                        if (DamageMultiplierScale.NormalizeName(items.Value.Name) == currentWeaponName)
                        {
                            modPlayer.ItemWithDamage.Remove(items.Key);
                            modPlayer.weaponName.Remove(currentWeaponName);
                        }
                    }
                    RefreshWeaponList();
                };

                itemPanel.Append(removeButton);
                weaponList.Add(itemPanel);
            }
        }
    }
}