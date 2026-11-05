using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System.Linq;
using System.Collections.Generic;
using Terraria.ID;

namespace DamageMultiplier.PlayerFile
{
    public class WeaponLinkedProjectile : GlobalProjectile
    {
        public int linkedItemType = 0;
        public string linkedWeaponName = null;

        public override bool InstancePerEntity => true;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            // Ignore enemy projectiles
            if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers || projectile.hostile)
                return;

            var mainPlayer = Main.player[projectile.owner];
            var visited = new HashSet<int>();

            DetermineSource(source, visited);

            if (linkedItemType == 0)
            {
                if (MyGlobalItem.ProjectileToItemTypeMap.TryGetValue(projectile.type, out int mappedType))
                {
                    linkedItemType = mappedType;
                }
                else if (mainPlayer.HeldItem != null && mainPlayer.HeldItem.damage > 0)
                {
                    linkedItemType = mainPlayer.HeldItem.type;
                }
            }

            if (linkedItemType > 0)
            {
                linkedWeaponName = DamageMultiplierScale.NormalizeName(ContentSamples.ItemsByType[linkedItemType].Name);
                MyGlobalItem.ProjectileToItemTypeMap[projectile.type] = linkedItemType;
            }
        }

        private void DetermineSource(IEntitySource source, HashSet<int> visited)
        {
            if (source is EntitySource_ItemUse itemSource)
                linkedItemType = itemSource.Item.type;
            else if (source is EntitySource_ItemUse_WithAmmo ammoSource)
                linkedItemType = ammoSource.Item.type;
            else if (source is EntitySource_Buff buffSource)
            {
                if (MyGlobalItem.BuffToItemTypeMap.TryGetValue(buffSource.BuffId, out int buffItem))
                    linkedItemType = buffItem;
            }
            else if (source is EntitySource_Parent parentSource && parentSource.Entity is Projectile parentProj)
            {
                if (visited.Add(parentProj.whoAmI))
                {
                    var parentGlobal = parentProj.GetGlobalProjectile<WeaponLinkedProjectile>();
                    linkedItemType = parentGlobal.linkedItemType;
                }
            }
        }

        // Enforce damage BEFORE the projectile runs its logic
        public override bool PreAI(Projectile projectile)
        {
            // CRITICAL FIX: Removed the Summoner restriction. Now intercepts ALL player projectiles.
            if (projectile.owner >= 0 && projectile.owner < Main.maxPlayers && !projectile.hostile)
            {
                var mainPlayer = Main.player[projectile.owner];
                ApplyDamageOverride(mainPlayer, projectile);
            }
            
            return true; 
        }

        // NEW: Enforce damage AFTER Calamity runs its custom AI to undo hardcoded multipliers
        public override void PostAI(Projectile projectile)
        {
            if (projectile.owner >= 0 && projectile.owner < Main.maxPlayers && !projectile.hostile)
            {
                var mainPlayer = Main.player[projectile.owner];
                ApplyDamageOverride(mainPlayer, projectile);
            }
        }

        private void ApplyDamageOverride(Player player, Projectile projectile)
        {
            // Failsafe recovery during AI
            if (linkedItemType == 0)
            {
                if (MyGlobalItem.ProjectileToItemTypeMap.TryGetValue(projectile.type, out int mappedType))
                {
                    linkedItemType = mappedType;
                    linkedWeaponName = DamageMultiplierScale.NormalizeName(ContentSamples.ItemsByType[linkedItemType].Name);
                }
            }

            if (linkedItemType == 0 || string.IsNullOrEmpty(linkedWeaponName))
                return;

            var modPlayer = player.GetModPlayer<MyModPlayer>();
            
            if (modPlayer.WeaponConfigs.TryGetValue(linkedWeaponName, out WeaponScaleData config) && config.IsEnabled)
            {
                try
                {
                    // Calculate the exact damage requested by the UI
                    int totalScaledDamage = MyGlobalItem.CalculateTotalDamageByType(player, linkedItemType);
                    int baseScaledDamage = MyGlobalItem.CalculateMinionBaseDamageByType(player, linkedItemType);
                    
                    if (totalScaledDamage > 0)
                    {
                        projectile.originalDamage = baseScaledDamage;
                        projectile.damage = totalScaledDamage;
                    }
                }
                catch (System.Exception ex)
                {
                    Mod.Logger.Warn($"Damage is not scaling: {ex.Message}");
                }
            }
        }
    }
}