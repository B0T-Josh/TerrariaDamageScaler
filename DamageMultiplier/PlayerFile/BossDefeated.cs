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
            public Func<bool> IsAvailable; 
            public int MaxHP; 
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
                        if (entry.Key.StartsWith("StarsAbove", StringComparison.OrdinalIgnoreCase))
                            continue;

                        var data = entry.Value;
                        float prog = data.ContainsKey("progression") ? Convert.ToSingle(data["progression"]) : 0f;
                        List<int> npcIds = data.ContainsKey("npcIDs") ? (data["npcIDs"] as List<int>) : new List<int>();
                        Func<bool> downed = data.ContainsKey("downed") ? (data["downed"] as Func<bool>) : () => false;
                        
                        // FIX: Safely extract both availability AND hidden flags to strictly match the UI book
                        Func<bool> available = () => true;
                        if (data.ContainsKey("availability") && data["availability"] is Func<bool> avail1) available = avail1;
                        else if (data.ContainsKey("available") && data["available"] is Func<bool> avail2) available = avail2;

                        Func<bool> hidden = () => false;
                        if (data.ContainsKey("hidden") && data["hidden"] is Func<bool> hid) hidden = hid;

                        bool isBoss = data.ContainsKey("isBoss") && Convert.ToBoolean(data["isBoss"]);
                        bool isMiniBoss = data.ContainsKey("isMiniBoss") && Convert.ToBoolean(data["isMiniBoss"]);
                        bool isEvent = data.ContainsKey("isEvent") && Convert.ToBoolean(data["isEvent"]);

                        if (isBoss && !isMiniBoss && !isEvent && npcIds != null && npcIds.Count > 0)
                        {
                            // FIX: Find the single tankiest part of the boss. 
                            // This gives Slime God its true 10,000 HP without over-inflating it to 20k+.
                            int maxHPOfParts = 0;
                            foreach (int id in npcIds)
                            {
                                NPC tempNpc = new NPC();
                                tempNpc.SetDefaults(id);
                                if (tempNpc.lifeMax > maxHPOfParts)
                                {
                                    maxHPOfParts = tempNpc.lifeMax;
                                }
                            }

                            OrderedBosses.Add(new BossData
                            {
                                InternalName = entry.Key,
                                Progression = prog,
                                NpcIDs = npcIds,
                                IsDowned = downed,
                                IsAvailable = () => available() && !hidden(), // Hides Perforators in a Corruption world
                                MaxHP = maxHPOfParts 
                            });
                        }
                    }
                    
                    OrderedBosses = OrderedBosses.OrderBy(b => b.Progression).ToList();
                    return; 
                }
            }

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
            NPC tempNpc = new NPC();
            tempNpc.SetDefaults(npcId);

            OrderedBosses.Add(new BossData
            {
                InternalName = name,
                Progression = prog,
                NpcIDs = new List<int> { npcId },
                IsDowned = downedFunc,
                IsAvailable = () => true,
                MaxHP = tempNpc.lifeMax
            });
        }
    }
}