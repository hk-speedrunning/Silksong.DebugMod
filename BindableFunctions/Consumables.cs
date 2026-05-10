namespace DebugMod;

public static partial class BindableFunctions
{
    [BindableMethod(name = "CONSUMABLES_GIVEROSARIES", category = "CATEGORY_CONSUMABLES")]
    public static void GiveRosaries()
    {
        HeroController.instance.AddGeo(1000);
        DebugMod.LogConsole("Giving player 1000 rosaries");
    }

    [BindableMethod(name = "CONSUMABLES_GIVESHELLSHARDS", category = "CATEGORY_CONSUMABLES")]
    public static void GiveShellShards()
    {
        HeroController.instance.AddShards(100);
        DebugMod.LogConsole("Giving player 100 shell shards");
    }

    [BindableMethod(name = "CONSUMABLES_GIVEQUESTITEMS", category = "CATEGORY_CONSUMABLES")]
    public static void GiveQuestItems()
    {
        bool didSomething = false;

        foreach (FullQuestBase quest in QuestManager.GetActiveQuests())
        {
            foreach (FullQuestBase.QuestTarget target in quest.Targets)
            {
                if (target.Counter is CollectableItem item)
                {
                    int amountToGive = target.Count - item.CollectedAmount;
                    if (amountToGive > 0)
                    {
                        item.Collect(amountToGive);
                        didSomething = true;
                    }
                }
            }
        }

        if (didSomething)
        {
            DebugMod.LogConsole("Gave necessary items for all active quests");
        }
        else
        {
            DebugMod.LogConsole("No active quests that need items, doing nothing");
        }
    }
}