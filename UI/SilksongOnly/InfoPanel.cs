using GlobalSettings;

namespace DebugMod.UI;

public partial class InfoPanel
{
    private static partial string GameSpecificKeys()
    {
        return DebugMod.IH.inputActions.Taunt.State ? "t" : "  ";
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
}
