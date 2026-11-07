namespace DebugMod;

public static partial class BindableFunctions
{
    [BindableMethod(name = "MISC_TOGGLEACT3", category = "CATEGORY_MISC")]
    public static void ToggleAct3()
    {
        PlayerData.instance.blackThreadWorld = !PlayerData.instance.blackThreadWorld;
        DebugMod.LogConsole("Act 3 world is now " + (PlayerData.instance.blackThreadWorld ? "enabled" : "disabled")
            + ", reload the scene to apply changes");
    }

    // TODO(port): kill shade on HK
    [BindableMethod(name = "MISC_BREAKCOCOON", category = "CATEGORY_MISC")]
    public static void BreakCocoon()
    {
        HeroController.instance?.CocoonBroken();
        EventRegister.SendEvent("BREAK HERO CORPSE");
    }

    [BindableMethod(name = "MISC_QUEUEWALLJUMPINTERRUPT", category = "CATEGORY_MISC")]
    public static void QueueWallJump()
    {
        HeroController.instance.queuedWallJumpInterrupt = true;
    }

    private static partial void ClearSceneData(string sceneName)
    {
        SceneData.instance.persistentBools.scenes.Remove(sceneName);
        SceneData.instance.persistentInts.scenes.Remove(sceneName);
        SceneData.instance.geoRocks.scenes.Remove(sceneName);
    }
}
