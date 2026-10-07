using System.ComponentModel;
using Terraria.ModLoader.Config;
using Newtonsoft.Json;

namespace DamageMultiplier.PlayerFile
{
    public class DamageMultiplierConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Header("StandardProgression")]
        
        [Label("Use Time Insanely Fast")]
        [Tooltip("Type a whole number. It automatically multiplies by 0.001 (e.g., 2 = 0.002)")]
        [DefaultValue(2)]
        public int ProgSpeed1Int;

        [JsonIgnore]
        public float ProgSpeed1 => ProgSpeed1Int * 0.001f;

        [Label("Use Time Very Fast")]
        [Tooltip("Type a whole number. It automatically multiplies by 0.001 (e.g., 2 = 0.002)")]
        [DefaultValue(3)]
        public int ProgSpeed2Int;

        [JsonIgnore]
        public float ProgSpeed2 => ProgSpeed2Int * 0.001f;

        [Label("Use Time Fast")]
        [Tooltip("Type a whole number. It automatically multiplies by 0.001 (e.g., 2 = 0.002)")]
        [DefaultValue(4)]
        public int ProgSpeed3Int;

        [JsonIgnore]
        public float ProgSpeed3 => ProgSpeed3Int * 0.001f;

        [Label("Use Time Average")]
        [Tooltip("Type a whole number. It automatically multiplies by 0.001 (e.g., 2 = 0.002)")]
        [DefaultValue(5)]
        public int ProgSpeed4Int;

        [JsonIgnore]
        public float ProgSpeed4 => ProgSpeed4Int * 0.001f;

        [Label("Use Time Slow")]
        [Tooltip("Type a whole number. It automatically multiplies by 0.001 (e.g., 2 = 0.002)")]
        [DefaultValue(2)]
        public int ProgSpeed5Int;

        [JsonIgnore]
        public float ProgSpeed5 => ProgSpeed5Int * 0.001f;

        [Label("Use Time Very Slow")]
        [Tooltip("Type a whole number. It automatically multiplies by 0.001 (e.g., 2 = 0.002)")]
        [DefaultValue(2)]
        public int ProgSpeed6Int;

        [JsonIgnore]
        public float ProgSpeed6 => ProgSpeed6Int * 0.001f;

        [Label("Use Time Extremely Slow")]
        [Tooltip("Type a whole number. It automatically multiplies by 0.001 (e.g., 2 = 0.002)")]
        [DefaultValue(2)]
        public int ProgSpeed7Int;

        [JsonIgnore]
        public float ProgSpeed7 => ProgSpeed7Int * 0.001f;

        [Label("Use Time Snail")]
        [Tooltip("Type a whole number. It automatically multiplies by 0.001 (e.g., 2 = 0.002)")]
        [DefaultValue(2)]
        public int ProgSpeed8Int;

        [JsonIgnore]
        public float ProgSpeed8 => ProgSpeed8Int * 0.001f;

        [Header("PostGameMultipliers")]
        
        [Label("Use Time Insanely Fast")]
        [Tooltip("Type a whole number. It automatically multiplies by 0.001 (e.g., 2 = 0.002)")]
        [DefaultValue(1)]
        public int PostSpeed1Int;

        [JsonIgnore]
        public float PostSpeed1 => PostSpeed1Int * 0.001f;

        [Label("Use Time Very Fast")]
        [Tooltip("Type a whole number. It automatically multiplies by 0.001 (e.g., 2 = 0.002)")]
        [DefaultValue(2)]
        public int PostSpeed2Int;

        [JsonIgnore]
        public float PostSpeed2 => PostSpeed2Int * 0.001f;

        [Label("Use Time Fast")]
        [Tooltip("Type a whole number. It automatically multiplies by 0.001 (e.g., 2 = 0.002)")]
        [DefaultValue(3)]
        public int PostSpeed3Int;

        [JsonIgnore]
        public float PostSpeed3 => PostSpeed3Int * 0.001f;

        [Label("Use Time Average")]
        [Tooltip("Type a whole number. It automatically multiplies by 0.001 (e.g., 2 = 0.002)")]
        [DefaultValue(4)]
        public int PostSpeed4Int;

        [JsonIgnore]
        public float PostSpeed4 => PostSpeed4Int * 0.001f;

        [Label("Use Time Slow")]
        [Tooltip("Type a whole number. It automatically multiplies by 0.001 (e.g., 2 = 0.002)")]
        [DefaultValue(5)]
        public int PostSpeed5Int;

        [JsonIgnore]
        public float PostSpeed5 => PostSpeed5Int * 0.001f; // Fixed!

        [Label("Use Time Very Slow")]
        [Tooltip("Type a whole number. It automatically multiplies by 0.001 (e.g., 2 = 0.002)")]
        [DefaultValue(6)]
        public int PostSpeed6Int;

        [JsonIgnore]
        public float PostSpeed6 => PostSpeed6Int * 0.001f; // Fixed!

        [Label("Use Time Extremely Slow")]
        [Tooltip("Type a whole number. It automatically multiplies by 0.001 (e.g., 2 = 0.002)")]
        [DefaultValue(7)]
        public int PostSpeed7Int;

        [JsonIgnore]
        public float PostSpeed7 => PostSpeed7Int * 0.001f; // Fixed!

        [Label("Use Time Snail")]
        [Tooltip("Type a whole number. It automatically multiplies by 0.001 (e.g., 2 = 0.002)")]
        [DefaultValue(8)]
        public int PostSpeed8Int;

        [JsonIgnore]
        public float PostSpeed8 => PostSpeed8Int * 0.001f; // Fixed!


        [Header("ClassAdjustments")]
        
        [Label("High-Mana Magic Multiplier")]
        [Tooltip("The flat damage multiplier for magic weapons costing more than 28 mana.")]
        [DefaultValue(2)]
        public int MagicMultiplier; // No float conversion needed since this is a clean 2x multiplier
    }
}