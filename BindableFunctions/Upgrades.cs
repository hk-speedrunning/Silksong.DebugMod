namespace DebugMod;

public static partial class BindableFunctions
{
    [BindableMethod(name = "Increase Needle Damage", category = "Upgrades")]
    public static void IncreaseNeedleDamage()
    {
        if (PlayerData.instance.nailDamage == 0)
        {
            PlayerData.instance.nailUpgrades = 0;
            DebugMod.extraNailDamage = 0;
            DebugMod.LogConsole("Resetting needle damage to 5");
        }
        else if (PlayerData.instance.nailUpgrades == 4 || DebugMod.extraNailDamage < 0)
        {
            DebugMod.extraNailDamage += 4;
            DebugMod.LogConsole("Adding 4 extra needle damage");
        }
        else
        {
            PlayerData.instance.nailUpgrades++;
            DebugMod.LogConsole("Adding needle upgrade");
        }

        PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
    }

    [BindableMethod(name = "Decrease Needle Damage", category = "Upgrades")]
    public static void DecreaseNeedleDamage()
    {
        if (PlayerData.instance.nailUpgrades == 0 || DebugMod.extraNailDamage > 0)
        {
            DebugMod.extraNailDamage -= 4;
            if (DebugMod.extraNailDamage < -5)
            {
                DebugMod.extraNailDamage = -5;
                DebugMod.LogConsole("Setting needle damage to 0");
            }
            else
            {
                DebugMod.LogConsole("Reducing nail damage by 4");
            }
        }
        else
        {
            PlayerData.instance.nailUpgrades--;
            DebugMod.LogConsole("Removing needle upgrade");
        }

        PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
    }

    [BindableMethod(name = "Increment Crafting Kit", category = "Upgrades")]
    public static void IncrementKits()
    {
        if (PlayerData.instance.ToolKitUpgrades < 4)
        {
            PlayerData.instance.ToolKitUpgrades++;
            DebugMod.LogConsole($"Increasing crafting kit level (now {PlayerData.instance.ToolKitUpgrades})");
        }
        else
        {
            DebugMod.LogConsole("Crafting kit already at max level");
        }
    }

    [BindableMethod(name = "Decrement Crafting Kit", category = "Upgrades")]
    public static void DecrementKits()
    {
        if (PlayerData.instance.ToolKitUpgrades > 0)
        {
            PlayerData.instance.ToolKitUpgrades--;
            DebugMod.LogConsole($"Decreasing crafting kit level (now {PlayerData.instance.ToolKitUpgrades})");
        }
        else
        {
            DebugMod.LogConsole("Crafting kit already at base level");
        }
    }

    [BindableMethod(name = "Increment Tool Pouch", category = "Upgrades")]
    public static void IncrementPouches()
    {
        if (PlayerData.instance.ToolPouchUpgrades < 4)
        {
            PlayerData.instance.ToolPouchUpgrades++;
            DebugMod.LogConsole($"Increasing tool pouch level (now {PlayerData.instance.ToolPouchUpgrades})");
        }
        else
        {
            DebugMod.LogConsole("Tool pouch already at max level");
        }
    }

    [BindableMethod(name = "Decrement Tool Pouch", category = "Upgrades")]
    public static void DecrementPouches()
    {
        if (PlayerData.instance.ToolPouchUpgrades > 0)
        {
            PlayerData.instance.ToolPouchUpgrades--;
            DebugMod.LogConsole($"Decreasing tool pouch level (now {PlayerData.instance.ToolPouchUpgrades})");
        }
        else
        {
            DebugMod.LogConsole("Tool pouch already at base level");
        }
    }

    [BindableMethod(name = "Increment Silk Hearts", category = "Upgrades")]
    public static void IncrementSilkHeart()
    {
        PlayerData.instance.silkRegenMax++;
        DebugMod.LogConsole($"Incremented Silk Hearts (now {PlayerData.instance.silkRegenMax})");
    }

