using System.ComponentModel;
using Terraria.ModLoader.Config;
using Newtonsoft.Json;

namespace DamageMultiplier.PlayerFile
{
    public class DamageMultiplierConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Header("StandardProgression")]
        
        [Label("Use Time <= 8")]
        [Tooltip("Type a whole number. It automatically multiplies by 0.001 (e.g., 2 = 0.002)")]
        [DefaultValue(2)]
        public int ProgSpeed1Int;

        [JsonIgnore]
        public float ProgSpeed1 => ProgSpeed1Int * 0.001f;

        [Label("Use Time 10 - 19")]
        [DefaultValue(3)]
        public int ProgSpeed2Int;

        [JsonIgnore]
        public float ProgSpeed2 => ProgSpeed2Int * 0.001f;

        [Label("Use Time 20 - 29")]
        [DefaultValue(4)]
        public int ProgSpeed3Int;

        [JsonIgnore]
        public float ProgSpeed3 => ProgSpeed3Int * 0.001f;

        [Label("Use Time >= 30")]
        [DefaultValue(5)]
        public int ProgSpeed4Int;

        [JsonIgnore]
        public float ProgSpeed4 => ProgSpeed4Int * 0.001f;


        [Header("PostGameMultipliers")]
        
        [Label("Post-Game Use Time < 10")]
        [Tooltip("Type a whole number. It automatically multiplies by 0.001 (e.g., 100 = 0.1)")]
        [DefaultValue(100)]
        public int PostSpeed1Int;

        [JsonIgnore]
        public float PostSpeed1 => PostSpeed1Int * 0.001f;

        [Label("Post-Game Use Time 10 - 19")]
        [DefaultValue(200)]
        public int PostSpeed2Int;

        [JsonIgnore]
        public float PostSpeed2 => PostSpeed2Int * 0.001f;

        [Label("Post-Game Use Time 20 - 29")]
        [DefaultValue(300)]
        public int PostSpeed3Int;

        [JsonIgnore]
        public float PostSpeed3 => PostSpeed3Int * 0.001f;

        [Label("Post-Game Use Time >= 30")]
        [DefaultValue(500)]
        public int PostSpeed4Int;

        [JsonIgnore]
        public float PostSpeed4 => PostSpeed4Int * 0.001f;


        [Header("ClassAdjustments")]
        
        [Label("High-Mana Magic Multiplier")]
        [Tooltip("The flat damage multiplier for magic weapons costing more than 28 mana.")]
        [DefaultValue(2)]
        public int MagicMultiplier; // No float conversion needed since this is a clean 2x multiplier
    }
}