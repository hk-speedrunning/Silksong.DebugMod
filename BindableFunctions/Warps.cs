namespace DebugMod;

public static partial class BindableFunctions
{
    private static void ToggleWarp(ref bool field, string name)
    {
        field = !field;
        DebugMod.LogConsole(field ? $"Unlocked {name}" : $"Locked {name}");
    }

    [BindableMethod(name = "Unlock All Bellways", category = "Skills")]
    public static void UnlockAllBellways()
    {
        PlayerData.instance.UnlockedDocksStation = true;
        PlayerData.instance.UnlockedBoneforestEastStation = true;
        PlayerData.instance.UnlockedGreymoorStation = true;
        PlayerData.instance.UnlockedBelltownStation = true;
        PlayerData.instance.UnlockedCoralTowerStation = true;
        PlayerData.instance.UnlockedCityStation = true;
        PlayerData.instance.UnlockedPeakStation = true;
        PlayerData.instance.UnlockedShellwoodStation = true;
        PlayerData.instance.UnlockedShadowStation = true;
        PlayerData.instance.UnlockedAqueductStation = true;
        DebugMod.LogConsole("Unlocked all Bellway stations");
    }

    [BindableMethod(name = "Unlock All Ventrica", category = "Skills")]
    public static void UnlockAllVentrica()
    {
        PlayerData.instance.UnlockedSongTube = true;
        PlayerData.instance.UnlockedUnderTube = true;
        PlayerData.instance.UnlockedCityBellwayTube = true;
        PlayerData.instance.UnlockedHangTube = true;
        PlayerData.instance.UnlockedEnclaveTube = true;
        PlayerData.instance.UnlockedArboriumTube = true;
        DebugMod.LogConsole("Unlocked all Ventrica tubes");
    }

    public static void ToggleBellwayDeepDocks() => ToggleWarp(ref PlayerData.instance.UnlockedDocksStation, "Deep Docks Bellway");
    public static void ToggleBellwayFarFields() => ToggleWarp(ref PlayerData.instance.UnlockedBoneforestEastStation, "Far Fields Bellway");
    public static void ToggleBellwayGreymoor() => ToggleWarp(ref PlayerData.instance.UnlockedGreymoorStation, "Greymoor Bellway");
    public static void ToggleBellwayBellhart() => ToggleWarp(ref PlayerData.instance.UnlockedBelltownStation, "Bellhart Bellway");
    public static void ToggleBellwayBlastedSteps() => ToggleWarp(ref PlayerData.instance.UnlockedCoralTowerStation, "Blasted Steps Bellway");
    public static void ToggleBellwayGrandBellway() => ToggleWarp(ref PlayerData.instance.UnlockedCityStation, "Grand Bellway");
    public static void ToggleBellwayTheSlab() => ToggleWarp(ref PlayerData.instance.UnlockedPeakStation, "The Slab Bellway");
    public static void ToggleBellwayShellwood() => ToggleWarp(ref PlayerData.instance.UnlockedShellwoodStation, "Shellwood Bellway");
    public static void ToggleBellwayBilewater() => ToggleWarp(ref PlayerData.instance.UnlockedShadowStation, "Bilewater Bellway");
    public static void ToggleBellwayPutrifiedDucts() => ToggleWarp(ref PlayerData.instance.UnlockedAqueductStation, "Putrified Ducts Bellway");

    public static void ToggleVentricaChoralChambers() => ToggleWarp(ref PlayerData.instance.UnlockedSongTube, "Choral Chambers Ventrica");
    public static void ToggleVentricaUnderworks() => ToggleWarp(ref PlayerData.instance.UnlockedUnderTube, "Underworks Ventrica");
    public static void ToggleVentricaGrandBellway() => ToggleWarp(ref PlayerData.instance.UnlockedCityBellwayTube, "Grand Bellway Ventrica");
    public static void ToggleVentricaHighHalls() => ToggleWarp(ref PlayerData.instance.UnlockedHangTube, "High Halls Ventrica");
    public static void ToggleVentricaFirstShrine() => ToggleWarp(ref PlayerData.instance.UnlockedEnclaveTube, "First Shrine Ventrica");
    public static void ToggleVentricaMemoriam() => ToggleWarp(ref PlayerData.instance.UnlockedArboriumTube, "Memorium Ventrica");
}
