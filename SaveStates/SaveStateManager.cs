using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using static DebugMod.SaveStates.SaveState;

namespace DebugMod.SaveStates;

public static class SaveStateManager
{
    public const int STATES_PER_PAGE = 10;

    private static readonly string pageDirectoryPattern = @"^(\d+)$";
    private static readonly string savestateFilePattern = @"^savestate(\d)\.json$";
    private static readonly string defaultPackName = "My Savestates";
    private static readonly string extraFilesPrefix = "notasavestatepack-";

    private static readonly string saveStatesBaseDirectory = Path.Combine(DebugMod.ModBaseDirectory, "Savestates 1.0");
    private static readonly string defaultPackDirectory = Path.Combine(saveStatesBaseDirectory, defaultPackName);

    public static int NumPages => fileStates.Count;

    public static string CurrentPack
    {
        get
        {
            return DebugMod.settings.CurrentSavestatePack;
        }

        private set
        {
            DebugMod.settings.CurrentSavestatePack = value;
        }
    }

    public static event Action PackChanged;

    private static List<SaveState[]> fileStates = [];
    private static SaveState quickState;

    private static List<string> packNames = [];
    private static string lastPackName;

    internal static void Initialize()
    {
        quickState = new SaveState();
        LoadSavestateFiles();
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
            string filePath = Path.Combine(saveStatesBaseDirectory, CurrentPack, page.ToString(), $"savestate{index}.json");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            data.BeforeSerialize();
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
        bool shouldLoad = LoadLockout();
        if (!shouldLoad && DebugMod.overrideLoadLockout)
        {
            DebugMod.LogConsole($"Overriding savestate lockout");
            shouldLoad = true;
        }

        if (state.IsSet() && shouldLoad)
        {
            GameManager.instance.StartCoroutine(state.Load());
        }
    }

