using DebugMod.SaveStates;
using DebugMod.UI;

namespace DebugMod;

public static partial class BindableFunctions
{
    [BindableMethod(name = "SAVESTATES_QUICKSLOTSAVE", category = "CATEGORY_SAVESTATES")]
    public static void SaveState()
    {
        SaveStateManager.SetQuickState(SaveStateManager.SaveNewState());
    }

    [BindableMethod(name = "SAVESTATES_QUICKSLOTLOAD", category = "CATEGORY_SAVESTATES")]
    public static void LoadState()
    {
        SaveStateManager.LoadState(SaveStateManager.GetQuickState());
    }

    //TODO: Allow these binds to override each other properly
    [BindableMethod(name = "SAVESTATES_QUICKSLOTTOFILE", category = "CATEGORY_SAVESTATES")]
    public static void CurrentSaveStateToFile()
    {
        SaveStatesPanel.Instance.EnterSelectState(SelectOperation.QuickslotToFile);
    }

    [BindableMethod(name = "SAVESTATES_FILETOQUICKSLOT", category = "CATEGORY_SAVESTATES")]
    public static void CurrentSlotToSaveMemory()
    {
        SaveStatesPanel.Instance.EnterSelectState(SelectOperation.FileToQuickslot);
    }

    [BindableMethod(name = "SAVESTATES_SAVETOFILE", category = "CATEGORY_SAVESTATES")]
    public static void NewSaveStateToFile()
    {
        SaveStatesPanel.Instance.EnterSelectState(SelectOperation.SaveToFile);
    }

    [BindableMethod(name = "SAVESTATES_LOADFROMFILE", category = "CATEGORY_SAVESTATES")]
    public static void LoadFromFile()
    {
        SaveStatesPanel.Instance.EnterSelectState(SelectOperation.LoadFromFile);

    }

    [BindableMethod(name = "SAVESTATES_NEXTSAVESTATEPAGE", category = "CATEGORY_SAVESTATES")]
    public static void NextStatePage()
    {
        SaveStatesPanel.Instance.NextPage();
    }

    [BindableMethod(name = "SAVESTATES_PREVSAVESTATEPAGE", category = "CATEGORY_SAVESTATES")]
    public static void PrevStatePage()
    {
        SaveStatesPanel.Instance.PrevPage();
    }

    [BindableMethod(name = "SAVESTATES_REFRESHFILESLOTS", category = "CATEGORY_SAVESTATES")]
    public static void RefreshFileSlots()
    {
        SaveStateManager.LoadFileStates();
        DebugMod.LogConsole("Reimporting file slots from disk");
    }

    [BindableMethod(name = "SAVESTATES_SAVESTATEONDEATH", category = "CATEGORY_SAVESTATES")]
    public static void LoadStateOnDeath()
    {
        DebugMod.stateOnDeath = !DebugMod.stateOnDeath;
        DebugMod.LogConsole("Quickslot savestate will now" + (DebugMod.stateOnDeath ? " be" : " no longer") + " loaded on death");
    }

    [BindableMethod(name = "SAVESTATES_OVERRIDELOADLOCKOUT", category = "CATEGORY_SAVESTATES")]
    public static void OverrideLoadLockout()
    {
        DebugMod.overrideLoadLockout = !DebugMod.overrideLoadLockout;
        DebugMod.LogConsole("Savestate lockout override set to " + DebugMod.overrideLoadLockout.ToString().ToUpper());
    }
}