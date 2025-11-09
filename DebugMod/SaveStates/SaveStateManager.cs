using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace DebugMod.SaveStates;

public enum SaveStateType
{
    Memory,
    File,
    SkipOne
}

// Handles organisation of SaveState-s
// quickState replicating legacy behaviour of only stored in RAM.
// Dictionary(int slot : file). Might change to HashMap(?) 
//  if: memory requirement too high: array for limiting savestates? hashmap as all states should logically be unique?
// HUD for viewing necessary info for UX.
// AutoSlotSelect to iterate over slots, eventually overwrite when circled and no free slots.
internal class SaveStateManager
{
    public static int maxSaveStates = DebugMod.settings.MaxSaveStates;
    public static int savePages = DebugMod.settings.MaxSavePages;
    public static int currentStateFolder = 0;
    public static SaveState quickState;
    public static bool inSelectSlotState = false;   // a mutex, in practice?
    public static int currentStateSlot = -1;
    public static readonly string saveStatesBaseDirectory = Path.Combine(DebugMod.ModBaseDirectory, "Savestates 1.0");
    public static string path = Path.Combine(saveStatesBaseDirectory, "0"); // initialize to page 0, this gets read and updated by callbacks during runtime.
    public static string currentStateOperation = null;

    private static string[] stateStrings =
    {
        "Quickslot (save)",
        "Quickslot (load)",
        "Save quickslot to file",
        "Load quickslot from file",
        "Save new state to file",
        "Load new state from file"
    };
    private static Dictionary<int, SaveState> saveStateFiles = new Dictionary<int, SaveState>();

    //private static bool autoSlot;

    //public static bool preserveThroughStates = false;

    internal SaveStateManager()
    {
        inSelectSlotState = false;
        //autoSlot = false;
        quickState = new SaveState();

        if (!Directory.Exists(saveStatesBaseDirectory))
        {
            string legacyPath = Path.Combine(DebugMod.ModBaseDirectory, "Savestates Current Patch");
            if (Directory.Exists(legacyPath))
            {
                DebugMod.LogWarn("Legacy savestates directory detected. Renaming...");
                Directory.Move(legacyPath, saveStatesBaseDirectory);
            }
            else
            {
                Directory.CreateDirectory(saveStatesBaseDirectory);
            }
        }

        for (int i = 0; i < savePages; i++)
        {
            string saveStatePageDirectory = Path.Combine(saveStatesBaseDirectory, i.ToString());
            if (!Directory.Exists(saveStatePageDirectory))
            {
                Directory.CreateDirectory(saveStatePageDirectory);
            }
            else
            {
                RefreshStateMenu();
            }
        }
    }

    #region saving
    public void SaveSaveState(SaveStateType stateType)
    {
        if (SaveState.loadingSavestate == null)
        {
            switch (stateType)
            {
                case SaveStateType.Memory:
                    quickState.SaveTempState();
                    break;
                case SaveStateType.File or SaveStateType.SkipOne:
                    if (!inSelectSlotState)
                    {
                        RefreshStateMenu();
                        GameManager.instance.StartCoroutine(SelectSlot(true, stateType));
                    }
                    break;
            }
        }
        else if (DebugMod.overrideLoadLockout)
        {
            DebugMod.LogConsole("Attempting Savestate Load Override");
            switch (stateType)
            {
                case SaveStateType.Memory:
                    quickState.SaveTempState();
                    break;
                case SaveStateType.File or SaveStateType.SkipOne:
                    if (!inSelectSlotState)
                    {
                        RefreshStateMenu();
                        GameManager.instance.StartCoroutine(SelectSlot(true, stateType));
                    }
                    break;
            }
        }
        else
        {
            DebugMod.LogConsole("Cannot save new states while loading");
        }
    }

    #endregion

    #region loading

    //loadDuped is used by external mods
    public void LoadSaveState(SaveStateType stateType, bool loadDuped = false, string operationName = null)
    {
        switch (stateType)
        {
            case SaveStateType.Memory:
                if (quickState.IsSet())
                {
                    quickState.LoadTempState(loadDuped);
                }
                else
                {
                    DebugMod.LogConsole("No save state active");
                }
                break;
            case SaveStateType.File or SaveStateType.SkipOne:
                if (!inSelectSlotState)
                {
                    RefreshStateMenu();
                    GameManager.instance.StartCoroutine(SelectSlot(false, stateType, loadDuped, operationName));
                }
                break;
            default:
                break;
        }
    }

    #endregion

    #region helper functionality
    private IEnumerator SelectSlot(bool save, SaveStateType stateType, bool loadDuped = false, string operationName = null)
    {
        if (operationName == null)
        {
            switch (stateType)
            {
                case SaveStateType.Memory:
                    currentStateOperation = save ? "Quickslot (save)" : "Quickslot (load)";
                    break;
                case SaveStateType.File:
                    currentStateOperation = save ? "Quickslot save to file" : "Load file to quickslot";
                    break;
                case SaveStateType.SkipOne:
                    currentStateOperation = save ? "Save new state to file" : "Load new state from file";
                    break;
                default:
                    //DebugMod.LogError("SelectSlot ended started");
                    throw new ArgumentException(
                        "Helper func SelectSlot requires `bool` and `SaveStateType` to proceed the savestate process");
            }
        }
        else
        {
            currentStateOperation = operationName;
        }
        
        yield return null;

        inSelectSlotState = true;
        yield return new WaitUntil(DidInput);
        
        if (GUIController.didInput && !GUIController.inputEsc)
        {
            if (currentStateSlot >= 0 && currentStateSlot < maxSaveStates)
            {
                if (save)
                {
                    SaveCoroHelper(stateType);
                }
                else
                {
                    LoadCoroHelper(stateType, loadDuped);
                }
            }
        }
        else
        {
            if (GUIController.didInput) DebugMod.LogConsole("Savestate action cancelled");
        }
        
        currentStateOperation = null;
        GUIController.inputEsc = GUIController.didInput = false;
        inSelectSlotState = false;
    }

