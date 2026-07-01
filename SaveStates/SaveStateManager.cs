using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

    public static readonly string saveStatesBaseDirectory = Path.Combine(DebugMod.ModBaseDirectory, "Savestates 1.0");
    public static readonly string packsBaseDirectory = Path.Combine(DebugMod.ModBaseDirectory, "Savestate Packs");
    public static readonly string backupsDirectory = Path.Combine(DebugMod.ModBaseDirectory, "Savestate Backups");

    public static int NumPages => fileStates.Count;

    public static event Action PackChanged;

    private static readonly List<SaveState[]> fileStates = [];
    private static SaveState quickState;

    private static readonly List<string> packNames = [];

    internal static void Initialize()
    {
        quickState = new SaveState();
        LoadSavestateFiles();
    }

    #region operations

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
            SetFileStateForce(state, page, index);
        }
    }

    public static void DeleteFileState(int page, int index)
    {
        fileStates[page][index].data = new SaveStateData();
        SaveToFile(page, index);
    }

    public static void SetFileStateForce(SaveState state, int page, int index)
    {
        fileStates[page][index].data = state.data.DeepCopy();
        SaveToFile(page, index);
    }

    public static void RenameFileState(int page, int index, string name)
    {
        if (fileStates[page][index].IsSet())
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                fileStates[page][index].data.saveStateIdentifier = name;
                SaveToFile(page, index);
            }
            else
            {
                DebugMod.LogConsole("Invalid name for savestate");
            }
        }
    }

    public static void SwapFileStates(int page1, int index1, int page2, int index2)
    {
        SaveState state1 = GetFileState(page1, index1);
        SaveState state2 = GetFileState(page2, index2);

        fileStates[page1][index1] = state2;
        fileStates[page2][index2] = state1;
    }

    public static void AddPage(int page)
    {
        SaveState[] array = new SaveState[STATES_PER_PAGE];

        for (int i = 0; i < array.Length; i++)
        {
            array[i] = new SaveState();
        }

        for (int i = fileStates.Count - 1; i >= page; i--)
        {
            Directory.Move(GetPagePath(i), GetPagePath(i + 1));
        }

        Directory.CreateDirectory(GetPagePath(page));

        fileStates.Insert(page, array);
    }

    public static bool RemovePage(int page, bool force)
    {
        if (fileStates.Count > 1)
        {
            SaveState[] array = fileStates[page];
            string path = GetPagePath(page);

            if (array.Any(x => x.IsSet()))
            {
                if (force)
                {
                    Directory.Delete(path, recursive: true);
                    fileStates.Remove(array);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                Directory.Delete(path);
                fileStates.Remove(array);
            }
        }

        for (int i = page; i < fileStates.Count; i++)
        {
            Directory.Move(GetPagePath(i + 1), GetPagePath(i));
        }

        return true;
    }

    public static void SwapPages(int a, int b)
    {
        if (a >= 0 && a < fileStates.Count && b >= 0 && b < fileStates.Count && a != b)
        {
            string tempPath = Path.Combine(saveStatesBaseDirectory, "Temp");
            Directory.Move(GetPagePath(a), tempPath);
            Directory.Move(GetPagePath(b), GetPagePath(a));
            Directory.Move(tempPath, GetPagePath(b));

            (fileStates[a], fileStates[b]) = (fileStates[b], fileStates[a]);
        }
    }

    private static string GetPagePath(int page)
    {
        return Path.Combine(saveStatesBaseDirectory, page.ToString());
    }

    private static string GetSavestatePath(int page, int index)
    {
        return Path.Combine(GetPagePath(page), $"savestate{index}.json");
    }

    #endregion

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

    private static void SaveToFile(int page, int index)
    {
        try
        {
            SaveState state = fileStates[page][index];
            string filePath = GetSavestatePath(page, index);

            if (state.IsSet())
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                state.data.BeforeSerialize();
                File.WriteAllText(filePath, JsonUtility.ToJson(state.data, prettyPrint: true));
            }
            else if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (Exception ex)
        {
            DebugMod.LogDebug(ex.Message);
            throw;
        }
    }

    #endregion

    #region loading

    public static void LoadState(SaveState state)
    {
        bool shouldLoad = LoadLockout();
        if (!shouldLoad && DebugMod.overrideLoadLockout)
        {
            DebugMod.LogConsole("Overriding savestate lockout");
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

            LoadAllSavestates();
            RefreshSavestatePacks();
        }
        catch (Exception ex)
        {
            DebugMod.LogError(ex.ToString());
        }
    }

    private static void LoadAllSavestates()
    {
        fileStates.Clear();
        Directory.CreateDirectory(saveStatesBaseDirectory);

        foreach (string pageDirectory in Directory.EnumerateDirectories(saveStatesBaseDirectory).OrderBy(x => x))
        {
            Match pageMatch = Regex.Match(Path.GetFileName(pageDirectory), pageDirectoryPattern);
            if (!pageMatch.Success)
            {
                continue;
            }

            int page = int.Parse(pageMatch.Groups[1].Value);

            while (fileStates.Count <= page)
            {
                AddPage(fileStates.Count);
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
            AddPage(0);
        }
    }

    public static void RefreshSavestatePacks()
    {
        packNames.Clear();
        Directory.CreateDirectory(packsBaseDirectory);

        foreach (string path in Directory.EnumerateFileSystemEntries(packsBaseDirectory, "*.zip").OrderBy(x => x))
        {
            packNames.Add(Path.GetFileNameWithoutExtension(path));
        }
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

    public static void ImportPack(string name)
    {
        string packPath = GetPackPath(name);

        if (!packNames.Contains(name))
        {
            return;
        }

        if (!File.Exists(packPath))
        {
            packNames.Remove(name);
            return;
        }

        BackupSavestates();

        Directory.Delete(saveStatesBaseDirectory, recursive: true);

        try
        {
            ZipFile.ExtractToDirectory(GetPackPath(name), saveStatesBaseDirectory);
        }
        catch (Exception e)
        {
            DebugMod.LogConsole("Error extracting pack zip");
            DebugMod.LogError(e.ToString());
        }

        LoadAllSavestates();

        DebugMod.settings.LastLoadedPack = name;
        PackChanged?.Invoke();

        DebugMod.LogConsole($"Imported pack {name}");
    }

    private static void BackupSavestates()
    {
        string savestatesName = DebugMod.settings.LastLoadedPack;
        if (string.IsNullOrEmpty(savestatesName)) savestatesName = "Savestates";

        string path = Path.Combine(backupsDirectory, $"{savestatesName} {DateTime.Now:yy-MM-dd-HH-mm-ss}.zip");

        Directory.CreateDirectory(backupsDirectory);
        File.Delete(path);
        ZipFile.CreateFromDirectory(saveStatesBaseDirectory, path);

        DirectoryInfo directory = new(backupsDirectory);
        List<FileInfo> files = directory.EnumerateFiles().OrderBy(x => x.CreationTime).ToList();

        for (int i = 0; i < files.Count - 20; i++)
        {
            files[i].Delete();
        }
    }

    public static void ExportPack(string name)
    {
        RefreshSavestatePacks();

        if (ValidateNewPackName(name) != "")
        {
            return;
        }

        string packPath = GetPackPath(name);

        File.Delete(packPath);
        ZipFile.CreateFromDirectory(saveStatesBaseDirectory, packPath);

        if (!packNames.Contains(name))
        {
            packNames.Add(name);
            packNames.Sort();
        }

        DebugMod.LogConsole($"Exported pack {name}");
    }

    public static string ValidateNewPackName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return "SAVESTATES_ERRORPACKNAMEEMPTY";
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

    private static string GetPackPath(string name)
    {
        return Path.Combine(packsBaseDirectory, $"{name}.zip");
    }
    #endregion
}