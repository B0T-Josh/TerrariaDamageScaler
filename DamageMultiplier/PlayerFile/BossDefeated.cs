using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DamageMultiplier.PlayerFile
{
    public class BossDefeated : ModSystem
    {
        public static List<BossData> OrderedBosses = new List<BossData>();

        public struct BossData
        {
            public string InternalName;
            public float Progression;
            public List<int> NpcIDs;
            public Func<bool> IsDowned;
        }

        public override void PostAddRecipes()
        {
            OrderedBosses.Clear();

            if (ModLoader.TryGetMod("BossChecklist", out Mod bossChecklist))
            {
                var result = bossChecklist.Call("GetBossInfoDictionary", Mod, "1.1.5.5");
                
                if (result is Dictionary<string, Dictionary<string, object>> bossInfoDict)
                {
                    foreach (var entry in bossInfoDict)
                    {
                        // --- NEW FILTER: Ignore all bosses from The Stars Above ---
                        if (entry.Key.StartsWith("StarsAbove", StringComparison.OrdinalIgnoreCase))
                        {
                            continue; // Skips this boss and moves to the next one in the loop
                        }

                        var data = entry.Value;
                        
                        // Extract the data fields
                        float prog = data.ContainsKey("progression") ? Convert.ToSingle(data["progression"]) : 0f;
                        List<int> npcIds = data.ContainsKey("npcIDs") ? (data["npcIDs"] as List<int>) : new List<int>();
                        Func<bool> downed = data.ContainsKey("downed") ? (data["downed"] as Func<bool>) : () => false;
                        
                        // Check if Boss Checklist considers this a main boss
                        bool isBoss = data.ContainsKey("isBoss") ? Convert.ToBoolean(data["isBoss"]) : false;

                        // Only add to our list if it is a main boss AND has an NPC ID
                        if (isBoss && npcIds != null && npcIds.Count > 0)
                        {
                            OrderedBosses.Add(new BossData
                            {
                                InternalName = entry.Key,
                                Progression = prog,
                                NpcIDs = npcIds,
                                IsDowned = downed
                            });
                        }
                    }
                    
                    OrderedBosses = OrderedBosses.OrderBy(b => b.Progression).ToList();
                    Mod.Logger.Info($"[DamageMultiplier] Loaded {OrderedBosses.Count} MAIN bosses from Boss Checklist (Stars Above ignored)!");
                    return; 
                }
            }

            Mod.Logger.Warn("[DamageMultiplier] Boss Checklist not found. Falling back to Vanilla bosses only.");
            LoadVanillaFallback();
        }

        private void LoadVanillaFallback()
        {
            AddVanillaBoss("KingSlime", 1f, NPCID.KingSlime, () => NPC.downedSlimeKing);
            AddVanillaBoss("EyeofCthulhu", 2f, NPCID.EyeofCthulhu, () => NPC.downedBoss1);
            AddVanillaBoss("EaterofWorlds", 3f, NPCID.EaterofWorldsHead, () => NPC.downedBoss2);
            AddVanillaBoss("Skeletron", 4f, NPCID.SkeletronHead, () => NPC.downedBoss3);
            AddVanillaBoss("WallofFlesh", 5f, NPCID.WallofFlesh, () => Main.hardMode);
            AddVanillaBoss("QueenSlime", 6f, NPCID.QueenSlimeBoss, () => NPC.downedQueenSlime);
            AddVanillaBoss("TheDestroyer", 7f, NPCID.TheDestroyer, () => NPC.downedMechBoss1);
            AddVanillaBoss("TheTwins", 8f, NPCID.Spazmatism, () => NPC.downedMechBoss2);
            AddVanillaBoss("SkeletronPrime", 9f, NPCID.SkeletronPrime, () => NPC.downedMechBoss3);
            AddVanillaBoss("Plantera", 10f, NPCID.Plantera, () => NPC.downedPlantBoss);
            AddVanillaBoss("Golem", 11f, NPCID.Golem, () => NPC.downedGolemBoss);
            AddVanillaBoss("DukeFishron", 12f, NPCID.DukeFishron, () => NPC.downedFishron);
            AddVanillaBoss("EmpressOfLight", 13f, NPCID.EmpressButterfly, () => NPC.downedEmpressOfLight);
            AddVanillaBoss("LunaticCultist", 14f, NPCID.CultistBoss, () => NPC.downedAncientCultist);
            AddVanillaBoss("MoonLord", 15f, NPCID.MoonLordCore, () => NPC.downedMoonlord);
        }

        private void AddVanillaBoss(string name, float prog, int npcId, Func<bool> downedFunc)
        {
            OrderedBosses.Add(new BossData
            {
                InternalName = name,
                Progression = prog,
                NpcIDs = new List<int> { npcId },
                IsDowned = downedFunc
            });
        }
    }
}