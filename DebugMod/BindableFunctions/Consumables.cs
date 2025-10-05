namespace DebugMod
{
    public static partial class BindableFunctions
    {
        [BindableMethod(name = "Give Rosaries", category = "Consumables")]
        public static void GiveRosaries()
        {
            HeroController.instance.AddGeo(1000);
            Console.AddLine("Giving player 1000 rosaries");
        }

        [BindableMethod(name = "Give Shell Shards", category = "Consumables")]
        public static void GiveShellShards()
        {
            HeroController.instance.AddShards(100);
            Console.AddLine("Giving player 100 shell shards");
        }

        [BindableMethod(name = "Give Memory Lockets", category = "Consumables")]
        public static void GiveMemoryLockets()
        {
            CollectableItemManager.GetItemByName("Crest Socket Unlocker").AddAmount(10);
            Console.AddLine("Giving player 10 memory lockets");
        }

        [BindableMethod(name = "Give Craftmetal", category = "Consumables")]
        public static void GiveCraftmetal()
        {
            CollectableItemManager.GetItemByName("Tool Metal").AddAmount(8);
            Console.AddLine("Giving player 8 craftmetal");
        }

        [BindableMethod(name = "Give Silkeater", category = "Consumables")]
        public static void GiveSilkeater()
        {
            CollectableItemManager.GetItemByName("Silk Grub").AddAmount(1);
            Console.AddLine("Giving player a silkeater");
        }

        // TODO: add bind to give all items needed for the active quest(s)
    }
}