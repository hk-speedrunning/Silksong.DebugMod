namespace DebugMod;

public static partial class BindableFunctions
{
    [BindableMethod(name = "CONSUMABLES_GIVEROSARIES", category = "CATEGORY_CONSUMABLES")]
    public static void GiveRosaries()
    {
        HeroController.instance.AddGeo(1000);
        DebugMod.LogConsole("Giving player 1000 rosaries");
    }
}
