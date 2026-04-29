namespace DebugMod;

public static partial class BindableFunctions
{
    [BindableMethod(name = "Give Rosaries", category = "Consumables")]
    public static void GiveRosaries()
    {
        HeroController.instance.AddGeo(1000);
        DebugMod.LogConsole("Giving player 1000 rosaries");
    }

    [BindableMethod(name = "Give Shell Shards", category = "Consumables")]
    public static void GiveShellShards()
    {
        HeroController.instance.AddShards(100);
        DebugMod.LogConsole("Giving player 100 shell shards");
    }

    [BindableMethod(name = "Give Quest Items", category = "Consumables")]
    public static void GiveQuestItems()
    {
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
                    }
                }
            }
        }
    }
}