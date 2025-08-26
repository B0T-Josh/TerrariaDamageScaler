using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace DamageMultiplier.PlayerFile
{
    public class BossDefeated : ModSystem
    {
        // Dictionary to track boss kills (replace int with NPC ID)
        public static Dictionary<int, bool> bossDefeated = new Dictionary<int, bool>();
        public static List<int> vanillaBosses = new List<int>()
        {
            NPCID.KingSlime,
            NPCID.EyeofCthulhu,
            NPCID.SkeletronHead,
            NPCID.WallofFlesh,
            NPCID.QueenSlimeBoss,
            NPCID.Spazmatism,
            NPCID.SkeletronPrime,
            NPCID.TheDestroyer,
            NPCID.Plantera,
            NPCID.Golem,
            NPCID.DukeFishron,
            NPCID.HallowBoss,
            NPCID.CultistBoss,
            NPCID.MoonLordCore,
        };

        public static List<int> CalamityBosses = new List<int>();

        public override void OnWorldLoad()
        {
            bossDefeated = new Dictionary<int, bool>();
            CalamityBosses.Clear();

            // If Calamity is installed, populate CalamityBosses now
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                CalamityBosses.Add(NPCID.KingSlime);
                CalamityBosses.Add(NPCID.EyeofCthulhu);
                CalamityBosses.Add(NPCID.SkeletronHead);
                CalamityBosses.Add(NPCID.WallofFlesh);
                CalamityBosses.Add(NPCID.QueenSlimeBoss);
                CalamityBosses.Add(NPCID.Spazmatism);
                CalamityBosses.Add(NPCID.SkeletronPrime);
                CalamityBosses.Add(calamity.Find<ModNPC>("Cryogen").Type);
                CalamityBosses.Add(calamity.Find<ModNPC>("AquaticScourgeHead").Type);
                CalamityBosses.Add(NPCID.TheDestroyer);
                CalamityBosses.Add(calamity.Find<ModNPC>("CalamitasClone").Type);
                CalamityBosses.Add(NPCID.Plantera);
                CalamityBosses.Add(calamity.Find<ModNPC>("AstrumAureus").Type);
                CalamityBosses.Add(NPCID.Golem);
                CalamityBosses.Add(NPCID.DukeFishron);
                CalamityBosses.Add(calamity.Find<ModNPC>("PlaguebringerGoliath").Type);
                CalamityBosses.Add(NPCID.HallowBoss);
                CalamityBosses.Add(calamity.Find<ModNPC>("RavagerBody").Type);
                CalamityBosses.Add(NPCID.CultistBoss);
                CalamityBosses.Add(calamity.Find<ModNPC>("AstrumDeusHead").Type);
                CalamityBosses.Add(NPCID.MoonLordCore);
                CalamityBosses.Add(calamity.Find<ModNPC>("ProfanedGuardianCommander").Type);
                CalamityBosses.Add(calamity.Find<ModNPC>("Providence").Type);
                CalamityBosses.Add(calamity.Find<ModNPC>("CeaselessVoid").Type);
                CalamityBosses.Add(calamity.Find<ModNPC>("StormWeaverBody").Type);
                CalamityBosses.Add(calamity.Find<ModNPC>("Signus").Type);
                CalamityBosses.Add(calamity.Find<ModNPC>("Polterghast").Type);
                CalamityBosses.Add(calamity.Find<ModNPC>("OldDuke").Type);
                CalamityBosses.Add(calamity.Find<ModNPC>("DevourerofGodsBody").Type);
                CalamityBosses.Add(calamity.Find<ModNPC>("Yharon").Type);
                CalamityBosses.Add(calamity.Find<ModNPC>("SupremeCalamitas").Type);
            }
        }

        public override void OnWorldUnload()
        {
            // Clear static data when leaving a world
            bossDefeated.Clear();
            CalamityBosses.Clear();
            vanillaBosses = new List<int>()
            {
                NPCID.KingSlime,
                NPCID.EyeofCthulhu,
                NPCID.BrainofCthulhu,
                NPCID.SkeletronHead,
                NPCID.WallofFlesh,
                NPCID.QueenSlimeBoss,
                NPCID.Spazmatism,
                NPCID.SkeletronPrime,
                NPCID.TheDestroyer,
                NPCID.Plantera,
                NPCID.Golem,
                NPCID.DukeFishron,
                NPCID.HallowBoss,
                NPCID.CultistBoss,
                NPCID.MoonLordCore,
            };
        }


        public override void SaveWorldData(TagCompound tag)
        {
            // Save defeated bosses as a list
            tag["defeatedBosses"] = bossDefeated.Where(b => b.Value).Select(b => b.Key).ToList();
        }

        public override void LoadWorldData(TagCompound tag)
        {
            bossDefeated = new Dictionary<int, bool>();

            var bossList = CalamityBosses.Any() ? CalamityBosses : vanillaBosses;

            // Set all bosses to false by default
            foreach (var id in bossList)
                bossDefeated[id] = false;

            if (tag.ContainsKey("defeatedBosses"))
            {
                var defeatedList = tag.GetList<int>("defeatedBosses");

                foreach (var bossId in defeatedList)
                {
                    bossDefeated[bossId] = true;
                }
            }
        }
    }
}
