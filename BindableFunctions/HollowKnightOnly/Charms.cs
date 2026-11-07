namespace DebugMod;

public static partial class BindableFunctions
{
    // TODO(port): Kingsoul/Void Heart, Grimmchild/Carefree Melody and the unbreakable charms have extra state
    [BindableMethod(name = "HK_CHARMS_GIVEALLCHARMS", category = "HK_CATEGORY_CHARMS")]
    public static void GiveAllCharms()
    {
        for (int id = 1; id <= 40; id++)
        {
            PlayerData.instance.SetBool($"gotCharm_{id}", true);
        }
        PlayerData.instance.hasCharm = true;
        PlayerData.instance.CountCharms();

        DebugMod.LogConsole("Giving player all charms");
    }

    internal static void ToggleCharm(int id)
    {
        PlayerData pd = PlayerData.instance;
        if (!pd.GetBool($"gotCharm_{id}"))
        {
            pd.SetBool($"gotCharm_{id}", true);
            pd.hasCharm = true;
        }
        else
        {
            pd.SetBool($"gotCharm_{id}", false);
            if (pd.GetBool($"equippedCharm_{id}"))
            {
                pd.SetBool($"equippedCharm_{id}", false);
                pd.UnequipCharm(id);
                pd.CalculateNotchesUsed();
                HeroController.instance.CharmUpdate();
                PlayMakerFSM.BroadcastEvent("CHARM INDICATOR CHECK");
            }
        }
        pd.CountCharms();
    }
}
