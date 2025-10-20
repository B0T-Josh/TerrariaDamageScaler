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

            // Run on main thread to ensure ModNPCs are registered
            Main.QueueMainThreadAction(() =>
            {
                var downedBossType = calamity.Code.GetType("CalamityMod.DownedBossSystem");
                if (downedBossType == null)
                {
                    Mod.Logger.Warn("[DamageMultiplier] ❌ DownedBossSystem type not found.");
                    return;
                }

                Mod.Logger.Info($"[DamageMultiplier] ✅ Found DownedBossSystem in {downedBossType.FullName}");

                // Manual mapping for known mismatches (field name -> ModNPC registration name)
                var manualMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "_downedDesertScourge", "DesertScourgeHead" },
                    { "_downedCrabulon", "Crabulon" },
                    { "_downedHiveMind", "HiveMind" },
                    { "_downedPerforator", "PerforatorHive" },      // adjust if needed
                    { "_downedSlimeGod", "TheSlimeGodCore" },
                    { "_downedCryogen", "Cryogen" },
                    { "_downedAquaticScourge", "AquaticScourgeHead" },
                    { "_downedBrimstoneElemental", "BrimstoneElemental" },
                    { "_downedCalamitasClone", "CalamitasClone" },
                    { "_downedLeviathan", "Leviathan" },
                    { "_downedAstrumAureus", "AstrumAureus" },
                    { "_downedPlaguebringer", "PlaguebringerGoliath" },
                    { "_downedRavager", "RavagerBody" },
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
                    { "_downedExoMechs", "AresBody" }, // example, adjust if needed
                    { "_downedSupremeCalamitas", "SupremeCalamitas" }
                    // Add more manual pairs if logs show unmatched field names
                };

                // Gather all Calamity ModNPCs for fuzzy matching (normalized name -> ModNPC)
                var calamityNpcs = calamity.GetContent<ModNPC>().ToList();
                Dictionary<string, ModNPC> normalizedNpcLookup = new(StringComparer.OrdinalIgnoreCase);
                foreach (var npc in calamityNpcs)
                {
                    string normalized = NormalizeName(npc.Name);
                    if (!normalizedNpcLookup.ContainsKey(normalized))
                        normalizedNpcLookup[normalized] = npc;
                }

                // Reflect fields (including private ones)
                var fields = downedBossType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                int added = 0;
                var unmatched = new List<string>();

                foreach (var field in fields)
                {
                    if (field.FieldType != typeof(bool))
                        continue;

                    bool defeated;
                    try
                    {
                        defeated = (bool)field.GetValue(null);
                    }
                    catch (Exception ex)
                    {
                        Mod.Logger.Warn($"[DamageMultiplier] ⚠ Could not read {field.Name}: {ex.Message}");
                        continue;
                    }

                    // Try manual map first
                    string modNpcName = null;
                    if (manualMap.TryGetValue(field.Name, out string m))
                        modNpcName = m;

                    ModNPC foundNpc = null;

                    // 1) Direct TryFind using manual mapping or cleaned name
                    if (!string.IsNullOrEmpty(modNpcName))
                    {
                        if (calamity.TryFind<ModNPC>(modNpcName, out var npc1))
                            foundNpc = npc1;
                    }
                    else
                    {
                        // clean field name: remove leading '_' and 'downed' prefix
                        string candidate = field.Name;
                        if (candidate.StartsWith("_"))
                            candidate = candidate.Substring(1);
                        if (candidate.StartsWith("downed", StringComparison.OrdinalIgnoreCase))
                            candidate = candidate.Substring(6);

                        // try direct find with candidate
                        if (calamity.TryFind<ModNPC>(candidate, out var npc2))
                            foundNpc = npc2;
                    }

                    // 2) Fuzzy lookup by normalized names (if still not found)
                    if (foundNpc == null)
                    {
                        string candidateNorm = NormalizeName(field.Name);
                        // try variants: remove underscore and 'downed' etc.
                        candidateNorm = candidateNorm.Replace("downed", "");
                        candidateNorm = candidateNorm.Trim();

                        // exact normalized match
                        if (normalizedNpcLookup.TryGetValue(candidateNorm, out var npcExact))
                            foundNpc = npcExact;
                        else
                        {
                            // substring match: find first npc whose normalized name contains candidateNorm or vice versa
                            foreach (var kv in normalizedNpcLookup)
                            {
                                if (string.IsNullOrEmpty(candidateNorm)) break;
                                if (kv.Key.Contains(candidateNorm) || candidateNorm.Contains(kv.Key))
                                {
                                    foundNpc = kv.Value;
                                    break;
                                }
                            }
                        }
                    }

                    // 3) If found, set dictionary by NPC Type (the correct int key)
                    if (foundNpc != null)
                    {
                        bossDefeated[foundNpc.Type] = defeated;
                        Mod.Logger.Info($"[DamageMultiplier] 🧩 {foundNpc.Name} (Type {foundNpc.Type}) ← {field.Name} = {defeated}");
                        added++;
                    }
                    else
                    {
                        unmatched.Add(field.Name);
                        Mod.Logger.Warn($"[DamageMultiplier] ⚠ Unmatched downed flag: {field.Name}");
                    }
                }

                Mod.Logger.Info($"[DamageMultiplier] ✅ Calamity bosses mapped: {added}. Unmatched: {unmatched.Count}.");
                if (unmatched.Count > 0)
                    Mod.Logger.Info("[DamageMultiplier] Unmatched flags: " + string.Join(", ", unmatched));
            });

            // Helper to normalize names: remove non-letters, spaces, to lower
            static string NormalizeName(string s)
            {
                if (string.IsNullOrEmpty(s)) return s ?? "";
                var sb = new System.Text.StringBuilder(s.Length);
                foreach (char c in s)
                {
                    if (char.IsLetter(c))
                        sb.Append(char.ToLowerInvariant(c));
                }
                return sb.ToString();
            }
        }


    }
}
