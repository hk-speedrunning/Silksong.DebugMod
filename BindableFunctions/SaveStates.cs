using DebugMod.SaveStates;
using DebugMod.UI;

namespace DebugMod;

public static partial class BindableFunctions
{
    [BindableMethod(name = "ACTION_QUICKSLOTSAVE", category = "CATEGORY_SAVESTATES")]
    public static void SaveState()
    {
        SaveStateManager.SetQuickState(SaveStateManager.SaveNewState());
    }

    [BindableMethod(name = "ACTION_QUICKSLOTLOAD", category = "CATEGORY_SAVESTATES")]
    public static void LoadState()
    {
        SaveStateManager.LoadState(SaveStateManager.GetQuickState());
    }

    //TODO: Allow these binds to override each other properly
    [BindableMethod(name = "ACTION_QUICKSLOTTOFILE", category = "CATEGORY_SAVESTATES")]
    public static void CurrentSaveStateToFile()
    {
        SaveStatesPanel.Instance.EnterSelectState(SelectOperation.QuickslotToFile);
    }

    [BindableMethod(name = "ACTION_FILETOQUICKSLOT", category = "CATEGORY_SAVESTATES")]
    public static void CurrentSlotToSaveMemory()
    {
        SaveStatesPanel.Instance.EnterSelectState(SelectOperation.FileToQuickslot);
    }

    [BindableMethod(name = "ACTION_SAVETOFILE", category = "CATEGORY_SAVESTATES")]
    public static void NewSaveStateToFile()
    {
        SaveStatesPanel.Instance.EnterSelectState(SelectOperation.SaveToFile);
    }

    [BindableMethod(name = "ACTION_LOADFROMFILE", category = "CATEGORY_SAVESTATES")]
    public static void LoadFromFile()
    {
        SaveStatesPanel.Instance.EnterSelectState(SelectOperation.LoadFromFile);

    }

    [BindableMethod(name = "ACTION_NEXTSAVESTATEPAGE", category = "CATEGORY_SAVESTATES")]
    public static void NextStatePage()
    {
        SaveStatesPanel.Instance.NextPage();
    }

    [BindableMethod(name = "ACTION_PREVSAVESTATEPAGE", category = "CATEGORY_SAVESTATES")]
    public static void PrevStatePage()
    {
        SaveStatesPanel.Instance.PrevPage();
    }

    [BindableMethod(name = "ACTION_REFRESHFILESLOTS", category = "CATEGORY_SAVESTATES")]
    public static void RefreshFileSlots()
    {
        SaveStateManager.LoadFileStates();
        DebugMod.LogConsole("Reimporting file slots from disk");
    }

    [BindableMethod(name = "ACTION_SAVESTATEONDEATH", category = "CATEGORY_SAVESTATES")]
    public static void LoadStateOnDeath()
    {
        DebugMod.stateOnDeath = !DebugMod.stateOnDeath;
        DebugMod.LogConsole("Quickslot savestate will now" + (DebugMod.stateOnDeath ? " be" : " no longer") + " loaded on death");
    }

    [BindableMethod(name = "ACTION_OVERRIDELOADLOCKOUT", category = "CATEGORY_SAVESTATES")]
    public static void OverrideLoadLockout()
    {
        DebugMod.overrideLoadLockout = !DebugMod.overrideLoadLockout;
        DebugMod.LogConsole("Savestate lockout override set to " + DebugMod.overrideLoadLockout.ToString().ToUpper());
    }
}