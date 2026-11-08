using System.Linq;

namespace DebugMod;

public static partial class BindableFunctions
{
    private static readonly string[] mapBools =
    [
        nameof(PlayerData.mapAllRooms),
        nameof(PlayerData.mapDirtmouth),
        nameof(PlayerData.mapCrossroads),
        nameof(PlayerData.mapGreenpath),
        nameof(PlayerData.mapFogCanyon),
        nameof(PlayerData.mapRoyalGardens),
        nameof(PlayerData.mapFungalWastes),
        nameof(PlayerData.mapCity),
        nameof(PlayerData.mapWaterways),
        nameof(PlayerData.mapMines),
        nameof(PlayerData.mapDeepnest),
        nameof(PlayerData.mapCliffs),
        nameof(PlayerData.mapOutskirts),
        nameof(PlayerData.mapRestingGrounds),
        nameof(PlayerData.mapAbyss),
    ];

    private static readonly string[] stagBools =
    [
        nameof(PlayerData.openedTown),
        nameof(PlayerData.openedCrossroads),
        nameof(PlayerData.openedGreenpath),
        nameof(PlayerData.openedFungalWastes),
        nameof(PlayerData.openedRoyalGardens),
        nameof(PlayerData.openedRuins1),
        nameof(PlayerData.openedRuins2),
        nameof(PlayerData.openedRestingGrounds),
        nameof(PlayerData.openedDeepnest),
        nameof(PlayerData.openedStagNest),
        nameof(PlayerData.openedHiddenStation),
    ];

    [BindableMethod(name = "WORLD_ALLMAPS", category = "CATEGORY_UPGRADES")]
    public static void UnlockAllMaps()
    {
        PlayerData pd = PlayerData.instance;
        bool unlock = !mapBools.All(pd.GetBool);

        foreach (string mapBool in mapBools)
        {
            pd.SetBool(mapBool, unlock);
        }
        pd.SetBool(nameof(PlayerData.hasMap), unlock);
        pd.SetBool(nameof(PlayerData.hasQuill), unlock);

        GameManager.instance.gameMap.GetComponent<GameMap>().SetupMap();

        DebugMod.LogConsole(unlock ? "Unlocked all maps" : "Removed all maps");
    }

    [BindableMethod(name = "WORLD_ALLFASTTRAVEL", category = "CATEGORY_UPGRADES")]
    public static void UnlockAllFastTravel()
    {
        PlayerData pd = PlayerData.instance;
        bool unlock = !stagBools.All(pd.GetBool);

        foreach (string stagBool in stagBools)
        {
            pd.SetBool(stagBool, unlock);
        }

        DebugMod.LogConsole(unlock ? "Unlocked all stag stations" : "Locked all stag stations");
    }
}