    [BindableMethod(name = "Decrement Silk Hearts", category = "Upgrades")]
    public static void DecrementSilkHeart()
    {
        if (PlayerData.instance.silkRegenMax > 0)
        {
            PlayerData.instance.silkRegenMax--;
            DebugMod.LogConsole($"Decremented Silk Hearts (now {PlayerData.instance.silkRegenMax})");
        }
        else
        {
            DebugMod.LogConsole("Can't decrement below 0 Silk Hearts!");
        }
    }

    [BindableMethod(name = "Unlock All Maps", category = "Upgrades")]
    public static void UnlockAllMaps()
    {
        if (PlayerData.instance.HasAllMaps)
        {
            PlayerData.instance.HasAbyssMap = false;
            PlayerData.instance.HasAqueductMap = false;
            PlayerData.instance.HasArboriumMap = false;
            PlayerData.instance.HasBellhartMap = false;
            PlayerData.instance.HasBoneforestMap = false;
            PlayerData.instance.HasCitadelUnderstoreMap = false;
            PlayerData.instance.HasCloverMap = false;
            PlayerData.instance.HasCogMap = false;
            PlayerData.instance.HasCoralMap = false;
            PlayerData.instance.HasCradleMap = false;
            PlayerData.instance.HasCrawlMap = false;
            PlayerData.instance.HasDocksMap = false;
            PlayerData.instance.HasDustpensMap = false;
            PlayerData.instance.HasGreymoorMap = false;
            PlayerData.instance.HasHallsMap = false;
            PlayerData.instance.HasHangMap = false;
            PlayerData.instance.HasHuntersNestMap = false;
            PlayerData.instance.HasJudgeStepsMap = false;
            PlayerData.instance.HasLibraryMap = false;
            PlayerData.instance.HasMossGrottoMap = false;
            PlayerData.instance.HasPeakMap = false;
            PlayerData.instance.HasShellwoodMap = false;
            PlayerData.instance.HasSlabMap = false;
            PlayerData.instance.HasSongGateMap = false;
            PlayerData.instance.HasSwampMap = false;
            PlayerData.instance.HasWardMap = false;
            PlayerData.instance.HasWeavehomeMap = false;
            PlayerData.instance.HasWildsMap = false;

            PlayerData.instance.hasQuill = false;

            DebugMod.LogConsole("Removed all maps");
            return;
        }

        PlayerData.instance.HasAbyssMap = true;
        PlayerData.instance.HasAqueductMap = true;
        PlayerData.instance.HasArboriumMap = true;
        PlayerData.instance.HasBellhartMap = true;
        PlayerData.instance.HasBoneforestMap = true;
        PlayerData.instance.HasCitadelUnderstoreMap = true;
        PlayerData.instance.HasCloverMap = true;
        PlayerData.instance.HasCogMap = true;
        PlayerData.instance.HasCoralMap = true;
        PlayerData.instance.HasCradleMap = true;
        PlayerData.instance.HasCrawlMap = true;
        PlayerData.instance.HasDocksMap = true;
        PlayerData.instance.HasDustpensMap = true;
        PlayerData.instance.HasGreymoorMap = true;
        PlayerData.instance.HasHallsMap = true;
        PlayerData.instance.HasHangMap = true;
        PlayerData.instance.HasHuntersNestMap = true;
        PlayerData.instance.HasJudgeStepsMap = true;
        PlayerData.instance.HasLibraryMap = true;
        PlayerData.instance.HasMossGrottoMap = true;
        PlayerData.instance.HasPeakMap = true;
        PlayerData.instance.HasShellwoodMap = true;
        PlayerData.instance.HasSlabMap = true;
        PlayerData.instance.HasSongGateMap = true;
        PlayerData.instance.HasSwampMap = true;
        PlayerData.instance.HasWardMap = true;
        PlayerData.instance.HasWeavehomeMap = true;
        PlayerData.instance.HasWildsMap = true;

        PlayerData.instance.hasQuill = true;

        foreach (GameMap.ZoneInfo zone in GameManager.instance.gameMap.mapZoneInfo)
        {
            foreach (GameMap.ParentInfo parent in zone.Parents)
            {
                foreach (GameMap.ZoneInfo.MapCache cache in parent.Maps)
                {
                    string sceneName = cache.sceneName;
                    if (!string.IsNullOrEmpty(sceneName))
                    {
                        PlayerData.instance.scenesVisited.Add(sceneName);
                        PlayerData.instance.scenesMapped.Add(sceneName);
                    }
                }
            }
        }

        GameManager.instance.gameMap.SetupMap();

        DebugMod.LogConsole("Unlocked all maps");
    }

