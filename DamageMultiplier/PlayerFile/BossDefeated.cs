using System.Collections.Generic;
using System.Linq;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace DamageMultiplier.PlayerFile
{
    public class BossDefeated : ModSystem
    {
        public static Dictionary<int, bool> bossDefeated = new();

        public override void OnWorldLoad()
        {
            LoadBossProgress();
        }

        public override void LoadWorldData(TagCompound tag)
        {
            LoadBossProgress();
        }

        public override void SaveWorldData(TagCompound tag)
        {
            // Required override, even if empty
        }

        public override void NetSend(BinaryWriter writer)
        {
            // Sync boss progress to clients
            writer.Write(bossDefeated.Count);
            foreach (var pair in bossDefeated)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value);
            }
        }

        public override void NetReceive(BinaryReader reader)
        {
            bossDefeated.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int key = reader.ReadInt32();
                bool value = reader.ReadBoolean();
                bossDefeated[key] = value;
            }
        }

        private void LoadBossProgress()
        {
            bossDefeated.Clear();

            // ✅ Vanilla bosses
            bossDefeated[NPCID.KingSlime] = NPC.downedSlimeKing;
            bossDefeated[NPCID.EyeofCthulhu] = NPC.downedBoss1;
            bossDefeated[NPCID.BrainofCthulhu] = NPC.downedBoss2;
            bossDefeated[NPCID.SkeletronHead] = NPC.downedBoss3;
            bossDefeated[NPCID.WallofFlesh] = Main.hardMode;
            bossDefeated[NPCID.QueenSlimeBoss] = NPC.downedQueenSlime;
            bossDefeated[NPCID.TheDestroyer] = NPC.downedMechBoss1;
            bossDefeated[NPCID.Spazmatism] = NPC.downedMechBoss2;
            bossDefeated[NPCID.SkeletronPrime] = NPC.downedMechBoss3;
            bossDefeated[NPCID.Plantera] = NPC.downedPlantBoss;
            bossDefeated[NPCID.Golem] = NPC.downedGolemBoss;
            bossDefeated[NPCID.DukeFishron] = NPC.downedFishron;
            bossDefeated[NPCID.EmpressButterfly] = NPC.downedEmpressOfLight;
            bossDefeated[NPCID.CultistBoss] = NPC.downedAncientCultist;
            bossDefeated[NPCID.MoonLordCore] = NPC.downedMoonlord;

            // ✅ Add Calamity bosses if loaded
            TryLoadCalamityBossProgress();
        }

        private void TryLoadCalamityBossProgress()
        {
            if (!ModLoader.TryGetMod("CalamityMod", out Mod calamity))
                return;

            var downedBossType = calamity.Code.GetType("CalamityMod.World.DownedBossSystem");
            if (downedBossType == null)
                return;

            var fields = downedBossType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                                       .Where(f => f.FieldType == typeof(bool));

            foreach (var field in fields)
            {
                try
                {
                    bool defeated = (bool)(field.GetValue(null) ?? false);
                    string cleanName = field.Name.Replace("Downed", "");

                    var modNPC = calamity.Find<ModNPC>(cleanName);
                    if (modNPC != null)
                    {
                        bossDefeated[modNPC.Type] = defeated;
                    }
                }
                catch { }
            }
        }

        public Dictionary<int, bool> GetAllBossProgress() => bossDefeated;
    }
}
