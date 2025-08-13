using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System.Collections.Generic;
using System.Linq;
using Terraria.WorldBuilding;
//Fix Summon Damage

namespace DamageMultiplier.PlayerFile
{
    public class DamageMultiplierScale : ModPlayer
    {
        public static string NormalizeName(string name) => name.Replace(" ", "").Trim().ToLowerInvariant();
        public static int GetBossScaleHP(int npcId)
        {
            NPC npc = new NPC();
            npc.SetDefaults(npcId);
            return npc.lifeMax;
        }
    }
}
