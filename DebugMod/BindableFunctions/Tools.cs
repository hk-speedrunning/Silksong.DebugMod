namespace DebugMod
{
    public static partial class BindableFunctions
    {
        [BindableMethod(name = "Unlock All Tools", category = "Tools")]
        public static void UnlockAllTools()
        {
            ToolItemManager.UnlockAllTools();
            Console.AddLine("Unlocked all tools");
        }

        [BindableMethod(name = "Unlock All Crests", category = "Tools")]
        public static void UnlockAllCrests()
        {
            ToolItemManager.UnlockAllCrests();
            Console.AddLine("Unlocked all crests");
        }

        [BindableMethod(name = "Increment Tool Pouch", category = "Tools")]
        public static void IncrementPouches()
        {
            if (PlayerData.instance.ToolPouchUpgrades < 4)
            {
                PlayerData.instance.ToolPouchUpgrades++;
                Console.AddLine($"Increasing tool pouch level (now {PlayerData.instance.ToolPouchUpgrades})");
            }
            else
            {
                PlayerData.instance.ToolPouchUpgrades = 0;
                Console.AddLine("Resetting tool pouch level");
            }
        }

        [BindableMethod(name = "Increment Crafting Kit", category = "Tools")]
        public static void IncrementKits()
        {
            if (PlayerData.instance.ToolKitUpgrades < 4)
            {
                PlayerData.instance.ToolKitUpgrades++;
                Console.AddLine($"Increasing crafting kit level (now {PlayerData.instance.ToolKitUpgrades})");
            }
            else
            {
                PlayerData.instance.ToolKitUpgrades = 0;
                Console.AddLine("Resetting crafting kit level");
            }
        }

        [BindableMethod(name = "Infinite Tools", category = "Tools")]
        public static void ToggleInfiniteTools()
        {
            DebugMod.infiniteTools = !DebugMod.infiniteTools;
            Console.AddLine("Infinite Tools set to " + DebugMod.infiniteTools.ToString().ToUpper());
        }

        [BindableMethod(name = "Craft Tools", category = "Tools")]
        public static void CraftTools()
        {
            ToolItemManager.TryReplenishTools(true, ToolItemManager.ReplenishMethod.Bench);
            Console.AddLine("Crafted new tools");
        }
    }
}