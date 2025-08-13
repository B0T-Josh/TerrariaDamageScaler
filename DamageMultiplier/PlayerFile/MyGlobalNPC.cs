using MonoMod.Core.Utils;
using Terraria;
using Terraria.ModLoader;

namespace DamageMultiplier.PlayerFile
{
    public class MyGlobalNPC : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            if (npc.boss)
            {
                BossDefeated.bossDefeated[npc.type] = true;
                Main.NewText($"Boss defeated: {Lang.GetNPCNameValue(npc.type)}");
            }
        }
    }

}