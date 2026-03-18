namespace DebugMod;

public static partial class BindableFunctions
{
    [BindableMethod(name = "Give Rosaries", category = "Consumables")]
    public static void GiveRosaries()
    {
        HeroController.instance.AddGeo(1000);
        DebugMod.LogConsole("Giving player 1000 rosaries");
    }

    [BindableMethod(name = "Give Shell Shards", category = "Consumables")]
    public static void GiveShellShards()
    {
        HeroController.instance.AddShards(100);
        DebugMod.LogConsole("Giving player 100 shell shards");
    }

    // TODO: add bind to give all items needed for the active quest(s)
}