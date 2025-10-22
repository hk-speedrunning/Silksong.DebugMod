﻿namespace DebugMod;

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

    private static void SetCollectable(string name, int amount)
    {
        if (CollectableItemManager.IsInHiddenMode())
        {
            CollectableItemManager.Instance.AffectItemData(name, (ref CollectableItemsData.Data data) => data.AmountWhileHidden = amount);
        }
        else
        {
            CollectableItemManager.Instance.AffectItemData(name, (ref CollectableItemsData.Data data) => data.Amount = amount);
        }
    }

    [BindableMethod(name = "Give Memory Lockets", category = "Consumables")]
    public static void GiveMemoryLockets()
    {
        SetCollectable("Crest Socket Unlocker", 20);
        DebugMod.LogConsole("Set player memory lockets to 20");
    }

    [BindableMethod(name = "Give Craftmetal", category = "Consumables")]
    public static void GiveCraftmetal()
    {
        SetCollectable("Tool Metal", 8);
        DebugMod.LogConsole("Set player craftmetal to 8");
    }

    [BindableMethod(name = "Give Silkeater", category = "Consumables")]
    public static void GiveSilkeater()
    {
        CollectableItemManager.GetItemByName("Silk Grub").AddAmount(1);
        DebugMod.LogConsole("Giving player a silkeater");
    }

    // TODO: add bind to give all items needed for the active quest(s)
}