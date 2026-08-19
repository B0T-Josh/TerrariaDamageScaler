using System.Collections.Generic;
using System.IO;
using Terraria;
using System.Linq;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace DamageMultiplier.PlayerFile
{
    public class BossDefeated : ModSystem
    {
        public static Dictionary<int, bool> bossDefeated = new();
        private bool calamityChecked = false;

        public override void OnWorldLoad()
        {
            ModContent.GetInstance<DamageMultiplier>().Logger.Info("[DamageMultiplier] OnWorldLoad() triggered");
            LoadVanillaBossProgress();
            TryLoadCalamityBossProgress();
        }

        public override void LoadWorldData(TagCompound tag)
        {
            ModContent.GetInstance<DamageMultiplier>().Logger.Info("[DamageMultiplier] LoadWorldData() triggered");
            LoadVanillaBossProgress();
            TryLoadCalamityBossProgress();
        }

        public override void PostWorldGen()
        {
            ModContent.GetInstance<DamageMultiplier>().Logger.Info("[DamageMultiplier] PostWorldGen() triggered");
            TryLoadCalamityBossProgress();
        }

        public override void PostUpdateEverything()
        {
            if (!calamityChecked)
            {
                calamityChecked = true;
                TryLoadCalamityBossProgress();
                Main.NewText("Loaded Calamity boss progress!", Microsoft.Xna.Framework.Color.Cyan);
            }
        }

        public override void SaveWorldData(TagCompound tag) { }

        public override void NetSend(BinaryWriter writer)
        {
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

        private void LoadVanillaBossProgress()
        {
            bossDefeated.Clear();

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
        }

        private void TryLoadCalamityBossProgress()
        {
            Mod.Logger.Info("[DamageMultiplier] ▶ TryLoadCalamityBossProgress() called");

            if (!ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                Mod.Logger.Warn("[DamageMultiplier] ❌ CalamityMod not found.");
                return;
            }

            // Run safely on the main thread so Calamity NPCs are ready
            Main.QueueMainThreadAction(() =>
            {
                var downedBossType = calamity.Code.GetType("CalamityMod.DownedBossSystem");
                if (downedBossType == null)
                {
                    Mod.Logger.Warn("[DamageMultiplier] ❌ DownedBossSystem type not found.");
                    return;
                }

                Mod.Logger.Info($"[DamageMultiplier] ✅ Found DownedBossSystem in {downedBossType.FullName}");

                // ✅ Only these bosses will be tracked
                var manualMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "_downedAstrumDeus", "AstrumDeusHead" },
                    { "_downedGuardians", "ProfanedGuardianCommander" },
                    { "_downedProvidence", "Providence" },
                    { "_downedStormWeaver", "StormWeaverHead" },
                    { "_downedCeaselessVoid", "CeaselessVoid" },
                    { "_downedSignus", "Signus" },
                    { "_downedPolterghast", "Polterghast" },
                    { "_downedBoomerDuke", "OldDuke" },
                    { "_downedDoG", "DevourerofGodsHead" },
                    { "_downedYharon", "Yharon" },
                    { "_downedAres", "AresBody" },
                    { "_downedExoMechs", "AresBody" },
                };

                var fields = downedBossType.GetFields(System.Reflection.BindingFlags.Public |
                                                    System.Reflection.BindingFlags.NonPublic |
                                                    System.Reflection.BindingFlags.Static);

                int added = 0;
                var unmatched = new List<string>();

                foreach (var pair in manualMap)
                {
                    string fieldName = pair.Key;
                    string modNpcName = pair.Value;

                    // Try to find matching field in DownedBossSystem
                    var field = fields.FirstOrDefault(f => f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
                    if (field == null)
                    {
                        Mod.Logger.Warn($"[DamageMultiplier] ⚠ Field {fieldName} not found in DownedBossSystem.");
                        unmatched.Add(fieldName);
                        continue;
                    }

                    if (field.FieldType != typeof(bool))
                    {
                        Mod.Logger.Warn($"[DamageMultiplier] ⚠ Field {fieldName} is not boolean.");
                        unmatched.Add(fieldName);
                        continue;
                    }

                    bool defeated;
                    try
                    {
                        defeated = (bool)field.GetValue(null);
                    }
                    catch (Exception ex)
                    {
                        Mod.Logger.Warn($"[DamageMultiplier] ⚠ Could not read {field.Name}: {ex.Message}");
                        unmatched.Add(fieldName);
                        continue;
                    }

                    // Find the ModNPC from Calamity
                    if (!calamity.TryFind<ModNPC>(modNpcName, out var npc))
                    {
                        Mod.Logger.Warn($"[DamageMultiplier] ⚠ Could not find ModNPC for {modNpcName}");
                        unmatched.Add(fieldName);
                        continue;
                    }

                    // ✅ Add to bossDefeated dictionary
                    bossDefeated[npc.Type] = defeated;
                    Mod.Logger.Info($"[DamageMultiplier] 🧩 Added {npc.Name} (Type {npc.Type}) ← {field.Name} = {defeated}");
                    added++;
                }

                Mod.Logger.Info($"[DamageMultiplier] ✅ Calamity bosses mapped (manual list only): {added}. Unmatched: {unmatched.Count}.");
                if (unmatched.Count > 0)
                    Mod.Logger.Info("[DamageMultiplier] Unmatched manual flags: " + string.Join(", ", unmatched));
            });
        }



    }
}
