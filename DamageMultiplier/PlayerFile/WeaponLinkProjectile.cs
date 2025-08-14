using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System.Linq;
using System.Collections.Generic;

namespace DamageMultiplier.PlayerFile
{
    public class WeaponLinkedProjectile : GlobalProjectile
    {
        // Stores the normalized weapon name responsible for this projectile
        public string linkedWeaponName = null;

        // Each projectile needs its own data
        public override bool InstancePerEntity => true;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers || projectile.hostile)
                return;

            var player = Main.player[projectile.owner].GetModPlayer<MyModPlayer>();

            var visited = new HashSet<int>();

            linkedWeaponName = GetWeaponNameFromSource(source, visited);

            if (!string.IsNullOrEmpty(linkedWeaponName) &&
                player.playerWeapons.Any(w => DamageMultiplierScale.NormalizeName(w) == linkedWeaponName))
            {
                projectile.damage = player.itemDamage;
                projectile.originalDamage = player.itemDamage;
            }
        }

        private string GetWeaponNameFromSource(IEntitySource source, HashSet<int> visited)
        {
            if (source is EntitySource_ItemUse itemSource && itemSource.Item != null)
            {
                return DamageMultiplierScale.NormalizeName(itemSource.Item.Name);
            }
            else if (source is EntitySource_Parent parentSource && parentSource.Entity is Projectile parentProj)
            {
                if (!visited.Add(parentProj.whoAmI))
                {
                    // Prevents infinite recursion in cyclic references
                    return null;
                }

                var parentGlobal = parentProj.GetGlobalProjectile<WeaponLinkedProjectile>();
                if (!string.IsNullOrEmpty(parentGlobal.linkedWeaponName))
                {
                    return parentGlobal.linkedWeaponName;
                }

                // Recurse into parent's source if no linked weapon name found
                return GetWeaponNameFromSource(parentProj.GetSource_FromThis(), visited);
            }

            return null;
        }
    }
}
