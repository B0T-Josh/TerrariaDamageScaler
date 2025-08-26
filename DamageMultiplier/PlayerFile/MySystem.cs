using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria.ID;
using System.Collections.Generic;

namespace DamageMultiplier.PlayerFile
{
    public class MySystem : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "weapon";
        public override string Description => "Add, remove, or list weapons for damage scaling.";
        bool IsValidWeapon(string input)
        {
            Player player = Main.LocalPlayer;
            string normInput = DamageMultiplierScale.NormalizeName(input);

            foreach (Item item in player.inventory)
            {
                if (item != null && !item.IsAir && item.damage > 0) // damage > 0 → likely a weapon
                {
                    string normName = DamageMultiplierScale.NormalizeName(item.Name);
                    string normModName = DamageMultiplierScale.NormalizeName(item.ModItem?.Name ?? "");

                    if (normName == normInput || normModName == normInput)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var modPlayer = caller.Player.GetModPlayer<MyModPlayer>();

            if (args.Length == 0)
            {
                caller.Reply("Usage: /weapon <name> | /weapon remove <name> | /weapon list", Color.Red);
                return;
            }

            string subCommand = args[0].ToLower();

            if (subCommand == "list")
            {
                if (modPlayer.playerWeapons.Count == 0)
                    caller.Reply("No weapons stored.", Color.Yellow);
                else
                    caller.Reply("Stored weapons: " + string.Join(", ", modPlayer.playerWeapons), Color.Green);
                return;
            }

            if (subCommand == "remove")
            {
                if (args.Length < 2)
                {
                    caller.Reply("Specify a weapon to remove.", Color.Red);
                    return;
                }
                string weaponToRemove = string.Join(" ", args.Skip(1)).Trim().Replace(" ", "").ToLowerInvariant();
                if (modPlayer.playerWeapons.Remove(weaponToRemove))
                {
                    Mod Calamity = ModLoader.GetMod("CalamityMod");
                    bool isCalamityLoaded = ModLoader.HasMod("CalamityMod") && Calamity != null;
                    Dictionary<int, Item> allItems = ContentSamples.ItemsByType;
                    foreach (var weapons in modPlayer.playerWeapons)
                    {
                        foreach (var items in allItems)
                        {
                            if (DamageMultiplierScale.NormalizeName(items.Value.Name) == weapons)
                            {
                                modPlayer.ItemWithDamage.Remove(items.Key);
                            }
                        }
                    }
                    caller.Reply($"Removed weapon: {weaponToRemove}", Color.Green);
                }
                else
                    caller.Reply($"Weapon not found: {weaponToRemove}", Color.Yellow);
                return;
            }
            
            // Add weapon
            string weaponToAdd = string.Join(" ", args).Trim().ToLower().Replace(" ", "");

            if (IsValidWeapon(weaponToAdd))
            {
                if (!modPlayer.playerWeapons.Contains(weaponToAdd))
                {
                    var player = Main.LocalPlayer;
                    modPlayer.playerWeapons.Add(weaponToAdd);
                    caller.Reply($"Weapon added: {weaponToAdd}", Color.Green);
                    Mod Calamity = ModLoader.GetMod("CalamityMod");
                    bool isCalamityLoaded = ModLoader.HasMod("CalamityMod") && Calamity != null;
                    Dictionary<int, Item> allItems = ContentSamples.ItemsByType;
                    foreach (var weapons in modPlayer.playerWeapons)
                    {
                        foreach (var items in allItems)
                        {
                            if (DamageMultiplierScale.NormalizeName(items.Value.Name) == weapons)
                            {
                                Item weapon = new Item();
                                weapon.SetDefaults(items.Key);
                                int damage = MyGlobalItem.CalculateDamage(player, items.Value, isCalamityLoaded);
                                modPlayer.ItemWithDamage.Add(items.Key, damage);
                                weapon.damage = damage;
                            }
                        }
                    }
                }
                else
                {
                    caller.Reply($"Weapon already stored: {weaponToAdd}", Color.Yellow);
                }
            }
            else
            {
                caller.Reply($"Weapon invalid: {weaponToAdd}", Color.Red);
            }  
        }
    }
}
