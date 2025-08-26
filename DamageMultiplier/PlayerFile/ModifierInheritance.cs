using Terraria.ModLoader;

namespace DamageMultiplier.PlayerFile
{
    public class ModifierInheritance : DamageClass
    {
        public override StatInheritanceData GetModifierInheritance(DamageClass damageClass)
        {
            if (damageClass == DamageClass.Generic)
                return StatInheritanceData.Full;

            return new StatInheritanceData(
                damageInheritance: 1f,    
                critChanceInheritance: 1f,
                attackSpeedInheritance: 1f,
                knockbackInheritance: 1f
            );
        }
    }
}
