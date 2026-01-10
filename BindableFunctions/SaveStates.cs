using DebugMod.SaveStates;
using DebugMod.UI;

namespace DebugMod;

public static partial class BindableFunctions
{
    [BindableMethod(name = "Quickslot (save)", category = "Savestates")]
    public static void SaveState()
    {
        SaveStateManager.SetQuickState(SaveStateManager.SaveNewState());
    }

    [BindableMethod(name = "Quickslot (load)", category = "Savestates")]
    public static void LoadState()
    {
        SaveStateManager.LoadState(SaveStateManager.GetQuickState());
    }

    //TODO: Allow these binds to override each other properly
    [BindableMethod(name = "Quickslot save to file", category = "Savestates")]
    public static void CurrentSaveStateToFile()
    {
        SaveStatesPanel.Instance.EnterSelectState(SelectOperation.QuickslotToFile);
    }

    [BindableMethod(name = "Load file to quickslot", category = "Savestates")]
    public static void CurrentSlotToSaveMemory()
    {
        SaveStatesPanel.Instance.EnterSelectState(SelectOperation.FileToQuickslot);
    }

    [BindableMethod(name = "Save new state to file", category = "Savestates")]
    public static void NewSaveStateToFile()
    {
        SaveStatesPanel.Instance.EnterSelectState(SelectOperation.SaveToFile);
    }

    [BindableMethod(name = "Load new state from file", category = "Savestates")]
    public static void LoadFromFile()
    {
        SaveStatesPanel.Instance.EnterSelectState(SelectOperation.LoadFromFile);

    }

    [BindableMethod(name = "Next Save Page", category = "Savestates")]
    public static void NextStatePage()
    {
        SaveStatesPanel.Instance.NextPage();
    }

    [BindableMethod(name = "Prev Save Page", category = "Savestates")]
    public static void PrevStatePage()
    {
        SaveStatesPanel.Instance.PrevPage();
    }

    [BindableMethod(name = "Load Quickslot On Death", category = "Savestates")]
    public static void LoadStateOnDeath()
    {
        DebugMod.stateOnDeath = !DebugMod.stateOnDeath;
        DebugMod.LogConsole("Quickslot savestate will now" + (DebugMod.stateOnDeath ? " be" : " no longer") + " loaded on death");
    }

    [BindableMethod(name = "Override Lockout", category = "Savestates")]
    public static void OverrideLoadLockout()
    {
        DebugMod.overrideLoadLockout = !DebugMod.overrideLoadLockout;
        DebugMod.LogConsole("Savestate lockout override set to " + DebugMod.overrideLoadLockout);
    }
}