    private static bool LoadLockout()
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
            DebugMod.LogConsole("Cannot load a savestate while another savestate is loading");
            return false;
        }

        return true;
    }

    public static void LoadSavestateFiles()
    {
        try
        {
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

            packNames.Clear();

            // Detect old savestate file layout and move into a default pack
            if (!Directory.Exists(defaultPackDirectory))
            {
                foreach (string path in Directory.EnumerateDirectories(saveStatesBaseDirectory))
                {
                    string name = Path.GetFileName(path);

                    if (Regex.IsMatch(name, pageDirectoryPattern))
                    {
                        Directory.CreateDirectory(defaultPackDirectory);
                        Directory.Move(path, Path.Combine(defaultPackDirectory, name));
                    }
                }
            }

            foreach (string path in Directory.EnumerateFileSystemEntries(saveStatesBaseDirectory).OrderBy(x => x))
            {
                if (Directory.Exists(path)
                    && Directory.EnumerateDirectories(path).Any(pagePath => Regex.IsMatch(Path.GetFileName(pagePath), pageDirectoryPattern)
                    && Directory.EnumerateFiles(pagePath).Any(savestatePath => Regex.IsMatch(Path.GetFileName(savestatePath), savestateFilePattern))))
                {
                    packNames.Add(Path.GetFileName(path));
                }
                else
                {
                    // Packs directory needs to stay clear or pack operations can break

                    try
                    {
                        // Just delete a directory if it's empty
                        Directory.Delete(path);
                        continue;
                    }
                    catch { }

                    try
                    {
                        // Rename it to a safe name otherwise
                        string newPath = Path.Combine(DebugMod.ModBaseDirectory,
                            extraFilesPrefix + Path.GetFileName(path));

                        if (Directory.Exists(path))
                        {
                            Directory.Move(path, newPath);
                        }
                        else
                        {
                            File.Move(path, newPath);
                        }
                    }
                    catch { }

                    try
                    {
                        // Well, I tried being nice...
                        if (Directory.Exists(path))
                        {
                            Directory.Delete(path, recursive: true);
                        }
                        else
                        {
                            File.Delete(path);
                        }
                    }
                    catch { }
                }
            }

            if (CurrentPack == "" || !packNames.Contains(CurrentPack))
            {
                CurrentPack = defaultPackName;
                Directory.CreateDirectory(defaultPackDirectory);
            }

            LoadPack(CurrentPack);
        }
        catch (Exception ex)
        {
            DebugMod.LogError(ex.ToString());
        }
    }

    private static void LoadPack(string name)
    {
        CurrentPack = name;
        fileStates.Clear();

        string baseDirectory = Path.Combine(saveStatesBaseDirectory, name);

        foreach (string pageDirectory in Directory.EnumerateDirectories(baseDirectory).OrderBy(x => x))
        {
            Match pageMatch = Regex.Match(Path.GetFileName(pageDirectory), pageDirectoryPattern);
            if (!pageMatch.Success)
            {
                continue;
            }

            int page = int.Parse(pageMatch.Groups[1].Value);

            while (fileStates.Count <= page)
            {
                AddFileSlotPage();
            }

            foreach (string savestateFile in Directory.EnumerateFiles(pageDirectory).OrderBy(x => x))
            {
                Match savestateMatch = Regex.Match(Path.GetFileName(savestateFile), savestateFilePattern);
                if (!savestateMatch.Success)
                {
                    continue;
                }

                int index = int.Parse(savestateMatch.Groups[1].Value);

                if (index < STATES_PER_PAGE)
                {
                    fileStates[page][index].data = LoadFromFile(savestateFile);
                }
            }
        }

        if (fileStates.Count == 0)
        {
            AddFileSlotPage();
        }

        PackChanged?.Invoke();
    }

    private static void AddFileSlotPage()
    {
        SaveState[] array = new SaveState[STATES_PER_PAGE];

        for (int i = 0; i < array.Length; i++)
        {
            array[i] = new SaveState();
        }

        fileStates.Add(array);
    }

    private static SaveStateData LoadFromFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                SaveStateData data = JsonUtility.FromJson<SaveStateData>(File.ReadAllText(path));
                data.AfterDeserialize();
                return data;
            }
        }
        catch (Exception ex)
        {
            DebugMod.LogError(ex.Message);
        }

        return new SaveStateData();
    }
    #endregion

    #region packs
    public static List<string> GetPackNames() => packNames;

    public static void SwitchPack(string name)
    {
        try
        {
            lastPackName = CurrentPack;
            LoadPack(name);
        }
        catch (Exception ex)
        {
            DebugMod.LogError(ex.ToString());
        }
    }

    public static void RenameCurrentPack(string newName)
    {
        if (ValidateNewPackName(newName) != "")
        {
            return;
        }

        string currentDirectory = GetPackDirectory(CurrentPack);
        string newDirectory = GetPackDirectory(newName);

        if (Directory.Exists(currentDirectory))
        {
            Directory.Move(currentDirectory, newDirectory);
        }

        for (int i = 0; i < packNames.Count; i++)
        {
            if (packNames[i] == CurrentPack)
            {
                packNames[i] = newName;
            }
        }

        CurrentPack = newName;
    }

    public static void CreateNewPack(string name)
    {
        if (ValidateNewPackName(name) != "")
        {
            return;
        }

        fileStates.Clear();
        AddFileSlotPage();

        packNames.Add(name);
        packNames.Sort();
        CurrentPack = name;
    }

    public static void DeleteCurrentPack()
    {
        if (!packNames.Remove(CurrentPack))
        {
            return;
        }

        string directory = GetPackDirectory(CurrentPack);

        if (Directory.Exists(directory))
        {
            Directory.Delete(Path.Combine(saveStatesBaseDirectory, CurrentPack), recursive: true);
        }

        if (packNames.Count == 0)
        {
            CreateNewPack(defaultPackName);
        }
        else
        {
            SwitchPack(packNames.Contains(lastPackName) ? lastPackName : packNames[0]);
        }

        lastPackName = null;
    }

    public static string GetPackDirectory(string name)
    {
        return Path.Combine(saveStatesBaseDirectory, name);
    }

    public static string ValidateNewPackName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return "SAVESTATES_ERRORPACKNAMEEMPTY";
        }

        if (name.StartsWith(extraFilesPrefix))
        {
            return "SAVESTATES_ERRORGENERIC";
        }

        if (packNames.Contains(name))
        {
            return "SAVESTATES_ERRORPACKALREADYEXISTS";
        }

        foreach (char c in Path.GetInvalidFileNameChars())
        {
            if (name.Contains(c))
            {
                return "SAVESTATES_ERRORINVALIDPACKNAMECHAR";
            }
        }

        return "";
    }
    #endregion
}