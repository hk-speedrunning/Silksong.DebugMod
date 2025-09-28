namespace DebugMod
{
    public static partial class BindableFunctions
    {
        [BindableMethod(name = "Unlock All Tools", category = "Tools")]
        public static void UnlockAllTools()
        {
            ToolItemManager.UnlockAllTools();
        }

        [BindableMethod(name = "Unlock All Crests", category = "Tools")]
        public static void UnlockAllCrests()
        {
            ToolItemManager.UnlockAllCrests();
        }

        [BindableMethod(name = "Increment Tool Pouch", category = "Tools")]
        public static void IncrementPouches()
        {
            var i = ++PlayerData.instance.ToolPouchUpgrades;
            if (i > 4) PlayerData.instance.ToolPouchUpgrades = 0;
        }

        [BindableMethod(name = "Increment Crafting Kit", category = "Tools")]
        public static void IncrementKits()
        {
            var i = ++PlayerData.instance.ToolKitUpgrades;
            if (i > 4) PlayerData.instance.ToolKitUpgrades = 0;
        }


        [BindableMethod(name = "Craft Tools", category = "Tools")]
        public static void CraftTools()
        {
            ToolItemManager.TryReplenishTools(true, ToolItemManager.ReplenishMethod.Bench);
        }
    }
}