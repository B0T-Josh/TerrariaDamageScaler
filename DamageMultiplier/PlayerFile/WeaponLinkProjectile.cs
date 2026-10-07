using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System.Linq;
using System.Collections.Generic;

namespace DamageMultiplier.PlayerFile
{
    public class WeaponLinkedProjectile : GlobalProjectile
    {
        public string linkedWeaponName = null;

        public override bool InstancePerEntity => true;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers || projectile.hostile)
                return;

            if (projectile.minion || projectile.sentry || projectile.DamageType == DamageClass.Summon || projectile.DamageType.CountsAsClass(DamageClass.Summon))
            {
                var mainPlayer = Main.player[projectile.owner];
                var visited = new HashSet<int>();

                linkedWeaponName = GetWeaponNameFromSource(source, visited);

                // If a Buff spawned the minion, trace its Projectile ID directly to the Weapon
                if (string.IsNullOrEmpty(linkedWeaponName))
                {
                    if (MyGlobalItem.ProjectileToItemMap.TryGetValue(projectile.type, out string mappedName))
                    {
                        linkedWeaponName = mappedName;
                    }
                }

                if (string.IsNullOrEmpty(linkedWeaponName) && mainPlayer.HeldItem != null && mainPlayer.HeldItem.damage > 0)
                {
                    linkedWeaponName = DamageMultiplierScale.NormalizeName(mainPlayer.HeldItem.Name);
                }
            }
        }

        // CRITICAL FIX: Changed from AI() to PreAI() and removed the one-time stop switch.
        // This forces the scaled damage to apply every frame BEFORE Terraria calculates armor stats.
        public override bool PreAI(Projectile projectile)
        {
            if (projectile.minion || projectile.sentry || projectile.DamageType == DamageClass.Summon || projectile.DamageType.CountsAsClass(DamageClass.Summon))
            {
                if (projectile.owner >= 0 && projectile.owner < Main.maxPlayers)
                {
                    var mainPlayer = Main.player[projectile.owner];
                    ApplyDamageOverride(mainPlayer, projectile);
                }
            }
            
            return true; // Return true to let vanilla AI continue running
        }

        private void ApplyDamageOverride(Player player, Projectile projectile)
        {
            // Safety net in case OnSpawn missed the weapon name
            if (string.IsNullOrEmpty(linkedWeaponName))
            {
                if (MyGlobalItem.ProjectileToItemMap.TryGetValue(projectile.type, out string mappedName))
                {
                    linkedWeaponName = mappedName;
                }
                else if (player.HeldItem != null && !string.IsNullOrEmpty(player.HeldItem.Name))
                {
                    linkedWeaponName = DamageMultiplierScale.NormalizeName(player.HeldItem.Name);
                }
            }

            if (string.IsNullOrEmpty(linkedWeaponName))
                return;

            var modPlayer = player.GetModPlayer<MyModPlayer>();
            if (modPlayer.playerWeapons.Contains(linkedWeaponName))
            {
                try
                {
                    int scaledDamage = MyGlobalItem.CalculateMinionBaseDamageByName(player, linkedWeaponName);
                    
                    if (scaledDamage > 1)
                    {
                        // We ONLY set originalDamage. Terraria's vanilla AI will automatically 
                        // read this and dynamically calculate projectile.damage using your armor stats!
                        projectile.originalDamage = scaledDamage;
                    }
                }
                catch (System.Exception ex)
                {
                    Mod.Logger.Warn($"Damage is not scaling: {ex.Message}");
                }
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
                    if (miscSource.Context == "Summon" || miscSource.Context == "MagicItem" || miscSource.Context == "PlayerAction" || miscSource.Context == "Minion")
                    {
                        if (Main.player.Any(p => p.active))
                        {
                            foreach (var p in Main.player)
                            {
                                if (!p.active)
                                    continue;

                                var held = p.HeldItem;
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