    // Todo: cleanup Adds and Removes, because used to C++ :)
    private void SaveCoroHelper(SaveStateType stateType)
    {
        switch (stateType)
        {
            case SaveStateType.File:
                if (quickState == null || !quickState.IsSet())
                {
                    if (!quickState.SaveTempState()) break;
                }
                if (saveStateFiles.ContainsKey(currentStateSlot))
                {
                    saveStateFiles.Remove(currentStateSlot);
                }
                saveStateFiles.Add(currentStateSlot, new SaveState());
                saveStateFiles[currentStateSlot].data = quickState.DeepCopy();
                saveStateFiles[currentStateSlot].SaveStateToFile(currentStateSlot);
                break;
            case SaveStateType.SkipOne:
                if (saveStateFiles.ContainsKey(currentStateSlot))
                {
                    saveStateFiles.Remove(currentStateSlot);
                }
                saveStateFiles.Add(currentStateSlot, new SaveState());
                saveStateFiles[currentStateSlot].NewSaveStateToFile(currentStateSlot);
                break;
            default:
                break;
        }
    }

    //loadDuped is used by external mods
    private void LoadCoroHelper(SaveStateType stateType, bool loadDuped)
    {
        switch (stateType)
        {
            case SaveStateType.File:
                if (saveStateFiles.ContainsKey(currentStateSlot))
                {
                    saveStateFiles.Remove(currentStateSlot);
                }
                saveStateFiles.Add(currentStateSlot, new SaveState());
                saveStateFiles[currentStateSlot].LoadStateFromFile(currentStateSlot);
                quickState.data = saveStateFiles[currentStateSlot].DeepCopy();
                break;
            case SaveStateType.SkipOne:
                if (saveStateFiles.ContainsKey(currentStateSlot))
                {
                    saveStateFiles.Remove(currentStateSlot);
                }
                saveStateFiles.Add(currentStateSlot, new SaveState());
                saveStateFiles[currentStateSlot].NewLoadStateFromFile(loadDuped);
                break;
            default:
                break;
        }
    }

    private bool DidInput()
    {
        if (GUIController.didInput)
        {
            return true;
        }
        else if (!inSelectSlotState)
        {
            return true;
        }
        return false;
    }

    /*
    public void ToggleAutoSlot()
    {
        autoSlot = !autoSlot;
    }
    public static bool GetAutoSlot()
    {
        return autoSlot;
    }
    */

    public static int GetCurrentSlot()
    {
        return currentStateSlot;
    }

    public string[] GetCurrentMemoryState()
    {
        if (quickState.IsSet())
        {
            return quickState.GetSaveStateInfo();
        }
        return null;
    }

    public static bool HasFiles()
    {
        return (saveStateFiles.Count != 0);
    }

    public static Dictionary<int, string[]> GetSaveStatesInfo()
    {
        Dictionary<int, string[]> returnData = new Dictionary<int, string[]>();
        if (HasFiles())
        {
            int total = 0;
            foreach (KeyValuePair<int, SaveState> stateData in saveStateFiles)
            {
                if (stateData.Value.IsSet()
                    && stateData.Key < DebugMod.settings.MaxSaveStates
                    && stateData.Key >= 0
                    && total < DebugMod.settings.MaxSaveStates)
                {
                    returnData.Add(stateData.Key, stateData.Value.GetSaveStateInfo());
                    ++total;
                }
            }
        }
        return returnData;
    }

    public void RefreshStateMenu()
    {
        try
        {
            saveStateFiles.Clear();
            string shortFileName;
            string[] files = Directory.GetFiles(path);

            foreach (string file in files)
            {
                shortFileName = Path.GetFileName(file);
                //DebugMod.Log("file: " + shortFileName);
                var digits = shortFileName.SkipWhile(c => !Char.IsDigit(c)).TakeWhile(Char.IsDigit).ToArray();
                int slot = int.Parse(new string(digits));

                if (File.Exists(file) && (slot >= 0 || slot < maxSaveStates))
                {
                    if (saveStateFiles.ContainsKey(slot))
                    {
                        saveStateFiles.Remove(slot);
                    }
                    saveStateFiles.Add(slot, new SaveState());
                    saveStateFiles[slot].LoadStateFromFile(slot);

                    //DebugMod.LogError(saveStateFiles[slot].GetSaveStateID());
                }
            }
        }
        catch (Exception ex)
        {
            DebugMod.LogError(ex.Message);
            //throw ex;
        }
    }

    public static void RenameSaveState(int index)
    {
        GUIController.Instance.TextBox(saveStateFiles[index].data.saveStateIdentifier, name =>
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                saveStateFiles[index].data.saveStateIdentifier = name;
                saveStateFiles[index].SaveStateToFile(index);
            }
            else
            {
                DebugMod.LogConsole("Invalid name for savestate");
            }
        });
    }

    #endregion
}