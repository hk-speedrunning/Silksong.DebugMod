using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using static DebugMod.SaveStates.SaveState;

namespace DebugMod.SaveStates;

public static class SaveStateManager
{
    public const int STATES_PER_PAGE = 10;

    public static int NumPages => DebugMod.settings.MaxSavePages;

    private static readonly string saveStatesBaseDirectory = Path.Combine(DebugMod.ModBaseDirectory, "Savestates 1.0");

    private static readonly Dictionary<int, SaveState[]> fileStates = new();
    private static SaveState quickState;

    internal static void Initialize()
    {
        quickState = new SaveState();

        for (int i = 0; i < NumPages; i++)
        {
            fileStates.Add(i, new SaveState[STATES_PER_PAGE]);
            for (int j = 0; j < STATES_PER_PAGE; j++)
            {
                fileStates[i][j] = new SaveState();
            }
        }

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

        LoadFileStates();
    }

    public static SaveState GetQuickState() => quickState;
    public static SaveState GetFileState(int page, int index) => fileStates[page][index];

    public static void SetQuickState(SaveState state)
    {
        if (state.IsSet())
        {
            quickState.data = state.data.DeepCopy();
        }
    }

    public static void SetFileState(SaveState state, int page, int index)
    {
        if (state.IsSet())
        {
            fileStates[page][index].data = state.data.DeepCopy();
            SaveToFile(state.data, page, index);
        }
    }

    private static string GetFilePath(int page, int index)
    {
        return Path.Combine(saveStatesBaseDirectory, page.ToString(), $"savestate{index}.json");
    }

    #region saving
    public static SaveState SaveNewState()
    {
        if (SaveTest())
        {
            SaveState state = new();
            state.Save();
            return state;
        }

        return new SaveState();
    }

    private static bool SaveTest()
    {
        if (loadingSavestate != null)
        {
            if (DebugMod.overrideLoadLockout)
            {
                DebugMod.LogConsole("Overriding savestate lockout");
            }
            else
            {
                DebugMod.LogConsole("Cannot save a savestate while another savestate is loading");
                return false;
            }
        }

        return true;
    }

    private static void SaveToFile(SaveStateData data, int page, int index)
    {
        try
        {
            string filePath = GetFilePath(page, index);
            File.WriteAllText(filePath, JsonUtility.ToJson(data, prettyPrint: true));
        }
        catch (Exception ex)
        {
            DebugMod.LogDebug(ex.Message);
            throw;
        }
    }

    public static void RenameFileState(int page, int index, string name)
    {
        if (fileStates[page][index].IsSet())
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                fileStates[page][index].data.saveStateIdentifier = name;
                SaveToFile(fileStates[page][index].data, page, index);
            }
            else
            {
                DebugMod.LogConsole("Invalid name for savestate");
            }
        }
    }
    #endregion

    #region loading
    public static void LoadState(SaveState state)
    {
        if (state.IsSet() && LoadTest())
        {
            GameManager.instance.StartCoroutine(state.Load());
        }
    }

    private static bool LoadTest()
    {
        if (PlayerDeathWatcher.playerDead)
        {
            DebugMod.LogConsole("Savestates cannot be loaded when dead");
            return false;
        }

        if (HeroController.instance.cState.transitioning)
        {
            DebugMod.LogConsole("Savestates cannot be loaded when transitioning");
            return false;
        }

        if (loadingSavestate != null)
        {
            if (DebugMod.overrideLoadLockout)
            {
                DebugMod.LogConsole("Overriding savestate lockout");
            }
            else
            {
                DebugMod.LogConsole("Cannot load a savestate while another savestate is loading");
                return false;
            }
        }

        return true;
    }

    private static void LoadFileStates()
    {
        try
        {
            for (int page = 0; page < NumPages; page++)
            {
                string pageDirectory = Path.Combine(saveStatesBaseDirectory, page.ToString());
                if (!Directory.Exists(pageDirectory))
                {
                    Directory.CreateDirectory(pageDirectory);
                    continue;
                }

                foreach (string path in Directory.GetFiles(pageDirectory))
                {
                    try
                    {
                        string fileName = Path.GetFileName(path);
                        int index = int.Parse(Regex.Match(fileName, @"^savestate(\d+).json$").Groups[1].Value);

                        if (index >= 0 && index < STATES_PER_PAGE)
                        {
                            fileStates[page][index].data = LoadFromFile(page, index);
                        }
                    }
                    catch (Exception ex)
                    {
                        DebugMod.LogError(ex.Message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            DebugMod.LogError(ex.Message);
            throw;
        }
    }

    private static SaveStateData LoadFromFile(int page, int index)
    {
        try
        {
            string filePath = GetFilePath(page, index);
            if (File.Exists(filePath))
            {
                return JsonUtility.FromJson<SaveStateData>(File.ReadAllText(filePath));
            }
        }
        catch (Exception ex)
        {
            DebugMod.LogDebug(ex.Message);
            throw;
        }

        return new SaveStateData();
    }
    #endregion
}