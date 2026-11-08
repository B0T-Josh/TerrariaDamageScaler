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

        // NEW: Cached values updated dynamically while inside the world
        public static int CurrentTargetHP = 0;
        public static string CurrentTargetName = "None";
        private int updateTimer = 0;

        public struct BossData
        {
            public string InternalName;
            public float Progression;
            public List<int> NpcIDs;
            public Func<bool> IsDowned;
            public Func<bool> IsAvailable; 
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
                            OrderedBosses.Add(new BossData
                            {
                                InternalName = entry.Key,
                                Progression = prog,
                                NpcIDs = npcIds,
                                IsDowned = downed,
                                IsAvailable = () => available() && !hidden()
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
            OrderedBosses.Add(new BossData
            {
                InternalName = name,
                Progression = prog,
                NpcIDs = new List<int> { npcId },
                IsDowned = downedFunc,
                IsAvailable = () => true
            });
        }

        public override void OnWorldLoad()
        {
            CurrentTargetHP = 0;
            CurrentTargetName = "None";
            updateTimer = 60; // Force immediate calculation on world entry
        }

        // NEW: Background loop that calculates HP while inside the world
        public override void PostUpdateWorld()
        {
            updateTimer++;
            // Update once per second (60 ticks) so it doesn't cause frame lag
            if (updateTimer >= 60)
            {
                updateTimer = 0;
                UpdateProgressionTarget();
            }
        }

        public static void UpdateProgressionTarget()
        {
            if (OrderedBosses.Count == 0) return;

            float highestProgression = -1f;
            foreach (var boss in OrderedBosses)
            {
                if (boss.IsDowned.Invoke() && boss.Progression > highestProgression)
                {
                    highestProgression = boss.Progression;
                }
            }

            BossData targetBoss = default;
            bool found = false;
            
            foreach (var boss in OrderedBosses)
            {
                if (boss.IsAvailable.Invoke() && !boss.IsDowned.Invoke() && boss.Progression > highestProgression)
                {
                    targetBoss = boss;
                    found = true;
                    break; 
                }
            }

            if (!found)
            {
                for (int i = OrderedBosses.Count - 1; i >= 0; i--)
                {
                    if (OrderedBosses[i].IsAvailable.Invoke())
                    {
                        targetBoss = OrderedBosses[i];
                        found = true;
                        break;
                    }
                }
            }

            if (found)
            {
                CurrentTargetName = targetBoss.InternalName;
                CurrentTargetHP = CalculateDynamicHP(targetBoss.NpcIDs);
            }
        }

        private static int CalculateDynamicHP(List<int> npcIDs)
        {
            int maxHP = 0;
            
            // FIX: Pass the active GameModeData to the dummy NPC so Calamity can apply Revengeance multipliers
            NPCSpawnParams spawnParams = new NPCSpawnParams();
            spawnParams.gameModeData = Main.GameModeInfo;

            foreach (int id in npcIDs)
            {
                NPC tempNpc = new NPC();
                tempNpc.SetDefaults(id, spawnParams);
                if (tempNpc.lifeMax > maxHP)
                {
                    maxHP = tempNpc.lifeMax;
                }
            }
            return maxHP;
        }
    }
}