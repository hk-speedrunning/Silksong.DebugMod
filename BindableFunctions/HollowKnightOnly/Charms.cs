namespace DebugMod;

public static partial class BindableFunctions
{
    // TODO(port): charm 36 is given without a royalCharmState
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
                UpdateEquippedCharms();
            }
        }
        pd.CountCharms();
    }

    private const int CharmStateBroken = 1;
    private const int CharmStateUnbreakable = 2;

    internal static int GetCharmState(int id)
    {
        PlayerData pd = PlayerData.instance;
        return id switch
        {
            23 => pd.fragileHealth_unbreakable ? CharmStateUnbreakable : pd.brokenCharm_23 ? CharmStateBroken : 0,
            24 => pd.fragileGreed_unbreakable ? CharmStateUnbreakable : pd.brokenCharm_24 ? CharmStateBroken : 0,
            25 => pd.fragileStrength_unbreakable ? CharmStateUnbreakable : pd.brokenCharm_25 ? CharmStateBroken : 0,
            36 => pd.royalCharmState,
            40 => pd.grimmChildLevel - 1,
            _ => 0,
        };
    }

    private static void SetCharmState(int id, int state)
    {
        PlayerData pd = PlayerData.instance;
        switch (id)
        {
            case 23:
                pd.brokenCharm_23 = state == CharmStateBroken;
                pd.fragileHealth_unbreakable = state == CharmStateUnbreakable;
                break;
            case 24:
                pd.brokenCharm_24 = state == CharmStateBroken;
                pd.fragileGreed_unbreakable = state == CharmStateUnbreakable;
                break;
            case 25:
                pd.brokenCharm_25 = state == CharmStateBroken;
                pd.fragileStrength_unbreakable = state == CharmStateUnbreakable;
                break;
            case 36:
                pd.royalCharmState = state;
                pd.charmCost_36 = state == 4 ? 0 : 5;
                break;
            case 40:
                pd.grimmChildLevel = state + 1;
                pd.charmCost_40 = state == 4 ? 3 : 2;
                break;
        }

        if (pd.GetBool($"equippedCharm_{id}"))
        {
            if (id is 23 or 24 or 25 && state == CharmStateBroken)
            {
                pd.SetBool($"equippedCharm_{id}", false);
                pd.UnequipCharm(id);
            }
            UpdateEquippedCharms();
        }
    }

    internal static void CycleCharm(int id, int stateCount)
    {
        // royalCharmState 0 never happens
        int firstState = id == 36 ? 1 : 0;

        int state = GetCharmState(id);
        if (!PlayerData.instance.GetBool($"gotCharm_{id}"))
        {
            ToggleCharm(id);
            SetCharmState(id, firstState);
        }
        else if (state < stateCount - 1)
        {
            SetCharmState(id, state + 1);
        }
        else
        {
            ToggleCharm(id);
            SetCharmState(id, 0);
        }
    }

    private static void UpdateEquippedCharms()
    {
        PlayerData.instance.CalculateNotchesUsed();
        HeroController.instance.CharmUpdate();
        PlayMakerFSM.BroadcastEvent("CHARM INDICATOR CHECK");
    }
}
