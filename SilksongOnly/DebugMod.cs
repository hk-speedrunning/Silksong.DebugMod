using BepInEx;
using GlobalSettings;
using JetBrains.Annotations;

namespace DebugMod;

[BepInDependency("org.silksong-modding.modlist", "0.2.0")]
public partial class DebugMod
{
    [CanBeNull] internal static HealthManager.DamageScalingConfig lastScaling;
    internal static int lastScaleLevel;

    static partial void ResetCharacterState()
    {
        lastScaling = null;
        lastScaleLevel = 0;
    }

    static partial void CheckLethalDamagePrevented(ref bool willDie)
    {
        ToolItem fracturedMask = Gameplay.FracturedMaskTool;
        if (fracturedMask && fracturedMask.IsEquipped && fracturedMask.SavedData.AmountLeft > 0)
        {
            willDie = false;
        }
    }
}
