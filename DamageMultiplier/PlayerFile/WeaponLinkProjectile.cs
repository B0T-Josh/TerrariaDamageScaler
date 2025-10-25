using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System.Linq;
using System.Collections.Generic;
using Terraria.ID;
using System;

namespace DamageMultiplier.PlayerFile
{
    public class WeaponLinkedProjectile : GlobalProjectile
    {
        public string linkedWeaponName = null;
        private bool hasAppliedScaling = false;

        public override bool InstancePerEntity => true;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers || projectile.hostile)
                return;

            var mainPlayer = Main.player[projectile.owner];
            var player = mainPlayer.GetModPlayer<MyModPlayer>();
            var visited = new HashSet<int>();

            linkedWeaponName = GetWeaponNameFromSource(source, visited);

            Mod calamity = ModLoader.GetMod("CalamityMod");
            bool isCalamityLoaded = ModLoader.HasMod("CalamityMod") && calamity != null;

            if (!string.IsNullOrEmpty(linkedWeaponName) &&
                player.playerWeapons.Any(w => DamageMultiplierScale.NormalizeName(w) == linkedWeaponName))
            {
                int projectileDamage = MyGlobalItem.CalculateDamageByName(mainPlayer, linkedWeaponName, isCalamityLoaded);
                projectile.damage = projectileDamage;
                projectile.originalDamage = projectileDamage;
            }

            if (projectile.minion || projectile.sentry)
            {
                ApplyMinionScaling(mainPlayer, projectile, isCalamityLoaded);
                hasAppliedScaling = true;
            }
        }

        public override void AI(Projectile projectile)
        {
            if ((projectile.minion || projectile.sentry) && !hasAppliedScaling)
            {
                var mainPlayer = Main.player[projectile.owner];
                Mod calamity = ModLoader.GetMod("CalamityMod");
                bool isCalamityLoaded = ModLoader.HasMod("CalamityMod") && calamity != null;

                ApplyMinionScaling(mainPlayer, projectile, isCalamityLoaded);
                hasAppliedScaling = true;
            }
        }

        private void ApplyMinionScaling(Player player, Projectile projectile, bool isCalamityLoaded)
        {
            if (string.IsNullOrEmpty(linkedWeaponName))
            {
                var heldItem = player.HeldItem;
                if (heldItem != null && !string.IsNullOrEmpty(heldItem.Name))
                {
                    linkedWeaponName = DamageMultiplierScale.NormalizeName(heldItem.Name);
                }
            }

            if (string.IsNullOrEmpty(linkedWeaponName))
            {
                ModContent.GetInstance<DamageMultiplier>().Logger.Warn($"[DamageMultiplier] ⚠ Could not determine linked weapon for minion {projectile.Name}.");
                return;
            }

            try
            {
                int scaledDamage = MyGlobalItem.CalculateDamageByName(player, linkedWeaponName, isCalamityLoaded);
                projectile.damage = scaledDamage;
                projectile.originalDamage = scaledDamage;
            }
            catch (System.Exception ex)
            {
                Mod.Logger.Warn("Damage is not scaling");
            }
        }

        private string GetWeaponNameFromSource(IEntitySource source, HashSet<int> visited)
        {
            switch (source)
            {
                case EntitySource_ItemUse itemSource when itemSource.Item != null:
                    return DamageMultiplierScale.NormalizeName(itemSource.Item.Name);

                case EntitySource_Parent parentSource when parentSource.Entity is Projectile parentProj:
                    if (!visited.Add(parentProj.whoAmI))
                        return null;

                    var parentGlobal = parentProj.GetGlobalProjectile<WeaponLinkedProjectile>();
                    if (!string.IsNullOrEmpty(parentGlobal.linkedWeaponName))
                        return parentGlobal.linkedWeaponName;

                    return GetWeaponNameFromSource(parentProj.GetSource_FromThis(), visited);

                case EntitySource_Misc miscSource:
                    if (miscSource.Context == "Summon" || miscSource.Context == "MagicItem" || miscSource.Context == "PlayerAction")
                    {
                        if (Main.player.Any(p => p.active))
                        {
                            foreach (var player in Main.player)
                            {
                                if (!player.active)
                                    continue;

                                var held = player.HeldItem;
                                if (held != null && held.damage > 0)
                                    return DamageMultiplierScale.NormalizeName(held.Name);
                            }
                        }
                    }
                    break;
            }

            return null;
        }
    }
}
