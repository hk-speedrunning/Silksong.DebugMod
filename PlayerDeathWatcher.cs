namespace DebugMod;

public static class PlayerDeathWatcher
{
    public static void Init()
    {
        ModHooks.BeforePlayerDeadHook += SetPlayerDead;
    }

    public static void Unload()
    {
        ModHooks.BeforePlayerDeadHook -= SetPlayerDead;
    }

    public static bool playerDead;

    private static void SetPlayerDead()
    {
        playerDead = true;
        LogDeathDetails();
    }

    public static void Reset()
    {
        playerDead = false;
    }

    public static void LogDeathDetails()
    {
        DebugMod.LogConsole("Hero death detected");
        DebugMod.LogConsole($"\tGame playtime: {PlayerData.instance.playTime}");
        DebugMod.LogConsole($"\tRespawn scene: {PlayerData.instance.respawnScene}");
        DebugMod.LogConsole($"\tCocoon scene: {PlayerData.instance.HeroCorpseScene}");
        DebugMod.LogConsole($"\tCocoon rosaries: {PlayerData.instance.HeroCorpseMoneyPool}");
    }
}