namespace DebugMod;

public partial class GUIController
{
    partial void GameActiveUpdate()
    {
        if (DebugMod.infiniteTools && ToolItemManager.Instance && ToolItemManager.Instance.toolItems)
        {
            foreach (ToolItem tool in ToolItemManager.Instance.toolItems)
            {
                if (tool)
                {
                    ToolItemsData.Data data = tool.SavedData;
                    int oldAmount = data.AmountLeft;
                    data.AmountLeft = ToolItemManager.GetToolStorageAmount(tool);
                    tool.SavedData = data;

                    AttackToolBinding? binding = ToolItemManager.GetAttackToolBinding(tool);
                    if (binding.HasValue && oldAmount != data.AmountLeft)
                    {
                        ToolItemManager.ReportBoundAttackToolUpdated(binding.Value);
                    }
                }
            }
        }
    }
}