    [BindableMethod(name = "Unlock All Fast Travel", category = "Upgrades")]
    public static void UnlockAllFastTravel()
    {
        if (PlayerData.instance.UnlockedAqueductStation
            && PlayerData.instance.UnlockedBelltownStation
            && PlayerData.instance.UnlockedBoneforestEastStation
            && PlayerData.instance.UnlockedCityStation
            && PlayerData.instance.UnlockedCoralTowerStation
            && PlayerData.instance.UnlockedDocksStation
            && PlayerData.instance.UnlockedGreymoorStation
            && PlayerData.instance.UnlockedPeakStation
            && PlayerData.instance.UnlockedShadowStation
            && PlayerData.instance.UnlockedShellwoodStation
            && PlayerData.instance.UnlockedArboriumTube
            && PlayerData.instance.UnlockedCityBellwayTube
            && PlayerData.instance.UnlockedEnclaveTube
            && PlayerData.instance.UnlockedHangTube
            && PlayerData.instance.UnlockedSongTube
            && PlayerData.instance.UnlockedUnderTube)
        {
            PlayerData.instance.UnlockedAqueductStation = false;
            PlayerData.instance.UnlockedBelltownStation = false;
            PlayerData.instance.UnlockedBoneforestEastStation = false;
            PlayerData.instance.UnlockedCityStation = false;
            PlayerData.instance.UnlockedCoralTowerStation = false;
            PlayerData.instance.UnlockedDocksStation = false;
            PlayerData.instance.UnlockedGreymoorStation = false;
            PlayerData.instance.UnlockedPeakStation = false;
            PlayerData.instance.UnlockedShadowStation = false;
            PlayerData.instance.UnlockedShellwoodStation = false;

            PlayerData.instance.UnlockedArboriumTube = false;
            PlayerData.instance.UnlockedCityBellwayTube = false;
            PlayerData.instance.UnlockedEnclaveTube = false;
            PlayerData.instance.UnlockedHangTube = false;
            PlayerData.instance.UnlockedSongTube = false;
            PlayerData.instance.UnlockedUnderTube = false;

            DebugMod.LogConsole("Removed all fast travel");
            return;
        }

        PlayerData.instance.UnlockedAqueductStation = true;
        PlayerData.instance.UnlockedBelltownStation = true;
        PlayerData.instance.UnlockedBoneforestEastStation = true;
        PlayerData.instance.UnlockedCityStation = true;
        PlayerData.instance.UnlockedCoralTowerStation = true;
        PlayerData.instance.UnlockedDocksStation = true;
        PlayerData.instance.UnlockedGreymoorStation = true;
        PlayerData.instance.UnlockedPeakStation = true;
        PlayerData.instance.UnlockedShadowStation = true;
        PlayerData.instance.UnlockedShellwoodStation = true;

        PlayerData.instance.UnlockedArboriumTube = true;
        PlayerData.instance.UnlockedCityBellwayTube = true;
        PlayerData.instance.UnlockedEnclaveTube = true;
        PlayerData.instance.UnlockedHangTube = true;
        PlayerData.instance.UnlockedSongTube = true;
        PlayerData.instance.UnlockedUnderTube = true;

        DebugMod.LogConsole("Unlocked all fast travel");
    }
}