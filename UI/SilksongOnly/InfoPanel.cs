using DebugMod.Helpers;
using GlobalSettings;
using System.Linq;
using Object = UnityEngine.Object;

namespace DebugMod.UI;

public partial class InfoPanel
{
    private static partial string GameSpecificKeys()
    {
        return DebugMod.IH.inputActions.Taunt.State ? "t" : "  ";
    }

    private static string GetScaling()
    {
        if (DebugMod.lastScaling == null) return Localization.Get("INFOPANEL_NONE");

        string[] scaleMultipliers =
        [
            $"{DebugMod.lastScaling.Level1Mult:.###}",
            $"{DebugMod.lastScaling.Level2Mult:.###}",
            $"{DebugMod.lastScaling.Level3Mult:.###}",
            $"{DebugMod.lastScaling.Level4Mult:.###}",
            $"{DebugMod.lastScaling.Level5Mult:.###}"
        ];

        // Level from HealthManager.ApplyDamageScaling
        int level = DebugMod.lastScaleLevel;

        if (level > 4) level = 4;
        if (level >= 0) // Game behaviour; sub-zero levels scale to 1f so don't highlight any multiplier
            scaleMultipliers[level] = $"({scaleMultipliers[level]})";

        return string.Join("∶", scaleMultipliers);
    }

    partial void AppendProgressInfo()
    {
        AppendInfo("INFOPANEL_FLEAS", () => $"{Gameplay.FleasCollectedCount} / 30");
        AppendInfo("INFOPANEL_QUESTPOINTS", GetQuestPoints);
    }

    private static string GetQuestPoints()
    {
        QuestCompleteTotalGroup group = QuestManager.GetQuest("Soul Snare Pre").requiredCompleteTotalGroups[0];
        return $"{group.CurrentValueCount} / {group.target}";
    }

    private static partial string GetRoomSpecificText()
    {
        string text = "";

        MazeController mazeController = Object.FindAnyObjectByType<MazeController>();
        if (mazeController && mazeController.gameObject.scene.name != "Dust_09")
        {
            text += $"{Localization.Get("INFOPANEL_MISTSCORE")}: {PlayerData.instance.CorrectMazeDoorsEntered}\n";
            text += $"{Localization.Get("INFOPANEL_MISTCORRECTEXIT")}: {string.Join(", ",
                mazeController.correctDoors.Select(gate => gate.name))}\n";
        }

        text = text.TrimEnd('\n');
        return text;
    }
}
