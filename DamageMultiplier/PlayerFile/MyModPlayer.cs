using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace DamageMultiplier.PlayerFile
{
    // New data structure to hold individual weapon settings
    public class WeaponScaleData
    {
        public bool IsEnabled;
        public int DamageScale;

        public WeaponScaleData(bool isEnabled, int damageScale)
        {
            IsEnabled = isEnabled;
            DamageScale = damageScale;
        }
    }

    public class MyModPlayer : ModPlayer
    {
        // Replaces List<string> playerWeapons for individual UI configurations
        public Dictionary<string, WeaponScaleData> WeaponConfigs = new Dictionary<string, WeaponScaleData>();

        // Restored: Tracks the damage ratio of reforges (in-memory only, no need to save)
        public Dictionary<string, float> reforgeMultipliers = new Dictionary<string, float>();

        public override void SaveData(TagCompound tag)
        {
            var keys = new List<string>(WeaponConfigs.Keys);
            var enabledList = new List<bool>();
            var scaleList = new List<int>();

            foreach (var key in keys)
            {
                enabledList.Add(WeaponConfigs[key].IsEnabled);
                scaleList.Add(WeaponConfigs[key].DamageScale);
            }

            tag["weaponKeys"] = keys;
            tag["weaponEnabled"] = enabledList;
            tag["weaponScale"] = scaleList;
        }

        public override void LoadData(TagCompound tag)
        {
            WeaponConfigs.Clear();
            if (tag.ContainsKey("weaponKeys"))
            {
                var keys = tag.GetList<string>("weaponKeys");
                var enabledList = tag.GetList<bool>("weaponEnabled");
                var scaleList = tag.GetList<int>("weaponScale");

                for (int i = 0; i < keys.Count; i++)
                {
                    WeaponConfigs[keys[i]] = new WeaponScaleData(enabledList[i], scaleList[i]);
                }
            }
        }
    }
}