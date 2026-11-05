using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace DamageMultiplier.PlayerFile
{
    public class IndividualWeaponUIState : UIState
    {
        private UIPanel mainPanel;
        private UIText titleText;
        private UITextPanel<string> toggleButton;
        private UIText scaleDisplay;
        private UITextPanel<string> minusButton;
        private UITextPanel<string> plusButton;
        private UITextPanel<string> closeButton;

        private UIText bossReferenceText;

        private string currentWeapon;
        private MyModPlayer modPlayer;

        public override void OnInitialize()
        {
            mainPanel = new UIPanel();
            mainPanel.Left.Set(-150, 0.5f);
            mainPanel.Top.Set(-100, 0.5f);
            mainPanel.Width.Set(300, 0f);
            mainPanel.Height.Set(240, 0f);
            mainPanel.BackgroundColor = new Color(33, 43, 79) * 0.8f;
            Append(mainPanel);

            titleText = new UIText("Weapon Settings", 0.5f, true);
            titleText.Top.Set(10, 0f);
            titleText.HAlign = 0.5f;
            mainPanel.Append(titleText);

            toggleButton = new UITextPanel<string>("Scaling: OFF");
            toggleButton.Top.Set(50, 0f);
            toggleButton.Width.Set(150, 0f);
            toggleButton.Height.Set(30, 0f);
            toggleButton.HAlign = 0.5f;
            toggleButton.OnLeftClick += ToggleScaling;
            mainPanel.Append(toggleButton);

            minusButton = new UITextPanel<string>("-");
            minusButton.Top.Set(100, 0f);
            minusButton.Left.Set(40, 0f);
            minusButton.Width.Set(40, 0f);
            minusButton.Height.Set(30, 0f);
            minusButton.OnLeftClick += DecreaseScale;
            mainPanel.Append(minusButton);

            scaleDisplay = new UIText("Scale: 1");
            scaleDisplay.Top.Set(105, 0f);
            scaleDisplay.HAlign = 0.5f;
            mainPanel.Append(scaleDisplay);

            plusButton = new UITextPanel<string>("+");
            plusButton.Top.Set(100, 0f);
            plusButton.Left.Set(190, 0f);
            plusButton.Width.Set(40, 0f);
            plusButton.Height.Set(30, 0f);
            plusButton.OnLeftClick += IncreaseScale;
            mainPanel.Append(plusButton);

            bossReferenceText = new UIText("HP Scale Boss: Unknown", 0.8f);
            bossReferenceText.Top.Set(145, 0f);
            bossReferenceText.HAlign = 0.5f;
            mainPanel.Append(bossReferenceText);

            closeButton = new UITextPanel<string>("Close");
            closeButton.Top.Set(175, 0f); 
            closeButton.Width.Set(100, 0f);
            closeButton.Height.Set(30, 0f);
            closeButton.HAlign = 0.5f;
            closeButton.OnLeftClick += (evt, element) => IndividualWeaponUISystem.CloseUI();
            mainPanel.Append(closeButton);
        }

        // NEW: This intercepts the mouse so you don't swing your weapon while clicking the UI
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (mainPanel.IsMouseHovering)
            {
                Main.LocalPlayer.mouseInterface = true;
            }
        }

        public void SetWeapon(string weaponName)
        {
            currentWeapon = weaponName;
            modPlayer = Main.LocalPlayer.GetModPlayer<MyModPlayer>();
            
            if (!modPlayer.WeaponConfigs.ContainsKey(currentWeapon))
            {
                modPlayer.WeaponConfigs[currentWeapon] = new WeaponScaleData(false, 1);
            }

            titleText.SetText($"Settings: {currentWeapon}");
            RefreshUI();
        }

        private void RefreshUI()
        {
            var config = modPlayer.WeaponConfigs[currentWeapon];
            
            if (config.IsEnabled)
            {
                toggleButton.SetText("Scaling: ON");
                toggleButton.BackgroundColor = Color.Green * 0.7f;
            }
            else
            {
                toggleButton.SetText("Scaling: OFF");
                toggleButton.BackgroundColor = Color.Red * 0.7f;
            }

            scaleDisplay.SetText($"Scale: {config.DamageScale}");
            bossReferenceText.SetText($"HP Scale Boss: {MyGlobalItem.GetCurrentHPReferenceBoss()}");
        }

        private void ToggleScaling(UIMouseEvent evt, UIElement listeningElement)
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
            modPlayer.WeaponConfigs[currentWeapon].IsEnabled = !modPlayer.WeaponConfigs[currentWeapon].IsEnabled;
            RefreshUI();
        }

        private void DecreaseScale(UIMouseEvent evt, UIElement listeningElement)
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
            if (modPlayer.WeaponConfigs[currentWeapon].DamageScale > 1)
            {
                modPlayer.WeaponConfigs[currentWeapon].DamageScale--;
                RefreshUI();
            }
        }

        private void IncreaseScale(UIMouseEvent evt, UIElement listeningElement)
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
            modPlayer.WeaponConfigs[currentWeapon].DamageScale++;
            RefreshUI();
        }
    }
}